using SkiaSharp;

namespace TrompDiagrams.Rendering;

public static class SKBitmapExtensions
{
	public static void Save(this SKBitmap bitmap, string path)
	{
		using SKImage? image = SKImage.FromBitmap(bitmap);
		using SKData? data = image?.Encode(SKEncodedImageFormat.Png, 100);
		using FileStream stream = File.OpenWrite(path);
		data?.SaveTo(stream);
	}
}