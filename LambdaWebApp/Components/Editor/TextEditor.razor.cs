using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LambdaWebApp.Components.Editor;

public partial class TextEditor : ComponentBase
{
	[Inject] private AppSettings _settings { get; set; } = null!;

	private ElementReference _editor;

	private string _text = "";
	private int _cursorPosition = 0;
	private int _cursorColumn = 0;
	private bool _idling = false;
	private Timer _idleTimer;
	private DateTime _lastInput = DateTime.MinValue;

	private float characterHeight => _settings.EditorFontSize * 1.4f;
	private float characterWidth => _settings.EditorFontSize * AppSettings.EditorFontSizeRatio;

	public string Text
	{
		get => _text;
		set
		{
			_text = value;
			_lastInput = DateTime.Now;
			_cursorColumn = getCursorColumn();
			StateHasChanged();
		}
	}

	private int CursorPosition
	{
		get => _cursorPosition;
		set
		{
			_cursorPosition = value;
			_lastInput = DateTime.Now;
			StateHasChanged();
		}
	}

	private DateTime LastInput
	{
		get => _lastInput;
		set
		{
			_lastInput = value; 
			_idling = false;
		}
	}

	public TextEditor()
	{
		_idleTimer = new Timer(_ =>
		{
			if (_idling || _lastInput.AddSeconds(1) >= DateTime.Now)
			{
				return;
			}

			_idling = true;
			StateHasChanged();
		}, null, 0, 10);
	}
	
	private List<Token> tokenise(string text)
	{
		if (CursorPosition > text.Length)
		{
			CursorPosition = text.Length;
		}

		List<Token> tokens = [];

		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];

			if (i == CursorPosition)
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
				case '\n':
					tokens.Add(new Token
					{
						Value = "",
						Type = TokenType.Newline
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

		if (CursorPosition == text.Length)
		{
			tokens.Add(new Token
			{
				Value = "",
				Type = TokenType.Cursor
			});
		}


		return tokens;
	}


	// TODO: Selections
	private void onEditorClick(MouseEventArgs e)
	{
		int x = (int)Math.Round(e.OffsetX / characterWidth);
		int y = (int)(e.OffsetY / characterHeight);
		
		string[] lines = Text.Split('\n');
		
		int line = Math.Clamp(y, 0, lines.Length - 1);
		int column = Math.Clamp(x, 0, lines[line].Length);
		
		int cursor = 0;
		for (int i = 0; i < line; i++)
		{
			cursor += lines[i].Length + 1;
		}
		
		CursorPosition = cursor + column;
	}

	private async Task onEditorKeyDown(KeyboardEventArgs e)
	{
		bool ctrl = e.CtrlKey;

		if (e.AltKey)
		{
			return;
		}

		switch (e.Key)
		{
			case "ArrowLeft":
				CursorPosition = Math.Max(0, CursorPosition - 1);
				_cursorColumn = getCursorColumn();
				return;
			case "ArrowRight":
				CursorPosition = Math.Min(Text.Length, CursorPosition + 1);
				_cursorColumn = getCursorColumn();
				return;
			case "ArrowUp":
				CursorPosition = getTopIndex();
				return;
			case "ArrowDown":
				CursorPosition = getBottomIndex();
				return;
			case "Backspace" when ctrl:
				Text = controlRemove(Text, CursorPosition, out int removedBack);
				CursorPosition -= removedBack;
				return;
			case "Backspace":
				if (CursorPosition == 0)
				{
					return;
				}

				Text = Text.Remove(CursorPosition - 1, 1);
				CursorPosition--;
				return;
			case "Delete" when ctrl:
				Text = controlRemove(Text, CursorPosition, out _, true);
				return;
			case "Delete":
				if (CursorPosition == Text.Length)
				{
					return;
				}

				Text = Text.Remove(CursorPosition, 1);
				return;
			case "Enter":
				Text = Text.Insert(CursorPosition, "\n");
				CursorPosition++;
				_cursorColumn = 0;
				return;
			case "Tab":
				Text = Text.Insert(CursorPosition, "    ");
				CursorPosition += 4;
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

		Text = Text.Insert(CursorPosition, character);
		CursorPosition++;
	}

	// TODO: Fix cursor moving in weird ways
	private int getCursorColumn()
	{
		if (CursorPosition > Text.Length)
		{
			CursorPosition = Text.Length;
		}
		
		for (int i = CursorPosition - 1; i >= 0; i--)
		{
			if (Text[i] == '\n')
			{
				return CursorPosition - i - 1;
			}
		}
		return CursorPosition;
	}

	private int getBottomIndex()
	{
		string text = _text[CursorPosition..];
		
		if (!text.Contains('\n'))
		{
			CursorPosition = text.Length + CursorPosition;
			_cursorColumn = getCursorColumn();
			return CursorPosition;
		}
		
		string[] lines = text.Split('\n');
		string currentLine = lines[0];
		string bottomLine = lines[1];
		return Math.Min(bottomLine.Length, _cursorColumn) + currentLine.Length + CursorPosition + 1;
	}
	
	private int getTopIndex()
	{
		string text = _text[..CursorPosition];
		
		if (!text.Contains('\n'))
		{
			_cursorColumn = 0;
			return 0;
		}
		
		string[] lines = text.Split('\n');
		string topLine = lines[^2];
		return CursorPosition - _cursorColumn - (topLine.Length - Math.Min(topLine.Length, _cursorColumn)) - 1;
	}

	private static int controlFindIndex(string text, int index, bool forward = false)
	{
		char c;

		try
		{
			c = text[index - (forward ? 0 : 1)];
		}
		catch (IndexOutOfRangeException)
		{
			return -1;
		}

		bool removeWhitespace = char.IsWhiteSpace(c);

		int end = index - (forward ? 0 : 1);

		if (forward)
		{
			while (end < text.Length && char.IsWhiteSpace(text[end]) == removeWhitespace)
			{
				end++;
			}
		}
		else
		{
			while (end > 0 && char.IsWhiteSpace(text[end - 1]) == removeWhitespace)
			{
				end--;
			}
		}

		return end;
	}

	private static string controlRemove(string text, int index, out int removed, bool forward = false)
	{
		int end = controlFindIndex(text, index, forward);

		if (end == -1)
		{
			removed = 0;
			return text;
		}

		// end = forward ? end : end + 1;
		removed = Math.Abs(end - index);

		return end > index
			? text.Remove(index, end - index)
			: text.Remove(end, index - end);
	}
}