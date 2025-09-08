using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == CurrentBitmapProperty)
            UpdateBitmap();
    }

    private void UpdateBitmap()
    {
        if (CurrentBitmap == null)
        {
            _writeableBitmap = null;
            InvalidateMeasure();
            InvalidateVisual();
            return;
        }

        var src = CurrentBitmap.GetPixels();
        var srcRowBytes = CurrentBitmap.RowBytes;

        var pixelSize = new PixelSize(CurrentBitmap.Width, CurrentBitmap.Height);
        var dpi = new Vector(96, 96);

        _writeableBitmap = new WriteableBitmap(pixelSize, dpi,
            PixelFormat.Bgra8888, AlphaFormat.Premul);

        using (var lockInfo = _writeableBitmap.Lock())
        {
            unsafe
            {
                Buffer.MemoryCopy(
                    src.ToPointer(),
                    lockInfo.Address.ToPointer(),
                    srcRowBytes * CurrentBitmap.Height,
                    srcRowBytes * CurrentBitmap.Height);
            }
        }

        InvalidateMeasure();
        InvalidateVisual();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return availableSize;
    }

    public override void Render(DrawingContext dc)
    {
        base.Render(dc);

        if (_writeableBitmap == null) return;

        var bounds = Bounds;
        var imageSize = GetLogicalSize(_writeableBitmap);

        Rect destRect;
        switch (Stretch)
        {
            case StretchMode.None:
                destRect = new Rect(0, 0, imageSize.Width, imageSize.Height);
                break;

            case StretchMode.Fill:
                destRect = new Rect(0, 0, bounds.Width, bounds.Height);
                break;

            case StretchMode.Uniform:
                destRect = GetUniformRect(imageSize, bounds);
                break;

            case StretchMode.UniformToFill:
                destRect = GetUniformToFillRect(imageSize, bounds.Size);
                break;

            default:
                destRect = new Rect(0, 0, imageSize.Width, imageSize.Height);
                break;
        }

        var srcRect = new Rect(0, 0, imageSize.Width, imageSize.Height);
        dc.DrawImage(_writeableBitmap, srcRect, destRect);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return finalSize;
    }

    private Size GetLogicalSize(WriteableBitmap wb)
    {
        double factor = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;
        return new Size(wb.PixelSize.Width / factor,
            wb.PixelSize.Height / factor);
    }

    private Rect GetUniformRect(Size image, Rect bounds)
    {
        double scale = Math.Max(image.Width / bounds.Width, image.Height / bounds.Height);
        double w = image.Width / scale;
        double h = image.Height / scale;
        double x = (bounds.Width - w) / 2;
        double y = (bounds.Height - h) / 2;
        return new Rect(x, y, w, h);
    }

    private Rect GetUniformToFillRect(Size image, Size bounds)
    {
        double scale = Math.Max(bounds.Width / image.Width, bounds.Height / image.Height);
        double w = image.Width * scale;
        double h = image.Height * scale;
        double x = (bounds.Width - w) / 2;
        double y = (bounds.Height - h) / 2;
        return new Rect(x, y, w, h);
    }
}