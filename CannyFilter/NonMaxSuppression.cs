namespace CannyFilter;

public class NonMaxSuppression
{
    public static float[] Apply(float[] gx, float[] gy, float[] mag, int width, int height)
    {
        var outMag = new float[width * height];


        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int idx = y * width + x;
                float gxi = gx[idx];
                float gyi = gy[idx];
                float m = mag[idx];
                if (m == 0)
                {
                    outMag[idx] = 0;
                    continue;
                }
                
                float angle = (float)(Math.Atan2(gyi, gxi) * 180.0 / Math.PI);
                if (angle < 0) angle += 180f;


                float neighbor1 = 0f, neighbor2 = 0f;

                if ((angle >= 0 && angle < 22.5f) || (angle >= 157.5f && angle < 180))
                {
                    neighbor1 = mag[idx - 1];
                    neighbor2 = mag[idx + 1];
                }
                else if (angle >= 22.5f && angle < 67.5f)
                {
                    neighbor1 = mag[idx - width - 1];
                    neighbor2 = mag[idx + width + 1];
                }
                else if (angle >= 67.5f && angle < 112.5f)
                {
                    neighbor1 = mag[idx - width];
                    neighbor2 = mag[idx + width];
                }
                else
                {
                    neighbor1 = mag[idx - width + 1];
                    neighbor2 = mag[idx + width - 1];
                }
                
                if (m >= neighbor1 && m >= neighbor2)
                {
                    outMag[idx] = m;
                }
                else
                {
                    outMag[idx] = 0f;
                }
            }
        }
        
        return outMag;
    }
}