using LambdaCalculus;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SkiaSharp;
using SkiaSharp.Views.Blazor;
using TrompDiagrams;
using TrompDiagrams.Rendering;

namespace LambdaWebApp.Components;

public partial class DiagramView : ComponentBase
{
	public LambdaExpression LambdaExpression
	{
		get => _lambdaExpression;
		set
		{
			_lambdaExpression = value;
			_geometry = LambdaExpressionRenderer.Render(value);

			try
			{
				_canvas.Invalidate();
			}
			catch (NullReferenceException)
			{
				// Ignore
			}
		}
	}

	private SKCanvasView _canvas;
	private Guid _id = Guid.NewGuid();

	[Parameter] public int? Width { get; set; } = null;
	[Parameter] public int? Height { get; set; } = null;

	private Geometry _geometry;
	private SKColor _lineColor = SKColor.FromHsl(52, 68, 54);

	private LambdaExpression _lambdaExpression = LambdaExpression.Parse("λx.x");

	[Parameter] public double Zoom { get; set; } = 10;
	[Parameter] public int CameraX { get; set; } = 10;
	[Parameter] public int CameraY { get; set; } = 10;

	private int _localCameraX => (int)(CameraX / Zoom);
	private int _localCameraY => (int)(CameraY / Zoom);

	private int getInLocalCoordinates(int x) => (int)(x / Zoom - _localCameraX);

	private bool _panning = false;
	private int _panStartX = 0;
	private int _panStartY = 0;
	private int _panStartCameraX = 0;
	private int _panStartCameraY = 0;


	public DiagramView()
	{
		LambdaExpression = LambdaExpression.Parse("λx.x");
	}

	// protected override async Task OnAfterRenderAsync(bool firstRender)
	// {
	// 	if (firstRender)
	// 	{
	// 		LambdaExpression = _lambdaExpression;
	// 	}
	// }

	private void onPaintSurface(SKPaintSurfaceEventArgs e)
	{
		e.Surface.Canvas.Clear(SKColors.Transparent);
		e.Surface.Canvas.Translate(CameraX, CameraY);
		e.Surface.Canvas.Scale((float)Zoom);

		foreach (Line line in _geometry.Lines)
		{
			int startX = 0;
			int startY = 0;
			int endX = 0;
			int endY = 0;

			switch (line)
			{
				case HorizontalLine hLine:
					startX = hLine.X;
					endX = hLine.X + hLine.Length;
					startY = hLine.Y;
					endY = hLine.Y;
					break;
				case VerticalLine vLine:
					startX = vLine.X;
					endX = vLine.X;
					startY = vLine.Y;
					endY = vLine.Y + vLine.Length;
					break;
			}

			startX *= 2;
			endX *= 2;
			startY *= 2;
			endY *= 2;

			switch (line)
			{
				case HorizontalLine:
					startX -= 1;
					endX -= 1;
					break;
				case VerticalLine:
					startY -= 1;
					endY -= 1;
					break;
			}

			e.Surface.Canvas.DrawLine(startX, startY, endX, endY, new SKPaint
			{
				StrokeWidth = 2,
				Color = _lineColor
			});
		}
	}

	private void onMouseDown(MouseEventArgs e)
	{
		_panStartX = (int)e.ClientX;
		_panStartY = (int)e.ClientY;
		_panStartCameraX = CameraX;
		_panStartCameraY = CameraY;
		_panning = true;
	}

	private void onMouseMove(MouseEventArgs e)
	{
		if (!_panning)
		{
			return;
		}

		CameraX = _panStartCameraX + (int)e.ClientX - _panStartX;
		CameraY = _panStartCameraY + (int)e.ClientY - _panStartY;

		_canvas.Invalidate();
	}

	private void onMouseUp(MouseEventArgs e)
	{
		_panning = false;
	}

	private void onMouseWheel(WheelEventArgs e)
	{
		double mouseX = e.OffsetX;
		double mouseY = e.OffsetY;

		double worldX = (mouseX - CameraX) / Zoom;
		double worldY = (mouseY - CameraY) / Zoom;

		Zoom -= e.DeltaY / 1000 * Zoom;

		double newScreenX = worldX * Zoom + CameraX;
		double newScreenY = worldY * Zoom + CameraY;

		CameraX += (int)(mouseX - newScreenX);
		CameraY += (int)(mouseY - newScreenY);

		_canvas.Invalidate();
	}
}