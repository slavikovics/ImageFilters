using SkiaSharp;

namespace CannyFilter;

public static class Morphology
{
    public static SKBitmap Close(SKBitmap src, int radius = 1)
    {
        var width = src.Width;
        var height = src.Height;

        var dilated = Dilate(src, radius);
        var closed = Erode(dilated, radius);
        return closed;
    }

    private static SKBitmap Dilate(SKBitmap src, int radius)
    {
        int w = src.Width, h = src.Height;
        var dst = new SKBitmap(w, h);
        unsafe
        {
            var srcPtr = (uint*)src.GetPixels().ToPointer();
            var dstPtr = (uint*)dst.GetPixels().ToPointer();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool hasWhite = false;
                    for (int ky = -radius; ky <= radius && !hasWhite; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int nx = x + kx, ny = y + ky;
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                                continue;
                            var color = srcPtr[ny * w + nx];
                            byte v = (byte)(color & 0xFF);
                            if (v > 128) { hasWhite = true; break; }
                        }
                    }
                    dstPtr[y * w + x] = hasWhite ? 0xFFFFFFFF : 0xFF000000;
                }
            }
        }
        return dst;
    }

    private static SKBitmap Erode(SKBitmap src, int radius)
    {
        int w = src.Width, h = src.Height;
        var dst = new SKBitmap(w, h);
        unsafe
        {
            var srcPtr = (uint*)src.GetPixels().ToPointer();
            var dstPtr = (uint*)dst.GetPixels().ToPointer();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool allWhite = true;
                    for (int ky = -radius; ky <= radius && allWhite; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int nx = x + kx, ny = y + ky;
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                                continue;
                            var color = srcPtr[ny * w + nx];
                            byte v = (byte)(color & 0xFF);
                            if (v < 128) { allWhite = false; break; }
                        }
                    }
                    dstPtr[y * w + x] = allWhite ? 0xFFFFFFFF : 0xFF000000;
                }
            }
        }
        return dst;
    }
}