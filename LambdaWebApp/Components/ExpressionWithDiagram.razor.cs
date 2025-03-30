using LambdaCalculus;
using Microsoft.AspNetCore.Components;

namespace LambdaWebApp.Components;

public partial class ExpressionWithDiagram : ComponentBase
{
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

	private DiagramView _diagramView = null!;
	private LambdaExpression _lambdaExpression = LambdaExpression.Parse("λx.x");
}