namespace CannyFilter;

public static class Hysteresis
{
    public static byte[] Apply(float[] mag, int width, int height, float lowRatio, float highRatio)
    {
        int size = width * height;
        byte[] result = new byte[size];
        
        float max = 0f;
        for (int i = 0; i < size; i++) if (mag[i] > max) max = mag[i];
        
        float high = highRatio * max;
        float low = lowRatio * max;

        var strong = new Stack<int>();
        
        const byte strongValue = 2;
        const byte weakValue = 1;
        var marks = new byte[size];
        
        for (int i = 0; i < size; i++)
        {
            if (mag[i] >= high)
            {
                marks[i] = strongValue;
                strong.Push(i);
            }
            else if (mag[i] >= low)
            {
                marks[i] = weakValue;
            }
            else
            {
                marks[i] = 0;
            }
        }

        while (strong.Count > 0)
        {
            int idx = strong.Pop();
            if (result[idx] == 1) continue;
            result[idx] = 1;


            int y = idx / width;
            int x = idx % width;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    int nidx = ny * width + nx;
                    if (marks[nidx] == weakValue && result[nidx] == 0)
                    {
                        result[nidx] = 1;
                        strong.Push(nidx);
                    }
                }
            }
        }
        
        return result;
    }
}