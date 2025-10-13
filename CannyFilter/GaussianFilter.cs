namespace CannyFilter;

public class GaussianFilter
{
    public static float[] Apply(float[] src, int width, int height, float sigma)
    {
        if (sigma <= 0) return (float[])src.Clone();
        
        int radius = (int)Math.Ceiling(3 * sigma);
        int kernelSize = 2 * radius + 1;
        var kernel = new float[kernelSize];
        
        float sum = 0f;
        for (int i = -radius; i <= radius; i++)
        {
            float v = (float)Math.Exp(-(i * i) / (2 * sigma * sigma));
            kernel[i + radius] = v;
            sum += v;
        }

        for (int i = 0; i < kernelSize; i++) kernel[i] /= sum;
        
        var tmp = new float[width * height];
        var dst = new float[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float acc = 0f;
                for (int k = -radius; k <= radius; k++)
                {
                    int sx = x + k;
                    if (sx < 0) sx = 0;
                    if (sx >= width) sx = width - 1;
                    acc += src[y * width + sx] * kernel[k + radius];
                }
                tmp[y * width + x] = acc;
            }
        }
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float acc = 0f;
                for (int k = -radius; k <= radius; k++)
                {
                    int sy = y + k;
                    if (sy < 0) sy = 0;
                    if (sy >= height) sy = height - 1;
                    acc += tmp[sy * width + x] * kernel[k + radius];
                }
                dst[y * width + x] = acc;
            }
        }
        
        return dst;
    }
}