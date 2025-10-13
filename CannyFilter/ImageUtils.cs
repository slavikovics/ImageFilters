using SkiaSharp;

namespace CannyFilter;

public class ImageUtils
{
    public static float[] ToGrayscale(SKBitmap bmp)
    {
        int w = bmp.Width;
        int h = bmp.Height;
        var res = new float[w * h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var color = bmp.GetPixel(x, y);
                float gray = 0.299f * color.Red + 0.587f * color.Green + 0.114f * color.Blue;
                res[y * w + x] = gray;
            }
        }
        return res;
    }


    public static SKBitmap FromBinaryEdgeMap(byte[] edges, int width, int height)
    {
        var outBmp = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                byte v = edges[idx] != 0 ? (byte)255 : (byte)0;
                outBmp.SetPixel(x, y, new SKColor(v, v, v));
            }
        }
        return outBmp;
    }
    
    public static SKBitmap MergeBitmaps(SKBitmap originalBitmap, SKBitmap overlayBitmap)
    {
        if (originalBitmap == null)
            throw new ArgumentNullException(nameof(originalBitmap));
        if (overlayBitmap == null)
            throw new ArgumentNullException(nameof(overlayBitmap));

        int width = Math.Min(originalBitmap.Width, overlayBitmap.Width);
        int height = Math.Min(originalBitmap.Height, overlayBitmap.Height);

        var result = new SKBitmap(width, height, originalBitmap.ColorType, originalBitmap.AlphaType);
        using (var canvas = new SKCanvas(result))
        {
            canvas.DrawBitmap(originalBitmap, new SKRect(0, 0, width, height));

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var overlayColor = overlayBitmap.GetPixel(x, y);
                    if (overlayColor.Red > 128 && overlayColor.Green > 128 && overlayColor.Blue > 128)
                    {
                        result.SetPixel(x, y, SKColors.Red);
                    }
                }
            }
        }

        return result;
    }
}