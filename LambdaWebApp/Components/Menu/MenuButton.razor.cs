using Microsoft.AspNetCore.Components;

namespace LambdaWebApp.Components.Menu;

public partial class MenuButton : ComponentBase
{
	[Parameter]
	public string Name { get; set; }
	
	[Parameter]
	public Func<Task>? OnClickAsync { get; set; }
}