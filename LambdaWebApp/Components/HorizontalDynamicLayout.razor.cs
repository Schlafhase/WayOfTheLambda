using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LambdaWebApp.Components;

public partial class HorizontalDynamicLayout : ComponentBase
{
	private Guid _id = Guid.NewGuid();
	[Parameter] public RenderFragment? ChildContent { get; set; }
	[Parameter] public float firstChildWidth { get; set; } = 50;

	[Inject] private IJSRuntime _jsRuntime { get; set; } = null!;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			try
			{
				await _jsRuntime.InvokeVoidAsync("setWidth", _id.ToString(), firstChildWidth);
			}
			catch (InvalidOperationException)
			{
				// Ignore
			}
		}

		try
		{
			await _jsRuntime.InvokeVoidAsync("addSeparator", _id.ToString());
		}
		catch (InvalidOperationException)
		{
			// Ignore
		}
	}
}