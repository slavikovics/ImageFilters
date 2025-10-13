namespace CannyFilter;

public class SobelOperator
{
    public static void ComputeGradients(float[] gray, int width, int height, 
        out float[] gx, out float[] gy, out float[] mag)
    {
        if (gray == null) throw new ArgumentNullException(nameof(gray));
        if (width <= 0 || height <= 0) throw new ArgumentException("Invalid dimensions");
        if (gray.Length != width * height) throw new ArgumentException("Array size doesn't match dimensions");
        
        gx = new float[width * height];
        gy = new float[width * height];
        mag = new float[width * height];
        
        int[,] kx = new int[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        int[,] ky = new int[3, 3] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sx = 0f, sy = 0f;
                
                for (int ky_i = -1; ky_i <= 1; ky_i++)
                {
                    for (int kx_i = -1; kx_i <= 1; kx_i++)
                    {
                        int px = Math.Clamp(x + kx_i, 0, width - 1);
                        int py = Math.Clamp(y + ky_i, 0, height - 1);
                        
                        float val = gray[py * width + px];
                        sx += val * kx[ky_i + 1, kx_i + 1];
                        sy += val * ky[ky_i + 1, kx_i + 1];
                    }
                }
                
                int idx = y * width + x;
                gx[idx] = sx;
                gy[idx] = sy;
                mag[idx] = MathF.Sqrt(sx * sx + sy * sy);
            }
        }
    }
}