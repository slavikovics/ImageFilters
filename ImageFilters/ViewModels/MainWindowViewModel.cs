using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ImageFilters.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFilePickerService _filePickerService;

    private Bitmap _imageBackup;

    [ObservableProperty]
    private Bitmap _imageSource;
    
    [ObservableProperty]
    private double _sliderValue = 0;
    
    [ObservableProperty]
    private SolidColorBrush _uploadButtonBrush = new (Color.FromArgb(255, 102, 102, 255));
    
    [ObservableProperty]
    private SolidColorBrush _firstFilterButtonBrush = new (Color.FromArgb(255, 102, 102, 255));
    
    [ObservableProperty]
    private SolidColorBrush _secondFilterButtonBrush = new (Color.FromArgb(255, 102, 102, 255));
    
    [ObservableProperty]
    private SolidColorBrush _thirdFilterButtonBrush = new (Color.FromArgb(255, 102, 102, 255));
    
    [ObservableProperty]
    private SolidColorBrush _fourthFilterButtonBrush = new (Color.FromArgb(255, 102, 102, 255));
    
    [ObservableProperty]
    private SolidColorBrush _fifthFilterButtonBrush = new (Color.FromArgb(255, 102, 102, 255));
    
    private List<SolidColorBrush> _filterBrushes;

    public MainWindowViewModel(IFilePickerService filePickerService)
    {
        _filePickerService = filePickerService;
        _filterBrushes =
        [
            FirstFilterButtonBrush, SecondFilterButtonBrush, ThirdFilterButtonBrush, FourthFilterButtonBrush,
            FifthFilterButtonBrush
        ];
    }

    [RelayCommand]
    private async Task SelectFilter(SolidColorBrush selectedFilter)
    {
        foreach (var brush in _filterBrushes)
        {
            brush.Color = Color.FromArgb(255, 102, 102, 255);
        }
        
        selectedFilter.Color = Color.FromArgb(255, 50, 50, 50);
    }

    [RelayCommand]
    private async Task ApplyFilter1()
    {
        ImageSource = SkiaFilters.Sepia(ImageSource);
    }
    
    [RelayCommand]
    private async Task ApplyFilter2()
    {
        ImageSource = SkiaFilters.Blur(ImageSource, SliderValue);
    }
    
    [RelayCommand]
    private async Task ApplyFilter3()
    {
        ImageSource = SkiaFilters.Brightness(ImageSource, SliderValue);
    }
    
    [RelayCommand]
    private async Task ApplyFilter4()
    {
        ImageSource = SkiaFilters.Sepia(ImageSource);
    }
    
    [RelayCommand]
    private async Task ApplyFilter5()
    {
        ImageSource = SkiaFilters.Sepia(ImageSource);
    }
    
    public static WriteableBitmap ConvertBitmapToWriteableBitmap(Bitmap bitmap)
    {
        return new WriteableBitmap(
            bitmap.PixelSize,
            bitmap.Dpi,
            bitmap.Format,
            bitmap.AlphaFormat
        );
    }

    [RelayCommand]
    private async Task LoadImage()
    {
        try
        {
            var source = await _filePickerService.PickImageAsync();
            if (source != null)
            {
                var bitmap = await _filePickerService.LoadImageFromStorageFileAsync(source);
                ImageSource = bitmap;
                _imageBackup = bitmap;
            }
        }
        catch (Exception ex)
        {
            // ignored
        }
    }

    [RelayCommand]
    private void ResetFilter()
    {
        ImageSource = _imageBackup;
    }
}