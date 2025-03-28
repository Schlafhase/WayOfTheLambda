using LambdaWebApp.Components.Editor;
using Microsoft.AspNetCore.Components;

namespace LambdaWebApp.Layout;

public partial class EditorLayout : LayoutComponentBase
{
	private TextEditor? _textEditor;

	protected override void OnAfterRender(bool firstRender)
	{
		if (firstRender)
		{
			_textEditor.Text = "λx.x";
		}
	}
}