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
}