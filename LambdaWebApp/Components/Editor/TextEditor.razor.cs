using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LambdaWebApp.Components.Editor;

public partial class TextEditor : ComponentBase
{
	[Inject] private AppSettings _settings { get; set; } = null!;

	private ElementReference _editor;

	private List<string> _lines = [""];
	private int _cursorLine = 0;
	private int _cursorColumn = 0;
	private int _previousCursorColumn = 0; // Keep the cursor in the same column when moving up or down
	// TODO: indentation preservation
	
	private bool _idling = false;
	private Timer _idleTimer;
	private DateTime _lastInput = DateTime.MinValue;

	private float characterHeight => _settings.EditorFontSize * 1.4f;
	private float characterWidth => _settings.EditorFontSize * AppSettings.EditorFontSizeRatio;

	public string Text
	{
		get => string.Join('\n', _lines);
		set => _lines = value.Split('\n').ToList();
	}

	private int cursorColumn
	{
		get => _cursorColumn;
		set
		{
			_cursorColumn = value;
			_previousCursorColumn = _cursorColumn;
		}
	}
	
	private void stopIdling()
	{
		_lastInput = DateTime.Now;
		_idling = false;
		StateHasChanged();
	}

	public TextEditor()
	{
		_idleTimer = new Timer(_ =>
		{
			if (_idling || _lastInput.AddMilliseconds(500) >= DateTime.Now)
			{
				return;
			}

			_idling = true;
			StateHasChanged();
		}, null, 0, 10);
	}

	private List<Token> tokenise()
	{
		string currentLine = _lines[_cursorLine];

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

				if (columnIndex == cursorColumn && lineIndex == _cursorLine)
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

			if (cursorColumn == line.Length && lineIndex == _cursorLine)
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



		return tokens;
	}


	// TODO: Selections
	private void onEditorClick(MouseEventArgs e)
	{
		int x = (int)Math.Round(e.OffsetX / characterWidth);
		int y = (int)(e.OffsetY / characterHeight);

		_cursorLine = Math.Clamp(y, 0, _lines.Count - 1);
		cursorColumn = Math.Clamp(x, 0, _lines[_cursorLine].Length);
		stopIdling();
		StateHasChanged();
	}

	private void onEditorKeyDown(KeyboardEventArgs e)
	{
		stopIdling();
		
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
					moveCursorLeft();
					return;
				}
				
				cursorColumn = endOfWordLeft;
				return;
			case "ArrowRight" when ctrl:
				int endOfWordRight = findEndOfWord(out int lineOffsetRight, true);
				
				if (lineOffsetRight == 1)
				{
					moveCursorRight();
					return;
				}
				
				cursorColumn = endOfWordRight;
				return;
			case "ArrowLeft":
				moveCursorLeft();
				return;
			case "ArrowRight":
				moveCursorRight();
				return;
			case "ArrowUp":
				moveCursorUp();
				return;
			case "ArrowDown":
				moveCursorDown();
				return;
			case "Backspace" when ctrl:
				int endOfWordBackspace = findEndOfWord(out int lineOffsetBackspace);

				if (lineOffsetBackspace == -1)
				{
					removeSingleCharacter();
					return;
				}
				
				_lines[_cursorLine] = _lines[_cursorLine].Remove(endOfWordBackspace, cursorColumn - endOfWordBackspace);
				cursorColumn = endOfWordBackspace;
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
				
				_lines[_cursorLine] = _lines[_cursorLine].Remove(cursorColumn, endOfWordDelete - cursorColumn);
				return;
			case "Delete":
				removeSingleCharacter(true);
				return;
			case "Enter":
				_lines.Insert(_cursorLine + 1, _lines[_cursorLine][cursorColumn..]);
				_lines[_cursorLine] = _lines[_cursorLine].Remove(cursorColumn, _lines[_cursorLine].Length - cursorColumn);
				_cursorLine++;
				cursorColumn = 0;
				return;
			case "Tab":
				_lines[_cursorLine] = _lines[_cursorLine].Insert(cursorColumn, "    ");
				cursorColumn += 4;
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

		_lines[_cursorLine] = _lines[_cursorLine].Insert(cursorColumn, character);
		cursorColumn++;
	}
	
	private void moveCursorLeft()
	{
		if (cursorColumn == 0)
		{
			if (_cursorLine == 0)
			{
				cursorColumn = 0;
				return;
			}

			_cursorLine--;
			cursorColumn = _lines[_cursorLine].Length;
		}
		else
		{
			cursorColumn--;
		}
		stopIdling();
	}
	
	private void moveCursorRight()
	{
		if (cursorColumn == _lines[_cursorLine].Length)
		{
			if (_cursorLine == _lines.Count - 1)
			{
				cursorColumn = _lines[_cursorLine].Length;
				return;
			}

			_cursorLine++;
			cursorColumn = 0;
		}
		else
		{
			cursorColumn++;
		}
		stopIdling();
	}
	
	private void moveCursorUp()
	{
		if (_cursorLine == 0)
		{
			cursorColumn = 0;
			return;
		}

		_cursorLine--;
		
		// Change cursor column without changing _previousCursorColumn
		_cursorColumn = Math.Min(_previousCursorColumn, _lines[_cursorLine].Length);
		stopIdling();
	}
	
	private void moveCursorDown()
	{
		if (_cursorLine == _lines.Count - 1)
		{
			cursorColumn = _lines[_cursorLine].Length;
			return;
		}

		_cursorLine++;
		
		// Change cursor column without changing _previousCursorColumn
		_cursorColumn = Math.Min(_previousCursorColumn, _lines[_cursorLine].Length);
		stopIdling();
	}

	private void removeSingleCharacter(bool inFront = false)
	{
		
		if (!inFront)
		{
			if (cursorColumn == 0)
			{
				if (_cursorLine == 0)
				{
					return;
				}
				
				int previousLineLength = _lines[_cursorLine].Length;
				
				_lines[_cursorLine - 1] += _lines[_cursorLine];
				_lines.RemoveAt(_cursorLine);
				_cursorLine--;
				cursorColumn = _lines[_cursorLine].Length - previousLineLength;
			}
			else
			{
				_lines[_cursorLine] = _lines[_cursorLine].Remove(cursorColumn - 1, 1);
				cursorColumn--;
			}
		}
		else
		{
			if (cursorColumn == _lines[_cursorLine].Length)
			{
				if (_cursorLine == _lines.Count - 1)
				{
					return;
				}

				_lines[_cursorLine] += _lines[_cursorLine + 1];
				_lines.RemoveAt(_cursorLine + 1);
			}
			else
			{
				_lines[_cursorLine] = _lines[_cursorLine].Remove(cursorColumn, 1);
			}
		}
		StateHasChanged();
	}

	private int findEndOfWord(out int lineOffset, bool forward = false)
	{
		lineOffset = 0;
		
		switch (forward)
		{
			case true when cursorColumn == _lines[_cursorLine].Length:
				lineOffset = _cursorLine == _lines.Count - 1 ? 0 : 1;
				return cursorColumn;
			case false when cursorColumn == 0:
				lineOffset = _cursorLine == 0 ? 0 : -1;
				return lineOffset == 0 ? 0 : _lines[_cursorLine + lineOffset].Length;
		}
		int i = cursorColumn - (forward ? 0 : 1);
		
		if (i is 0 && char.IsWhiteSpace(_lines[_cursorLine][i]) && !forward)
		{
			return 0;
		}
		
		// Skip whitespace
		if (!forward)
		{
			while (i > 0 && char.IsWhiteSpace(_lines[_cursorLine][i]))
			{
				i--;

				if (i != 0)
				{
					continue;
				}

				char.IsWhiteSpace(_lines[_cursorLine][i]);
				return 0;
			}
		}

		WordType currentType = _lines[_cursorLine][i] switch
		{
			'λ' or '\\' or '.' => WordType.Lambda,
			'(' or ')' => WordType.Parenthesis,
			' ' when forward => WordType.Whitespace,
			_ => WordType.Default
		};

		
		for (; i >= 0 && i < _lines[_cursorLine].Length; i += forward ? 1 : -1)
		{
			char c = _lines[_cursorLine][i];
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
}