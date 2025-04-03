using LambdaCalculus;
using Microsoft.AspNetCore.Components;

namespace LambdaWebApp.Components;

public partial class ExpressionWithDiagram : ComponentBase, IDisposable
{
	private DiagramView _diagramView = null!;
	private LambdaExpression _lambdaExpression = LambdaExpression.Parse("λx.x");
	private Action _updateDiagram = null!;

	public ExpressionWithDiagram()
	{
		_updateDiagram = () =>
		{
			try
			{
				_diagramView.LambdaExpression = _lambdaExpression;
			}
			catch (InvalidOperationException)
			{
				// Ignore
			}
			catch (NullReferenceException)
			{
				// Ignore
			}
		};
	}

	[Parameter]
	public LambdaExpression LambdaExpression
	{
		get => _lambdaExpression;
		set
		{
			_lambdaExpression = value;

			try
			{
				_diagramView.LambdaExpression = value;
			}
			catch (InvalidOperationException)
			{
				// Ignore
			}
			catch (NullReferenceException)
			{
				// Ignore
			}
		}
	}


	public void Dispose()
	{
		// Clock.OnTick -= _updateDiagram;
	}

	protected override void OnInitialized()
	{
		// Clock.OnTick += _updateDiagram; // TODO: make this more efficient
	}
}