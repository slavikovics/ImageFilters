using SkiaSharp;

namespace SpectralImages;

public static class Stereo
{
    public static SKBitmap CreateAnaglyphImage(SKBitmap original, int shiftAmount)
    {
        int width = original.Width;
        int height = original.Height;
    
        var result = new SKBitmap(width, height);
    
        using (var canvas = new SKCanvas(result))
        {
            canvas.Clear(SKColors.Black);
            
            using (var redPaint = new SKPaint())
            {
                redPaint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                {
                    1, 0, 0, 0, 0,  // R
                    0, 0, 0, 0, 0,  // G
                    0, 0, 0, 0, 0,  // B
                    0, 0, 0, 1, 0   // A
                });
            
                var leftRect = new SKRect(-shiftAmount, 0, width - shiftAmount, height);
                canvas.DrawBitmap(original, leftRect, redPaint);
            }
        
            using (var cyanPaint = new SKPaint())
            {
                cyanPaint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                {
                    0, 0, 0, 0, 0,  // R
                    0, 1, 0, 0, 0,  // G
                    0, 0, 1, 0, 0,  // B
                    0, 0, 0, 1, 0   // A
                });
                cyanPaint.BlendMode = SKBlendMode.Plus;
            
                var rightRect = new SKRect(shiftAmount, 0, width + shiftAmount, height);
                canvas.DrawBitmap(original, rightRect, cyanPaint);
            }
        }
    
        return result;
    }
}