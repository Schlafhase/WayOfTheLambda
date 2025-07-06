using LambdaCalculus;
using Microsoft.AspNetCore.Components;

namespace LambdaWebApp.Components;

public partial class BetaReductionView : ComponentBase
{
	private bool _betaNormal;
	private string _errorMessage = "";

	private LambdaExpression _lambdaExpression = LambdaExpression.Parse("λx.x");

	public List<LambdaExpression?> Steps { get; private set; } = [];

	public string ErrorMessage
	{
		get => _errorMessage;
		set
		{
			_errorMessage = value;
			StateHasChanged();
		}
	}

	public int MaxSteps { get; set; } = 100;
	// public LambdaExpression? LambdaExpression
	// {
	// 	get => _lambdaExpression;
	// 	set
	// 	{
	// 		if (value.Equals(_lambdaExpression))
	// 		{
	// 			return;
	// 		}
	// 		
	// 		_lambdaExpression = value;
	// 		_steps = getSteps(_lambdaExpression);
	// 		StateHasChanged();
	// 	}
	// }

	public async Task SetLambdaExpression(LambdaExpression lambdaExpression)
	{
		if (lambdaExpression.AlphaEquivalent(_lambdaExpression))
		{
			return;
		}

		_lambdaExpression = lambdaExpression;
		await loadSteps(_lambdaExpression);
	}

	public event Action? OnChange;


	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await loadSteps(_lambdaExpression);
		}
	}

	private async Task loadSteps(LambdaExpression lambdaExpression)
	{
		LambdaExpression? next = lambdaExpression.Clone();
		Steps = [];

		while (next is not null && Steps.Count < MaxSteps)
		{
			Steps.Add(next.Clone());
			next = next.BetaReduce();

			// if (_steps.Count % 10 != 0)
			// {
			// 	continue;
			// }
			//
			// await Task.Delay(100);
			// await InvokeAsync(StateHasChanged);
			// OnChange?.Invoke();
		}

		_betaNormal = next is null;
		OnChange?.Invoke();
		await InvokeAsync(StateHasChanged);
	}
}