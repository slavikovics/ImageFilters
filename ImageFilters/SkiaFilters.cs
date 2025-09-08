using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;

namespace ImageFilters;

public static class SkiaFilters
{
    public static SKBitmap Original { get; set; }

    public static SKBitmap? Cached { get; private set; }

    private static SKBitmap ApplyFilter(SKBitmap src, SKPaint paint)
    {
        var info = new SKImageInfo(src.Width, src.Height);
        using var surface = SKSurface.Create(info);

        surface.Canvas.Clear(SKColors.Transparent);
        surface.Canvas.DrawBitmap(src, 0, 0, paint);
        surface.Canvas.Flush();

        using var snapshot = surface.Snapshot();
        var dst = new SKBitmap(info);

        bool ok = snapshot.ReadPixels(dst.Info, dst.GetPixels(), dst.RowBytes);
        if (!ok)
            throw new InvalidOperationException("Failed to copy pixels");

        return dst;
    }

    public static SKBitmap Grayscale() => ApplyColorMatrix(Original, [
        0.2126f, 0.2126f, 0.2126f, 0, 0,
        0.7152f, 0.7152f, 0.7152f, 0, 0,
        0.0722f, 0.0722f, 0.0722f, 0, 0,
        0, 0, 0, 1, 0
    ]);

    public static SKBitmap Sepia() => ApplyColorMatrix(Original, [
        0.393f, 0.769f, 0.189f, 0, 0,
        0.349f, 0.686f, 0.168f, 0, 0,
        0.272f, 0.534f, 0.131f, 0, 0,
        0, 0, 0, 1, 0
    ]);

    public static SKBitmap Invert() => ApplyColorMatrix(Original, [
        -1, 0, 0, 0, 255,
        0, -1, 0, 0, 255,
        0, 0, -1, 0, 255,
        0, 0, 0, 1, 0
    ]);

    public static SKBitmap Brightness(float factor) => ApplyColorMatrix(Original, [
        factor, 0, 0, 0, 0,
        0, factor, 0, 0, 0,
        0, 0, factor, 0, 0,
        0, 0, 0, 1, 0
    ]);

    public static SKBitmap Contrast(float factor)
    {
        float t = 0.5f * (1 - factor) * 255f;
        return ApplyColorMatrix(Original, [
            factor, 0, 0, 0, t,
            0, factor, 0, 0, t,
            0, 0, factor, 0, t,
            0, 0, 0, 1, 0
        ]);
    }

    public static SKBitmap Blur(float sigma)
    {
        var paint = new SKPaint
        {
            ImageFilter = SKImageFilter.CreateBlur(sigma, sigma)
        };
        return ApplyFilter(Original, paint);
    }

    private static SKBitmap ApplyColorMatrix(SKBitmap src, float[] matrix)
    {
        var paint = new SKPaint { ColorFilter = SKColorFilter.CreateColorMatrix(matrix) };
        return ApplyFilter(src, paint);
    }
}