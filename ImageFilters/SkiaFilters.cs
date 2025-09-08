using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;

namespace ImageFilters;

public static class SkiaFilters
{
    private static Bitmap ApplyColorFilter(Bitmap source, SKColorFilter filter)
    {
        using var ms = new MemoryStream();
        source.Save(ms);
        ms.Position = 0;

        using var skBitmap = SKBitmap.Decode(ms);

        using var surface = SKSurface.Create(new SKImageInfo(skBitmap.Width, skBitmap.Height));
        var canvas = surface.Canvas;

        using var paint = new SKPaint { ColorFilter = filter };

        canvas.Clear(SKColors.Transparent);
        canvas.DrawBitmap(skBitmap, 0, 0, paint);
        canvas.Flush();

        using var snapshot = surface.Snapshot();
        using var data = snapshot.Encode(SKEncodedImageFormat.Png, 100);
        using var outStream = new MemoryStream();
        data.SaveTo(outStream);
        outStream.Position = 0;

        return new Bitmap(outStream);
    }
    
    public static Bitmap Grayscale(Bitmap source)
    {
        var matrix = new float[]
        {
            0.2126f, 0.2126f, 0.2126f, 0, 0,
            0.7152f, 0.7152f, 0.7152f, 0, 0,
            0.0722f, 0.0722f, 0.0722f, 0, 0,
            0,       0,       0,       1, 0
        };
        return ApplyColorFilter(source, SKColorFilter.CreateColorMatrix(matrix));
    }
    
    public static Bitmap Sepia(Bitmap source)
    {
        var matrix = new float[]
        {
            0.393f, 0.769f, 0.189f, 0, 0,
            0.349f, 0.686f, 0.168f, 0, 0,
            0.272f, 0.534f, 0.131f, 0, 0,
            0,      0,      0,      1, 0
        };
        return ApplyColorFilter(source, SKColorFilter.CreateColorMatrix(matrix));
    }
    
    public static Bitmap Invert(Bitmap source)
    {
        var matrix = new float[]
        {
            -1,  0,  0, 0, 255,
             0, -1,  0, 0, 255,
             0,  0, -1, 0, 255,
             0,  0,  0, 1,   0
        };
        return ApplyColorFilter(source, SKColorFilter.CreateColorMatrix(matrix));
    }
    
    public static Bitmap Brightness(Bitmap source, float factor)
    {
        var matrix = new float[]
        {
            factor, 0,      0,      0, 0,
            0,      factor, 0,      0, 0,
            0,      0,      factor, 0, 0,
            0,      0,      0,      1, 0
        };
        return ApplyColorFilter(source, SKColorFilter.CreateColorMatrix(matrix));
    }
    
    public static Bitmap Contrast(Bitmap source, float factor)
    {
        float t = 0.5f * (1 - factor) * 255f;
        var matrix = new float[]
        {
            factor, 0,      0,      0, t,
            0,      factor, 0,      0, t,
            0,      0,      factor, 0, t,
            0,      0,      0,      1, 0
        };
        return ApplyColorFilter(source, SKColorFilter.CreateColorMatrix(matrix));
    }
    
    public static Bitmap Blur(Bitmap source, float sigma)
    {
        using var ms = new MemoryStream();
        source.Save(ms);
        ms.Position = 0;

        using var skBitmap = SKBitmap.Decode(ms);

        using var surface = SKSurface.Create(new SKImageInfo(skBitmap.Width, skBitmap.Height));
        var canvas = surface.Canvas;

        using var paint = new SKPaint
        {
            ImageFilter = SKImageFilter.CreateBlur(sigma, sigma)
        };

        canvas.Clear(SKColors.Transparent);
        canvas.DrawBitmap(skBitmap, 0, 0, paint);
        canvas.Flush();

        using var snapshot = surface.Snapshot();
        using var data = snapshot.Encode(SKEncodedImageFormat.Png, 100);
        using var outStream = new MemoryStream();
        data.SaveTo(outStream);
        outStream.Position = 0;

        return new Bitmap(outStream);
    }
}