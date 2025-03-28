using Microsoft.AspNetCore.Components;

namespace LambdaWebApp.Components;

public partial class SubMenuButton : ComponentBase
{
	[Parameter]
	public string Name { get; set; }
	
	[Parameter]
	public Action? OnClick { get; set; }
}