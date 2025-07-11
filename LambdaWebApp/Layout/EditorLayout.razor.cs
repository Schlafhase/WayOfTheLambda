﻿using LambdaCalculus;
using LambdaWebApp.Components;
using LambdaWebApp.Components.Editor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using SkiaSharp;
using TrompDiagrams;
using TrompDiagrams.Rendering;

namespace LambdaWebApp.Layout;

public partial class EditorLayout : LayoutComponentBase, IDisposable
{
	private BetaReductionView _betaReductionView = null!;
	private LambdaExpression _currentExpression = LambdaExpression.Parse("λx.x");
	private DiagramView _diagramView = null!;

	private bool _dirty;

	private TextEditor _textEditor = null!;
	[Inject] private IJSRuntime _jsRuntime { get; set; }

	public void Dispose()
	{
		_textEditor.TextChanged -= onTextChanged;
		_textEditor.OnStartIdlingAsync -= onStartIdling;
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender && _textEditor is not null)
		{
			_textEditor.Text = "λx.x";
			await _betaReductionView.SetLambdaExpression(_currentExpression);
			_diagramView.LambdaExpression = _currentExpression;

			_textEditor.TextChanged += onTextChanged;
			_textEditor.OnStartIdlingAsync += onStartIdling;
		}
	}

	private void onTextChanged()
	{
		_dirty = true;
	}

	private async Task onStartIdling()
	{
		if (!_dirty)
		{
			return;
		}

		_dirty = false;

		try
		{
			_currentExpression = LambdaExpression.Parse(_textEditor.Text);
		}
		catch (InvalidTermException e)
		{
			_betaReductionView.ErrorMessage = e.Message + " At character " + e.Index;
			StateHasChanged();
			return;
		}
		catch (Exception e)
		{
			_betaReductionView.ErrorMessage = e.Message;
			StateHasChanged();
			return;
		}

		_diagramView.LambdaExpression = _currentExpression;
		_betaReductionView.ErrorMessage = "";

		await _betaReductionView.SetLambdaExpression(_currentExpression);
		StateHasChanged();
	}

	private async Task downloadDiagramJson()
	{
		string json = JsonConvert.SerializeObject(_diagramView.Geometry, Formatting.Indented);
		await download(generateStreamFromString(json), "diagram.json");
	}

	private async Task downloadAllStepsJson()
	{
		List<Geometry> geos = [];
		foreach (LambdaExpression? step in _betaReductionView.Steps)
		{
			if (step is null)
			{
				continue;
			}

			Geometry geo = LambdaExpressionRenderer.Render(step);
			geos.Add(geo);
		}
		
		string json = JsonConvert.SerializeObject(geos, Formatting.Indented);
		await download(generateStreamFromString(json), "steps.json");
	}

	private async Task downloadDiagramBitmap()
	{
		using SKBitmap bmp = _diagramView.Geometry.ToBitmap();

		SKData encoded = bmp.Encode(SKEncodedImageFormat.Png, 100);
		await download(encoded.AsStream(), "diagram.png");
	}

	private async Task downloadDiagramHD()
	{
		using SKBitmap bmp = _diagramView.Geometry.ToBitmap();

		double factor;

		if (bmp.Width > bmp.Height)
		{
			factor = 1920d / bmp.Width;
		}
		else
		{
			factor = 1080d / bmp.Height;
		}

		using SKBitmap bmpResized = bmp.Resize(new SKSizeI((int)(bmp.Width * factor), (int)(bmp.Height * factor)),
											   SKSamplingOptions.Default);
		SKData encoded = bmpResized.Encode(SKEncodedImageFormat.Png, 100);
		await download(encoded.AsStream(), "diagram_hd.png");
	}

	private async Task download(Stream file, string fileName)
	{
		using DotNetStreamReference streamRef = new(file);
		await _jsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
	}

	private static MemoryStream generateStreamFromString(string s)
	{
		MemoryStream stream = new();
		StreamWriter writer = new(stream);
		writer.Write(s);
		writer.Flush();
		stream.Position = 0;
		return stream;
	}
}