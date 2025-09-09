using System;
using SkiaSharp;

namespace ImageFilters;

public class SkiaFilters
{
    public SkiaFilters(SKBitmap original) 
    { 
        Original = original.Copy();
        LastEdit = original.Copy();
    }

    public SKBitmap Original { get; set; }

    public SKBitmap? LastEdit { get; set; } = null;

    private SKBitmap ApplyFilter(SKPaint paint)
    {
        var info = new SKImageInfo(Original.Width, Original.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);

        surface.Canvas.Clear(SKColors.Transparent);
        surface.Canvas.DrawBitmap(Original, 0, 0, paint);
        surface.Canvas.Flush();

        using var snapshot = surface.Snapshot();
        var dst = new SKBitmap(info);

        bool ok = snapshot.ReadPixels(dst.Info, dst.GetPixels(), dst.RowBytes);
        if (!ok)
            throw new InvalidOperationException("Failed to copy pixels");
        
        LastEdit = dst;
        return dst;
    }

    public void SaveChanges()
    {
        if (LastEdit is not null)
        {
            Original = LastEdit;
        }
    }

    public SKBitmap Grayscale() => ApplyColorMatrix(Original, [
        0.2126f, 0.2126f, 0.2126f, 0, 0,
        0.7152f, 0.7152f, 0.7152f, 0, 0,
        0.0722f, 0.0722f, 0.0722f, 0, 0,
        0, 0, 0, 1, 0
    ]);

    public float SmartClamp(float value, float newMin, float newMax, float originalMin = 0f, float originalMax = 100f)
    {
        float originalDelta = originalMax - originalMin;
        float newDelta = newMax - newMin;
        float factor = newDelta / originalDelta;
        return value * factor;
    }

    public SKBitmap Sepia(float intensity)
    {
        intensity = SmartClamp(intensity, 0f, 3f);
        
        float[] sepia =
        [
            0.393f, 0.769f, 0.189f, 0, 0,
            0.349f, 0.686f, 0.168f, 0, 0,
            0.272f, 0.534f, 0.131f, 0, 0,
            0,      0,      0,      1, 0
        ];
        
        float[] identity =
        [
            1, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 1, 0, 0,
            0, 0, 0, 1, 0
        ];
        
        float[] matrix = new float[20];
        for (int i = 0; i < 20; i++)
            matrix[i] = identity[i] * (1f - intensity) + sepia[i] * intensity;

        return ApplyColorMatrix(Original, matrix);
    }

    public SKBitmap Invert(float intensity)
    {
        if (Original == null) throw new ArgumentNullException(nameof(Original));

        intensity = SmartClamp(intensity, 0f, 1f);

        float[] matrix =
        [
            1 - 2 * intensity, 0, 0, 0, intensity,
            0, 1 - 2 * intensity, 0, 0, intensity,
            0, 0, 1 - 2 * intensity, 0, intensity,
            0, 0, 0, 1, 0
        ];

        return ApplyColorMatrix(Original, matrix);
    }

    public SKBitmap Brightness(float factor)
    {
        factor = SmartClamp(factor, 0f, 3f);
        return ApplyColorMatrix(Original, [
            factor, 0, 0, 0, 0,
            0, factor, 0, 0, 0,
            0, 0, factor, 0, 0,
            0, 0, 0, 1, 0
        ]);
    }

    public SKBitmap Contrast(float factor)
    {
        factor = SmartClamp(factor, 0f, 6f);
        float t = (128f * (1f - factor)) / 255f;

        return ApplyColorMatrix(
            Original,
            [
                factor, 0,     0,     0, t,
                0,     factor, 0,     0, t,
                0,     0,     factor, 0, t,
                0,     0,     0,     1, 0
            ]
        );
    }

    public SKBitmap Blur(float sigma)
    {
        sigma = SmartClamp(sigma, 0f, 15f);
        
        var paint = new SKPaint
        {
            ImageFilter = SKImageFilter.CreateBlur(sigma, sigma)
        };
        return ApplyFilter(paint);
    }

    private SKBitmap ApplyColorMatrix(SKBitmap src, float[] matrix)
    {
        var paint = new SKPaint { ColorFilter = SKColorFilter.CreateColorMatrix(matrix) };
        return ApplyFilter(paint);
    }
}