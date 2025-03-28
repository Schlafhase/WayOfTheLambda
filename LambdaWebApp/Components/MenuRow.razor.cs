﻿using Microsoft.AspNetCore.Components;

namespace LambdaWebApp.Components;

public partial class MenuRow : ComponentBase
{
	[Parameter]
	public RenderFragment ChildContent { get; set; }
	
	protected override void OnInitialized()
	{
		Console.WriteLine("MenuRow.OnInitialized");
	}
}