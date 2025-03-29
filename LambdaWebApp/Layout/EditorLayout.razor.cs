using LambdaCalculus;
using LambdaWebApp.Components;
using LambdaWebApp.Components.Editor;
using Microsoft.AspNetCore.Components;

namespace LambdaWebApp.Layout;

public partial class EditorLayout : LayoutComponentBase
{
	private TextEditor _textEditor = null!;
	private TextEditor _errorMessages = null!;
	private DiagramView _diagramView = null!;
	private LambdaExpression _currentExpression = LambdaExpression.Parse("λx.x");

	protected override void OnAfterRender(bool firstRender)
	{
		if (firstRender && _textEditor is not null)
		{
			_textEditor.Text = "λx.x";
			
			_textEditor.TextChanged += () =>
			{
				try
				{
					_currentExpression = LambdaExpression.Parse(_textEditor.Text);
				}
				catch (InvalidTermException e)
				{
					_errorMessages.Text = e.Message + " At character " + e.Index;
					StateHasChanged();
					return;
				}
				catch (Exception e)
				{
					_errorMessages.Text = e.Message;
					StateHasChanged();
					return;
				}
				
				_diagramView.LambdaExpression = _currentExpression;
				_errorMessages.Text = "";
				StateHasChanged();
			};
		}
	}
}