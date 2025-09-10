using System;
using SkiaSharp;

namespace ImageFilters;

public static class ImageAdjuster
{
    public static SKBitmap AdjustBrightnessContrast(SKBitmap target, SKBitmap reference)
    {
        // Приведение изображений к единому формату (Rgba8888)
        target = ConvertToRgba8888(target);
        reference = ConvertToRgba8888(reference);

        // Вычисление статистики для reference и target изображений
        var refStats = ComputeImageStats(reference);
        var targetStats = ComputeImageStats(target);

        // Создание копии target изображения для модификации
        SKBitmap adjusted = target.Copy();

        // Применение корректировок к каждому пикселю
        using (var pixmap = adjusted.PeekPixels())
        {
            var pixels = pixmap.GetPixelSpan<SKColor>();
            for (int i = 0; i < pixels.Length; i++)
            {
                SKColor color = pixels[i];
                double r = AdjustComponent(color.Red, targetStats.Red, refStats.Red);
                double g = AdjustComponent(color.Green, targetStats.Green, refStats.Green);
                double b = AdjustComponent(color.Blue, targetStats.Blue, refStats.Blue);

                // Клиппинг значений в диапазон [0, 255]
                byte newR = (byte)Math.Clamp(r, 0, 255);
                byte newG = (byte)Math.Clamp(g, 0, 255);
                byte newB = (byte)Math.Clamp(b, 0, 255);

                pixels[i] = new SKColor(newR, newG, newB, color.Alpha);
            }
        }

        return adjusted;
    }

    private static SKBitmap ConvertToRgba8888(SKBitmap bitmap)
    {
        if (bitmap.ColorType == SKColorType.Rgba8888)
            return bitmap;
        return bitmap.Copy(SKColorType.Rgba8888);
    }

    // Структура для хранения статистики по каналам
    private struct ImageStats
    {
        public (double Mean, double Std) Red;
        public (double Mean, double Std) Green;
        public (double Mean, double Std) Blue;
    }

    private static ImageStats ComputeImageStats(SKBitmap bitmap)
    {
        long sumR = 0, sumG = 0, sumB = 0;
        long sumSqR = 0, sumSqG = 0, sumSqB = 0;
        int totalPixels = bitmap.Width * bitmap.Height;

        // Первый проход: вычисление сумм
        using (var pixmap = bitmap.PeekPixels())
        {
            var pixels = pixmap.GetPixelSpan<SKColor>();
            foreach (var color in pixels)
            {
                sumR += color.Red;
                sumG += color.Green;
                sumB += color.Blue;
                sumSqR += color.Red * color.Red;
                sumSqG += color.Green * color.Green;
                sumSqB += color.Blue * color.Blue;
            }
        }

        // Вычисление среднего и стандартного отклонения
        return new ImageStats
        {
            Red = CalculateStats(sumR, sumSqR, totalPixels),
            Green = CalculateStats(sumG, sumSqG, totalPixels),
            Blue = CalculateStats(sumB, sumSqB, totalPixels)
        };
    }

    private static (double Mean, double Std) CalculateStats(long sum, long sumSq, int count)
    {
        double mean = (double)sum / count;
        double variance = (double)sumSq / count - mean * mean;
        double std = Math.Sqrt(variance);
        return (mean, std);
    }

    private static double AdjustComponent(byte value, (double Mean, double Std) target, (double Mean, double Std) reference)
    {
        if (target.Std == 0) // Избегание деления на ноль
            return reference.Mean;
        
        return (value - target.Mean) * (reference.Std / target.Std) + reference.Mean;
    }
}