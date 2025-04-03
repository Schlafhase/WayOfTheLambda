using Microsoft.AspNetCore.Components;

namespace LambdaWebApp.Components.Menu;

public partial class MenuLink : ComponentBase
{
	[Parameter] public string Name { get; set; }

	[Parameter] public Action? OnClick { get; set; }

	[Parameter] public string? Href { get; set; }
}