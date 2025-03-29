using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace LambdaWebApp.Components;

public partial class VerticalDynamicLayout : ComponentBase
{
	[Parameter] public RenderFragment? ChildContent { get; set; }
	[Parameter] public float firstChildHeight { get; set; } = 50;

	[Inject] private IJSRuntime _jsRuntime { get; set; } = null!;

	private Guid _id = Guid.NewGuid();

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			try
			{
				await _jsRuntime.InvokeVoidAsync("setHeight", _id.ToString(), firstChildHeight);
			}
			catch (InvalidOperationException)
			{
				// Ignore
			}
		}

		try
		{
			await _jsRuntime.InvokeVoidAsync("addSeparatorV", _id.ToString());
		}
		catch (InvalidOperationException)
		{
			// Ignore
		}
	}
}