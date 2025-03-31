using LambdaCalculus;
using LambdaWebApp.Components;
using LambdaWebApp.Components.Editor;
using Microsoft.AspNetCore.Components;

namespace LambdaWebApp.Layout;

public partial class EditorLayout : LayoutComponentBase, IDisposable
{
	private TextEditor _textEditor = null!;
	private BetaReductionView _betaReductionView = null!;
	private DiagramView _diagramView = null!;
	private LambdaExpression _currentExpression = LambdaExpression.Parse("λx.x");
	
	private bool _dirty = false;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender && _textEditor is not null)
		{
			_textEditor.Text = "λx.x";
			await _betaReductionView.SetLambdaExpression(_currentExpression);
			_diagramView.LambdaExpression = _currentExpression;

			_textEditor.TextChanged += onTextChanged;
			_textEditor.OnStartIdlingAsync += onStartIdling;
		}
	}

	private void onTextChanged()
	{
		_dirty = true;
		
	}

	private async Task onStartIdling()
	{
		if (!_dirty)
		{
			return;
		}
		_dirty = false;
		
		try
		{
			_currentExpression = LambdaExpression.Parse(_textEditor.Text);
		}
		catch (InvalidTermException e)
		{
			_betaReductionView.ErrorMessage = e.Message + " At character " + e.Index;
			StateHasChanged();
			return;
		}
		catch (Exception e)
		{
			_betaReductionView.ErrorMessage = e.Message;
			StateHasChanged();
			return;
		}
		
		_diagramView.LambdaExpression = _currentExpression;
		_betaReductionView.ErrorMessage = "";
		
		await _betaReductionView.SetLambdaExpression(_currentExpression);
		StateHasChanged();
	}

	public void Dispose()
	{
		_textEditor.TextChanged -= onTextChanged;
		_textEditor.OnStartIdlingAsync -= onStartIdling;
	}
}