using LambdaCalculus;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
		}
	}

	private SKCanvasView _canvas;
	
	private Geometry _geometry;
	private SKColor _lineColor = SKColor.FromHsl(52, 68, 54);
	
	private double _zoom = 10;
	private LambdaExpression _lambdaExpression = LambdaExpression.Parse("λx.x");
	
	private int _cameraX = 100;
	private int _cameraY = 100;

	private int _localCameraX => (int)(_cameraX/_zoom);
	private int _localCameraY => (int)(_cameraY/_zoom);
	
	private int getInLocalCoordinates(int x) => (int)(x/_zoom - _localCameraX);

	private bool _panning = false;
	private int _panStartX = 0;
	private int _panStartY = 0;
	private int _panStartCameraX = 0;
	private int _panStartCameraY = 0;
	

	public DiagramView()
	{
		LambdaExpression = LambdaExpression.Parse("λx.x");
	}

	private void onPaintSurface(SKPaintSurfaceEventArgs e)
	{
		e.Surface.Canvas.Clear(SKColors.Transparent);
		e.Surface.Canvas.Translate(_cameraX, _cameraY);
		e.Surface.Canvas.Scale((float)_zoom);
		
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
		_panStartCameraX = _cameraX;
		_panStartCameraY = _cameraY;
		_panning = true;
	}
	
	private void onMouseMove(MouseEventArgs e)
	{
		if (!_panning)
		{
			return;
		}

		_cameraX = _panStartCameraX + (int)e.ClientX - _panStartX;
		_cameraY = _panStartCameraY + (int)e.ClientY - _panStartY;
		
		Console.WriteLine(_localCameraX);
		Console.WriteLine(_localCameraY);
		Console.WriteLine();
	}
	
	private void onMouseUp(MouseEventArgs e)
	{
		_panning = false;
	}

	private void onMouseWheel(WheelEventArgs e)
	{
		double mouseX = e.OffsetX;
		double mouseY = e.OffsetY;
    
		double worldX = (mouseX - _cameraX) / _zoom;
		double worldY = (mouseY - _cameraY) / _zoom;
    
		_zoom -= e.DeltaY / 1000 * _zoom;
    
		double newScreenX = worldX * _zoom + _cameraX;
		double newScreenY = worldY * _zoom + _cameraY;
    
		_cameraX += (int)(mouseX - newScreenX);
		_cameraY += (int)(mouseY - newScreenY);
	}
}