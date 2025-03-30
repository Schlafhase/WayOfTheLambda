﻿using LambdaCalculus;
using Microsoft.AspNetCore.Components;

namespace LambdaWebApp.Components;

public partial class BetaReductionView : ComponentBase
{
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

	public string ErrorMessage
	{
		get => _errorMessage;
		set
		{
			_errorMessage = value;
			StateHasChanged();
		}
	}

	public event Action? OnChange;

	private LambdaExpression _lambdaExpression = LambdaExpression.Parse("λx.x");

	public int MaxSteps { get; set; } = 100;

	private List<LambdaExpression?> _steps = [];
	private bool _betaNormal;
	private string _errorMessage = "";


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
		_steps = [];

		while (next is not null && _steps.Count < MaxSteps)
		{
			_steps.Add(next.Clone());
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