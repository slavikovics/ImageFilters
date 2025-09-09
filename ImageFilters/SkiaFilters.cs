using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using InvalidOperationException = System.InvalidOperationException;

namespace ImageFilters;

public static class SkiaFilters
{
    public static SKBitmap Original { get; set; }

    public static SKBitmap? LastEdit { get; set; } = null;

    private static SKBitmap ApplyFilter(SKPaint paint)
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

    public static void SaveChanges()
    {
        if (LastEdit is not null)
        {
            Original = LastEdit;
        }
    }

    public static SKBitmap Grayscale() => ApplyColorMatrix(Original, [
        0.2126f, 0.2126f, 0.2126f, 0, 0,
        0.7152f, 0.7152f, 0.7152f, 0, 0,
        0.0722f, 0.0722f, 0.0722f, 0, 0,
        0, 0, 0, 1, 0
    ]);

    public static SKBitmap Sepia(float intensity)
    {
        intensity = Math.Clamp(intensity, 0f, 1f);
        
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

    public static SKBitmap Invert(float intensity)
    {
        if (Original == null) throw new ArgumentNullException(nameof(Original));

        intensity = Math.Clamp(intensity, 0f, 1f);

        // Create a color matrix that inverts colors with the specified intensity
        // The matrix is in the format expected by SkiaSharp (normalized values [0, 1])
        float[] matrix =
        [
            1 - 2 * intensity, 0, 0, 0, intensity,
            0, 1 - 2 * intensity, 0, 0, intensity,
            0, 0, 1 - 2 * intensity, 0, intensity,
            0, 0, 0, 1, 0
        ];

        return ApplyColorMatrix(Original, matrix);
    }

    public static SKBitmap Brightness(float factor) => ApplyColorMatrix(Original, [
        factor, 0, 0, 0, 0,
        0, factor, 0, 0, 0,
        0, 0, factor, 0, 0,
        0, 0, 0, 1, 0
    ]);

    public static SKBitmap Contrast(float factor)
    {
        factor = Math.Clamp(factor, 0f, 2f);
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

    public static SKBitmap Blur(float sigma)
    {
        var paint = new SKPaint
        {
            ImageFilter = SKImageFilter.CreateBlur(sigma, sigma)
        };
        return ApplyFilter(paint);
    }

    private static SKBitmap ApplyColorMatrix(SKBitmap src, float[] matrix)
    {
        var paint = new SKPaint { ColorFilter = SKColorFilter.CreateColorMatrix(matrix) };
        return ApplyFilter(paint);
    }
}