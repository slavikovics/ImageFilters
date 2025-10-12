using SkiaSharp;

namespace CannyFilter;

public class CannyDetector
{
    public float Sigma { get; set; } = 1.4f;
    public float LowThresholdRatio { get; set; } = 0.05f; // например 0.05
    public float HighThresholdRatio { get; set; } = 0.15f; // например 0.15
    
    public CannyDetector() { }
    
    public byte[] Detect(SKBitmap bmp)
    {
        int w = bmp.Width;
        int h = bmp.Height;

        var gray = ImageUtils.ToGrayscale(bmp);
        var blurred = GaussianFilter.Apply(gray, w, h, Sigma);
        SobelOperator.ComputeGradients(blurred, w, h, out var gx, out var gy, out var mag);
        var suppressed = NonMaxSuppression.Apply(gx, gy, mag, w, h);
        var edges = Hysteresis.Apply(suppressed, w, h, LowThresholdRatio, HighThresholdRatio);
        return edges;
    }


    public async Task<SKBitmap> DetectToBitmap(SKBitmap bmp)
    {
        int w = bmp.Width;
        int h = bmp.Height;
        var edges = await Task.Run(() => Detect(bmp));
        return await Task.Run(() => ImageUtils.FromBinaryEdgeMap(edges, w, h));
    }
}