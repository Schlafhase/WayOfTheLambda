using CurrieTechnologies.Razor.Clipboard;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace LambdaWebApp.Components.Editor;

public partial class TextEditor : ComponentBase
{
	[Inject] private AppSettings _settings { get; set; } = null!;
	[Inject] private IJSRuntime _jsRuntime { get; set; } = null!;
	[Inject] private ClipboardService _clipboard { get; set; } = null!;

	#region Properties
	private ElementReference _editor;

	private List<string> _lines = [""];
	private int _cursorLine = 0;
	private int _cursorColumn = 0;
	private int _previousCursorColumn = 0; // Keep the cursor in the same column when moving up or down
	// TODO: indentation preservation
	
	private (int line, int column) _selectionStart = (-1, -1);
	private (int line, int column) _selectionEnd = (-1, -1);
	
	private bool _idling = false;
	private Timer _idleTimer;
	private DateTime _lastInput = DateTime.MinValue;

	private float characterHeight => _settings.EditorFontSize * 1.4f;
	private float characterWidth => _settings.EditorFontSize * AppSettings.EditorFontSizeRatio;

	public string Text
	{
		get => string.Join('\n', _lines);
		set
		{
			_lines = value.Split('\n').ToList();
			StateHasChanged();
		}
	}

	public event Action? TextChanged;
	public event Func<Task>? OnStartIdlingAsync;

	private int cursorColumn
	{
		get => _cursorColumn;
		set
		{
			_cursorColumn = value;
			_previousCursorColumn = _cursorColumn;
		}
	}

	private int cursorLine
	{
		get => _cursorLine;
		set
		{
			_cursorLine = value;
		}
	}

	private async Task stopIdling()
	{
		_lastInput = DateTime.Now;
		_idling = false;
		StateHasChanged();
	}

	public TextEditor()
	{
		_idleTimer = new Timer(async _ =>
		{
			if (_idling || _lastInput.AddMilliseconds(500) >= DateTime.Now)
			{
				return;
			}

			_idling = true;
			
			if (OnStartIdlingAsync is not null)
			{
				await OnStartIdlingAsync();
			}
			
			StateHasChanged();
		}, null, 0, 10);
	}
	#endregion

	private List<Token> tokenise()
	{
		string currentLine = _lines[cursorLine];

		if (cursorColumn > currentLine.Length)
		{
			cursorColumn = currentLine.Length;
		}

		List<Token> tokens = [];

		for (int lineIndex = 0; lineIndex < _lines.Count; lineIndex++)
		{
			string line = _lines[lineIndex];

			for (int columnIndex = 0; columnIndex < line.Length; columnIndex++)
			{
				char c = line[columnIndex];

				if (columnIndex == cursorColumn && lineIndex == cursorLine)
				{
					tokens.Add(new Token
					{
						Value = "",
						Type = TokenType.Cursor
					});
				}

				switch (c)
				{
					case '\\' or 'λ' or '.':
						tokens.Add(new Token
						{
							Value = c is '\\' ? "λ" : c.ToString(),
							Type = TokenType.Lambda
						});
						continue;
					case '(' or ')':
						tokens.Add(new Token
						{
							Value = c.ToString(),
							Type = TokenType.Parenthesis
						});
						continue;
					default:
						switch (tokens.Count)
						{
							case > 0 when tokens[^1].Type == TokenType.Variable:
								tokens[^1] = new Token
								{
									Value = tokens[^1].Value + c,
									Type = TokenType.Variable
								};
								break;
							default:
								tokens.Add(new Token
								{
									Value = c.ToString(),
									Type = TokenType.Variable
								});
								break;
						}

						break;
				}
			}

			if (cursorColumn == line.Length && lineIndex == cursorLine)
			{
				tokens.Add(new Token
				{
					Value = "",
					Type = TokenType.Cursor
				});
			}
			
			if (lineIndex + 1 < _lines.Count)
			{
				tokens.Add(new Token
				{
					Value = "",
					Type = TokenType.Newline
				});
			}
		}
		
		try
		{
			_jsRuntime.InvokeVoidAsync("scrollToCursor", _cursorLine);
		}
		catch (InvalidOperationException)
		{
			// Ignore, this is most likely due to the component being disposed or not initialised yet
		}

		return tokens;
	}

	#region Cursor
	// TODO: Selections
	private async Task onEditorClick(MouseEventArgs e)
	{
		int x = (int)Math.Round(e.OffsetX / characterWidth);
		int y = (int)(e.OffsetY / characterHeight);

		cursorLine = Math.Clamp(y, 0, _lines.Count - 1);
		cursorColumn = Math.Clamp(x, 0, _lines[cursorLine].Length);
		await stopIdling();
		StateHasChanged();
	}
	#endregion

	#region Keyboard
	private async Task onEditorKeyDown(KeyboardEventArgs e)
	{
		await stopIdling();
		
		bool ctrl = e.CtrlKey;

		if (e.AltKey)
		{
			return;
		}

		switch (e.Key)
		{
			case "ArrowLeft" when ctrl:
				int endOfWordLeft = findEndOfWord(out int lineOffsetLeft);
				
				if (lineOffsetLeft == -1)
				{
					await moveCursorLeft();
					return;
				}
				
				cursorColumn = endOfWordLeft;
				return;
			case "ArrowRight" when ctrl:
				int endOfWordRight = findEndOfWord(out int lineOffsetRight, true);
				
				if (lineOffsetRight == 1)
				{
					await moveCursorRight();
					return;
				}
				
				cursorColumn = endOfWordRight;
				return;
			case "ArrowLeft":
				await moveCursorLeft();
				return;
			case "ArrowRight":
				await moveCursorRight();
				return;
			case "ArrowUp":
				await moveCursorUp();
				return;
			case "ArrowDown":
				await moveCursorDown();
				return;
			case "Backspace" when ctrl:
				int endOfWordBackspace = findEndOfWord(out int lineOffsetBackspace);

				if (lineOffsetBackspace == -1)
				{
					removeSingleCharacter();
					return;
				}
				
				_lines[cursorLine] = _lines[cursorLine].Remove(endOfWordBackspace, cursorColumn - endOfWordBackspace);
				cursorColumn = endOfWordBackspace;
				TextChanged?.Invoke();
				return;
			case "Backspace":
				removeSingleCharacter();
				return;
			case "Delete" when ctrl:
				int endOfWordDelete = findEndOfWord(out int lineOffsetDelete, true);
				
				if (lineOffsetDelete == 1)
				{
					removeSingleCharacter(true);
					return;
				}
				
				_lines[cursorLine] = _lines[cursorLine].Remove(cursorColumn, endOfWordDelete - cursorColumn);
				TextChanged?.Invoke();
				return;
			case "Delete":
				removeSingleCharacter(true);
				return;
			case "Enter":
				_lines.Insert(cursorLine + 1, _lines[cursorLine][cursorColumn..]);
				_lines[cursorLine] = _lines[cursorLine].Remove(cursorColumn, _lines[cursorLine].Length - cursorColumn);
				cursorLine++;
				cursorColumn = 0;
				TextChanged?.Invoke();
				return;
			case "Tab":
				_lines[cursorLine] = _lines[cursorLine].Insert(cursorColumn, "    ");
				cursorColumn += 4;
				TextChanged?.Invoke();
				return;
			case "v" when ctrl:
				// TODO: Handle pasting multiple lines
				string text = await _clipboard.ReadTextAsync();
				_lines[cursorLine] = _lines[cursorLine].Insert(cursorColumn, text);
				cursorColumn += text.Length;
				TextChanged?.Invoke();
				return;
		}

		if (ctrl)
		{
			return;
		}

		string character = e.Key;

		if (character.Length > 1)
		{
			return;
		}

		_lines[cursorLine] = _lines[cursorLine].Insert(cursorColumn, character);
		cursorColumn++;
		TextChanged?.Invoke();
	}
	
	private async Task moveCursorLeft()
	{
		if (cursorColumn == 0)
		{
			if (cursorLine == 0)
			{
				cursorColumn = 0;
				return;
			}

			cursorLine--;
			cursorColumn = _lines[cursorLine].Length;
		}
		else
		{
			cursorColumn--;
		}
		await stopIdling();
	}
	
	private async Task moveCursorRight()
	{
		if (cursorColumn == _lines[cursorLine].Length)
		{
			if (cursorLine == _lines.Count - 1)
			{
				cursorColumn = _lines[cursorLine].Length;
				return;
			}

			cursorLine++;
			cursorColumn = 0;
		}
		else
		{
			cursorColumn++;
		}
		await stopIdling();
	}
	
	private async Task moveCursorUp()
	{
		if (cursorLine == 0)
		{
			cursorColumn = 0;
			return;
		}

		cursorLine--;
		
		// Change cursor column without changing _previousCursorColumn
		_cursorColumn = Math.Min(_previousCursorColumn, _lines[cursorLine].Length);
		await stopIdling();
	}
	
	private async Task moveCursorDown()
	{
		if (cursorLine == _lines.Count - 1)
		{
			cursorColumn = _lines[cursorLine].Length;
			return;
		}

		cursorLine++;
		
		// Change cursor column without changing _previousCursorColumn
		_cursorColumn = Math.Min(_previousCursorColumn, _lines[cursorLine].Length);
		await stopIdling();
	}

	private void removeSingleCharacter(bool inFront = false)
	{
		
		if (!inFront)
		{
			if (cursorColumn == 0)
			{
				if (cursorLine == 0)
				{
					return;
				}
				
				int previousLineLength = _lines[cursorLine].Length;
				
				_lines[cursorLine - 1] += _lines[cursorLine];
				_lines.RemoveAt(cursorLine);
				cursorLine--;
				cursorColumn = _lines[cursorLine].Length - previousLineLength;
			}
			else
			{
				_lines[cursorLine] = _lines[cursorLine].Remove(cursorColumn - 1, 1);
				cursorColumn--;
			}
		}
		else
		{
			if (cursorColumn == _lines[cursorLine].Length)
			{
				if (cursorLine == _lines.Count - 1)
				{
					return;
				}

				_lines[cursorLine] += _lines[cursorLine + 1];
				_lines.RemoveAt(cursorLine + 1);
			}
			else
			{
				_lines[cursorLine] = _lines[cursorLine].Remove(cursorColumn, 1);
			}
		}
		TextChanged?.Invoke();
		StateHasChanged();
	}

	private int findEndOfWord(out int lineOffset, bool forward = false)
	{
		lineOffset = 0;
		
		switch (forward)
		{
			case true when cursorColumn == _lines[cursorLine].Length:
				lineOffset = cursorLine == _lines.Count - 1 ? 0 : 1;
				return cursorColumn;
			case false when cursorColumn == 0:
				lineOffset = cursorLine == 0 ? 0 : -1;
				return lineOffset == 0 ? 0 : _lines[cursorLine + lineOffset].Length;
		}
		int i = cursorColumn - (forward ? 0 : 1);
		
		if (i is 0 && char.IsWhiteSpace(_lines[cursorLine][i]) && !forward)
		{
			return 0;
		}
		
		// Skip whitespace
		if (!forward)
		{
			while (i > 0 && char.IsWhiteSpace(_lines[cursorLine][i]))
			{
				i--;

				if (i != 0)
				{
					continue;
				}

				return 0;
			}
		}

		WordType currentType = _lines[cursorLine][i] switch
		{
			'λ' or '\\' or '.' => WordType.Lambda,
			'(' or ')' => WordType.Parenthesis,
			' ' when forward => WordType.Whitespace,
			_ => WordType.Default
		};

		
		for (; i >= 0 && i < _lines[cursorLine].Length; i += forward ? 1 : -1)
		{
			char c = _lines[cursorLine][i];
			WordType type = c switch
			{
				'λ' or '\\' or '.' => WordType.Lambda,
				'(' or ')' => WordType.Parenthesis,
				' ' => WordType.Whitespace,
				_ => WordType.Default
			};

			if (type != currentType)
			{
				return i + (forward ? 0 : 1);
			}
		}
		return i + (forward ? 0 : 1);
	}

	private enum WordType
	{
		Lambda,
		Parenthesis,
		Whitespace,
		Default
	}
	#endregion
}