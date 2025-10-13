namespace CannyFilter;

public class GaussianFilter
{
    public static float[] Apply(float[] src, int width, int height, float sigma)
    {
        if (src == null) throw new ArgumentNullException(nameof(src));
        if (width <= 0 || height <= 0) throw new ArgumentException("Invalid dimensions");
        if (src.Length != width * height) throw new ArgumentException("Array size doesn't match dimensions");
        
        if (sigma <= 0) return (float[])src.Clone();
        
        int radius = Math.Max(1, (int)Math.Ceiling(3 * sigma));
        int kernelSize = 2 * radius + 1;
        var kernel = CreateGaussianKernel(radius, sigma);
        
        var tmp = ApplyHorizontalPass(src, width, height, kernel, radius);
        var dst = ApplyVerticalPass(tmp, width, height, kernel, radius);
        
        return dst;
    }
    
    private static float[] CreateGaussianKernel(int radius, float sigma)
    {
        int kernelSize = 2 * radius + 1;
        var kernel = new float[kernelSize];
        
        double sum = 0.0;
        double sigmaSquared2 = 2 * sigma * sigma;
        
        for (int i = -radius; i <= radius; i++)
        {
            double value = Math.Exp(-(i * i) / sigmaSquared2);
            kernel[i + radius] = (float)value;
            sum += value;
        }
        
        double invSum = 1.0 / sum;
        for (int i = 0; i < kernelSize; i++)
        {
            kernel[i] = (float)(kernel[i] * invSum);
        }
        
        return kernel;
    }
    
    private static float[] ApplyHorizontalPass(float[] src, int width, int height, float[] kernel, int radius)
    {
        var tmp = new float[width * height];
        
        for (int y = 0; y < height; y++)
        {
            int rowStart = y * width;
            
            for (int x = 0; x < width; x++)
            {
                float sum = 0f;
                
                for (int k = -radius; k <= radius; k++)
                {
                    int sx = x + k;
                    
                    if (sx < 0) sx = -sx;
                    else if (sx >= width) sx = 2 * width - sx - 2;
                    
                    sum += src[rowStart + sx] * kernel[k + radius];
                }
                
                tmp[rowStart + x] = sum;
            }
        }
        
        return tmp;
    }
    
    private static float[] ApplyVerticalPass(float[] tmp, int width, int height, float[] kernel, int radius)
    {
        var dst = new float[width * height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float sum = 0f;
                
                for (int k = -radius; k <= radius; k++)
                {
                    int sy = y + k;

                    if (sy < 0) sy = -sy;
                    else if (sy >= height) sy = 2 * height - sy - 2;
                    
                    sum += tmp[sy * width + x] * kernel[k + radius];
                }
                
                dst[y * width + x] = sum;
            }
        }
        
        return dst;
    }
}