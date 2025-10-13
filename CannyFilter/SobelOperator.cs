namespace CannyFilter;

public class SobelOperator
{
    public static void ComputeGradients(float[] gray, int width, int height, out float[] gx, out float[] gy, out float[] mag)
    {
        gx = new float[width * height];
        gy = new float[width * height];
        mag = new float[width * height];
        
        int[,] kx = new int[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        int[,] ky = new int[3, 3] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };


        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                float sx = 0f, sy = 0f;
                for (int ky_i = -1; ky_i <= 1; ky_i++)
                {
                    for (int kx_i = -1; kx_i <= 1; kx_i++)
                    {
                        int px = x + kx_i;
                        int py = y + ky_i;
                        float val = gray[py * width + px];
                        sx += val * kx[ky_i + 1, kx_i + 1];
                        sy += val * ky[ky_i + 1, kx_i + 1];
                    }
                }
                int idx = y * width + x;
                gx[idx] = sx;
                gy[idx] = sy;
                mag[idx] = (float)Math.Sqrt(sx * sx + sy * sy);
            }
        }
    }
}