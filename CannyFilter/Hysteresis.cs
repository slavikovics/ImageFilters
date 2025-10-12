namespace CannyFilter;

public class Hysteresis
{
    public static byte[] Apply(float[] mag, int width, int height, float lowRatio, float highRatio)
    {
        int size = width * height;
        byte[] result = new byte[size]; // 0 - non-edge, 1 - edge


// находим максимальный модуль градиента
        float max = 0f;
        for (int i = 0; i < size; i++) if (mag[i] > max) max = mag[i];


        float high = highRatio * max;
        float low = lowRatio * max;


        var strong = new Stack<int>();


// отмечаем сильные и слабые пиксели
        const byte STRONG = 2;
        const byte WEAK = 1;
        var marks = new byte[size];


        for (int i = 0; i < size; i++)
        {
            if (mag[i] >= high)
            {
                marks[i] = STRONG;
                strong.Push(i);
            }
            else if (mag[i] >= low)
            {
                marks[i] = WEAK;
            }
            else
            {
                marks[i] = 0;
            }
        }


// прослеживаем слабые пиксели, связанные с сильными
        while (strong.Count > 0)
        {
            int idx = strong.Pop();
            if (result[idx] == 1) continue; // уже помечено
            result[idx] = 1;


            int y = idx / width;
            int x = idx % width;


// проверяем 8 соседей
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    int nidx = ny * width + nx;
                    if (marks[nidx] == WEAK && result[nidx] == 0)
                    {
// если слабый и ещё не помечен, считаем его частью границы
                        result[nidx] = 1;
                        strong.Push(nidx);
                    }
                }
            }
        }


        return result;
    }
}