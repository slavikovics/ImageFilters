using SkiaSharp;

namespace CannyFilter;

public class RegionFiller
{
    public static SKBitmap FillRegions(SKBitmap edgesBitmap, SKColor fillColor)
    {
        int width = edgesBitmap.Width;
        int height = edgesBitmap.Height;
        
        var filled = new SKBitmap(width, height);
        using var canvas = new SKCanvas(filled);
        canvas.DrawBitmap(edgesBitmap, 0, 0);
        
        using (var paint = new SKPaint
        {
            Color = SKColors.White,
            StrokeWidth = 2,
            IsAntialias = false
        })
        {
            canvas.DrawBitmap(edgesBitmap, 0, 0, paint);
        }
        
        var pixels = new byte[width * height];
        unsafe
        {
            var ptr = (uint*)filled.GetPixels().ToPointer();
            for (int i = 0; i < width * height; i++)
            {
                uint color = ptr[i];
                pixels[i] = (byte)(((color & 0xFF) > 128) ? 255 : 0);
            }
        }
        
        var visited = new bool[width * height];
        var stack = new Stack<(int x, int y)>();
        var paintFill = new SKPaint { Color = fillColor };

        using var fillCanvas = new SKCanvas(filled);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                if (pixels[idx] == 0 && !visited[idx])
                {
                    stack.Push((x, y));
                    bool touchesEdge = false;
                    var region = new List<(int, int)>();

                    while (stack.Count > 0)
                    {
                        var (cx, cy) = stack.Pop();
                        if (cx < 0 || cy < 0 || cx >= width || cy >= height)
                        {
                            touchesEdge = true;
                            continue;
                        }

                        int cidx = cy * width + cx;
                        if (visited[cidx] || pixels[cidx] > 0)
                            continue;

                        visited[cidx] = true;
                        region.Add((cx, cy));

                        stack.Push((cx + 1, cy));
                        stack.Push((cx - 1, cy));
                        stack.Push((cx, cy + 1));
                        stack.Push((cx, cy - 1));
                    }

                    if (!touchesEdge && region.Count > 10)
                    {
                        foreach (var (rx, ry) in region)
                            fillCanvas.DrawPoint(rx, ry, paintFill);
                    }
                }
            }
        }

        return filled;
    }
}
