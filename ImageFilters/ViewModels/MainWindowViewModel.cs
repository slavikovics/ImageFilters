using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;

namespace ImageFilters.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFilePickerService _filePickerService;

    [ObservableProperty] private SKBitmap _imageSource;
    
    [ObservableProperty] 
    private double _sliderValue = 0;
    
    private double _lastSliderValue = 0;

    private delegate Task SelectedFilter();
    
    private SelectedFilter? _selectedLiveFilter;

    partial void OnSliderValueChanged(double value)
    {
        Task.Run(ApplyLiveFilterChange);
    }

    public MainWindowViewModel(IFilePickerService filePickerService)
    {
        _filePickerService = filePickerService;
    }

    [RelayCommand]
    private async Task ApplyFilter1()
    {
        _selectedLiveFilter = null;
        ImageSource = SkiaFilters.Sepia();
    }

    [RelayCommand]
    private async Task ApplyFilter2()
    {
        _selectedLiveFilter = null;
        _selectedLiveFilter += ApplyFilter2;
        ImageSource = SkiaFilters.Blur((float)SliderValue * 10);
    }

    [RelayCommand]
    private async Task ApplyFilter3()
    {
        _selectedLiveFilter = null;
        _selectedLiveFilter += ApplyFilter3;
        ImageSource = SkiaFilters.Brightness((float)SliderValue * 2);
    }

    [RelayCommand]
    private async Task ApplyFilter4()
    {
        _selectedLiveFilter = null;
        _selectedLiveFilter += ApplyFilter4;
        ImageSource = SkiaFilters.Contrast((float)SliderValue * 3);
    }

    [RelayCommand]
    private async Task ApplyFilter5()
    {
        _selectedLiveFilter = null;
        ImageSource = SkiaFilters.Grayscale();
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
    
    private async Task ApplyLiveFilterChange()
    {
        if (Math.Abs(SliderValue - _lastSliderValue) < 0.05) return;
        
        _lastSliderValue = SliderValue;
        await _selectedLiveFilter?.Invoke()!;
    }

    [RelayCommand]
    private async Task LoadImage()
    {
        try
        {
            var source = await _filePickerService.PickImageAsync();
            if (source != null)
            {
                await using Stream stream = await source.OpenReadAsync();
                ImageSource = SKBitmap.Decode(stream);
                SkiaFilters.Original = ImageSource;
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
    }
}