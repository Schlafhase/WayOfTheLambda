using Microsoft.AspNetCore.Components;

namespace LambdaWebApp.Components;

public partial class MenuCategory : ComponentBase, IMenuItem
{
	[Parameter]
	public string Name { get; set; }
	[Parameter]
	public RenderFragment? ChildContent { get; set; }
}