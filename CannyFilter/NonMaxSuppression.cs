namespace CannyFilter;

public class NonMaxSuppression
{
    public static float[] Apply(float[] gx, float[] gy, float[] mag, int width, int height)
    {
        if (gx == null) throw new ArgumentNullException(nameof(gx));
        if (gy == null) throw new ArgumentNullException(nameof(gy));
        if (mag == null) throw new ArgumentNullException(nameof(mag));
        if (width <= 0 || height <= 0) throw new ArgumentException("Invalid dimensions");
        if (gx.Length != width * height || gy.Length != width * height || mag.Length != width * height)
            throw new ArgumentException("Array sizes don't match dimensions");
        
        var outMag = new float[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                float gxi = gx[idx];
                float gyi = gy[idx];
                float m = mag[idx];
                
                if (m < float.Epsilon)
                {
                    outMag[idx] = 0;
                    continue;
                }
                
                float angle = MathF.Atan2(gyi, gxi) * (180f / MathF.PI);
                if (angle < 0) angle += 180f;
                
                (int idx1, int idx2) = GetNeighborIndices(x, y, width, height, angle);
                
                if (idx1 < 0 || idx2 < 0)
                {
                    outMag[idx] = 0;
                    continue;
                }
                
                float neighbor1 = mag[idx1];
                float neighbor2 = mag[idx2];
                
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
    
    private static (int idx1, int idx2) GetNeighborIndices(int x, int y, int width, int height, float angle)
    {
        int idx1 = -1, idx2 = -1;
        
        if ((angle >= 0 && angle < 22.5f) || (angle >= 157.5f && angle <= 180f))
        {
            if (x > 0) idx1 = y * width + (x - 1);
            if (x < width - 1) idx2 = y * width + (x + 1);
        }
        else if (angle >= 22.5f && angle < 67.5f)
        {
            if (x > 0 && y > 0) idx1 = (y - 1) * width + (x - 1);
            if (x < width - 1 && y < height - 1) idx2 = (y + 1) * width + (x + 1);
        }
        else if (angle >= 67.5f && angle < 112.5f)
        {
            if (y > 0) idx1 = (y - 1) * width + x;
            if (y < height - 1) idx2 = (y + 1) * width + x;
        }
        else
        {
            if (x < width - 1 && y > 0) idx1 = (y - 1) * width + (x + 1);
            if (x > 0 && y < height - 1) idx2 = (y + 1) * width + (x - 1);
        }
        
        return (idx1, idx2);
    }
}