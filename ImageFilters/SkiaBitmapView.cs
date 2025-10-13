using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;

namespace ImageFilters;

public class SkiaBitmapView : Control
{
    public enum StretchMode
    {
        None,
        Fill,
        Uniform,
        UniformToFill
    }

    public static readonly StyledProperty<SKBitmap?> CurrentBitmapProperty =
        AvaloniaProperty.Register<SkiaBitmapView, SKBitmap?>(nameof(CurrentBitmap));

    public static readonly StyledProperty<StretchMode> StretchProperty =
        AvaloniaProperty.Register<SkiaBitmapView, StretchMode>(nameof(Stretch), StretchMode.Uniform);

    private WriteableBitmap? _writeableBitmap;
    private Bitmap? _fallbackBitmap;
    private SKBitmap? _directBitmap;
    private bool _useFallbackRendering = false;

    public SKBitmap? CurrentBitmap
    {
        get => GetValue(CurrentBitmapProperty);
        set => SetValue(CurrentBitmapProperty, value);
    }

    public StretchMode Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    static SkiaBitmapView()
    {
        CurrentBitmapProperty.Changed.AddClassHandler<SkiaBitmapView>((x, e) => x.OnBitmapChanged(e));
    }

    private void OnBitmapChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var newBitmap = e.NewValue as SKBitmap;
        Debug.WriteLine($"Bitmap changed: {newBitmap?.Width}x{newBitmap?.Height}, ColorType: {newBitmap?.ColorType}");
        
        _directBitmap = newBitmap;
        UpdateBitmapRendering();
        InvalidateVisual();
    }

    private void UpdateBitmapRendering()
    {
        _writeableBitmap?.Dispose();
        _writeableBitmap = null;
        _fallbackBitmap?.Dispose();
        _fallbackBitmap = null;
        _useFallbackRendering = false;

        if (_directBitmap == null) return;

        try
        {
            if (TryCreateWriteableBitmap(_directBitmap, out _writeableBitmap))
            {
                Debug.WriteLine("Using WriteableBitmap rendering");
                return;
            }

            if (TryCreateFallbackBitmap(_directBitmap, out _fallbackBitmap))
            {
                _useFallbackRendering = true;
                Debug.WriteLine("Using fallback bitmap rendering");
                return;
            }

            Debug.WriteLine("All rendering strategies failed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Bitmap rendering setup failed: {ex.Message}");
        }
    }

    private bool TryCreateWriteableBitmap(SKBitmap skBitmap, out WriteableBitmap? writeableBitmap)
    {
        writeableBitmap = null;
        
        try
        {
            using var compatibleBitmap = EnsureCompatibleFormat(skBitmap);
            if (compatibleBitmap == null) return false;

            var pixelSize = new PixelSize(compatibleBitmap.Width, compatibleBitmap.Height);
            var dpi = new Vector(96, 96);

            writeableBitmap = new WriteableBitmap(pixelSize, dpi, PixelFormat.Bgra8888, AlphaFormat.Premul);

            using (var lockBuffer = writeableBitmap.Lock())
            {
                CopyBitmapDataSafe(compatibleBitmap, lockBuffer);
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WriteableBitmap creation failed: {ex.Message}");
            writeableBitmap?.Dispose();
            writeableBitmap = null;
            return false;
        }
    }

    private bool TryCreateFallbackBitmap(SKBitmap skBitmap, out Bitmap? bitmap)
    {
        bitmap = null;
        
        try
        {
            using var image = SKImage.FromBitmap(skBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            if (data == null) return false;

            using var stream = data.AsStream();
            bitmap = new Bitmap(stream);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Fallback bitmap creation failed: {ex.Message}");
            bitmap?.Dispose();
            bitmap = null;
            return false;
        }
    }

    private SKBitmap? EnsureCompatibleFormat(SKBitmap original)
    {
        try
        {
            if (original.ColorType == SKColorType.Bgra8888 && original.AlphaType == SKAlphaType.Premul)
            {
                return original.Copy();
            }

            var converted = new SKBitmap(original.Width, original.Height, 
                SKColorType.Bgra8888, SKAlphaType.Premul);
            
            using var canvas = new SKCanvas(converted);
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(original, 0, 0);
            
            return converted;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Format conversion failed: {ex.Message}");
            return null;
        }
    }

    private unsafe void CopyBitmapDataSafe(SKBitmap source, ILockedFramebuffer destination)
    {
        var srcPtr = (byte*)source.GetPixels().ToPointer();
        var dstPtr = (byte*)destination.Address.ToPointer();

        int srcRowBytes = source.RowBytes;
        int dstRowBytes = destination.RowBytes;
        int width = source.Width;
        int height = source.Height;
        int bytesPerPixel = 4;

        int bytesToCopyPerRow = Math.Min(width * bytesPerPixel, Math.Min(srcRowBytes, dstRowBytes));

        for (int y = 0; y < height; y++)
        {
            var srcRow = srcPtr + (y * srcRowBytes);
            var dstRow = dstPtr + (y * dstRowBytes);
            
            Buffer.MemoryCopy(srcRow, dstRow, bytesToCopyPerRow, bytesToCopyPerRow);
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        try
        {
            if (!_useFallbackRendering && _writeableBitmap != null)
            {
                RenderWriteableBitmap(context, bounds);
            }
            else if (_useFallbackRendering && _fallbackBitmap != null)
            {
                RenderFallbackBitmap(context, bounds);
            }
            else if (_directBitmap != null)
            {
                // Last resort: Try direct rendering
                RenderDirectBitmap(context, bounds);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Render failed: {ex.Message}");
            var errorBrush = new SolidColorBrush(Colors.Red);
            context.FillRectangle(errorBrush, bounds);
        }
    }

    private void RenderWriteableBitmap(DrawingContext context, Rect bounds)
    {
        var imageSize = new Size(_writeableBitmap!.Size.Width, _writeableBitmap.Size.Height);
        var destRect = CalculateDestinationRect(imageSize, bounds);
        var srcRect = new Rect(0, 0, imageSize.Width, imageSize.Height);
        
        context.DrawImage(_writeableBitmap, srcRect, destRect);
    }

    private void RenderFallbackBitmap(DrawingContext context, Rect bounds)
    {
        var imageSize = new Size(_fallbackBitmap!.Size.Width, _fallbackBitmap.Size.Height);
        var destRect = CalculateDestinationRect(imageSize, bounds);
        var srcRect = new Rect(0, 0, imageSize.Width, imageSize.Height);
        
        context.DrawImage(_fallbackBitmap, srcRect, destRect);
    }

    private void RenderDirectBitmap(DrawingContext context, Rect bounds)
    {
        var debugBrush = new SolidColorBrush(Colors.Blue);
        context.FillRectangle(debugBrush, bounds);
        Debug.WriteLine("Using direct bitmap fallback rendering");
    }

    private Rect CalculateDestinationRect(Size imageSize, Rect bounds)
    {
        return Stretch switch
        {
            StretchMode.None => new Rect(0, 0, imageSize.Width, imageSize.Height),
            StretchMode.Fill => new Rect(0, 0, bounds.Width, bounds.Height),
            StretchMode.Uniform => GetUniformRect(imageSize, bounds),
            StretchMode.UniformToFill => GetUniformToFillRect(imageSize, bounds.Size),
            _ => new Rect(0, 0, imageSize.Width, imageSize.Height)
        };
    }

    private Rect GetUniformRect(Size image, Rect bounds)
    {
        if (image.Width <= 0 || image.Height <= 0) return bounds;
        
        double scale = Math.Min(bounds.Width / image.Width, bounds.Height / image.Height);
        double w = image.Width * scale;
        double h = image.Height * scale;
        double x = (bounds.Width - w) / 2;
        double y = (bounds.Height - h) / 2;
        return new Rect(x, y, w, h);
    }

    private Rect GetUniformToFillRect(Size image, Size bounds)
    {
        if (image.Width <= 0 || image.Height <= 0) return new Rect(0, 0, bounds.Width, bounds.Height);
        
        double scale = Math.Max(bounds.Width / image.Width, bounds.Height / image.Height);
        double w = image.Width * scale;
        double h = image.Height * scale;
        double x = (bounds.Width - w) / 2;
        double y = (bounds.Height - h) / 2;
        return new Rect(x, y, w, h);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _writeableBitmap?.Dispose();
        _fallbackBitmap?.Dispose();
        base.OnDetachedFromVisualTree(e);
    }
}