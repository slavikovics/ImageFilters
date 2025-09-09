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

    [ObservableProperty] 
    private SKBitmap _imageSource;

    private SKBitmap _backup;
    
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
        SkiaFilters.SaveChanges();
        _selectedLiveFilter = null;
        _selectedLiveFilter += UpdateFilter1;
        await UpdateFilter1();
    }

    [RelayCommand]
    private async Task ApplyFilter2()
    {
        SkiaFilters.SaveChanges();
        _selectedLiveFilter = null;
        _selectedLiveFilter += UpdateFilter2;
        await UpdateFilter2();
    }

    [RelayCommand]
    private async Task ApplyFilter3()
    {
        SkiaFilters.SaveChanges();
        _selectedLiveFilter = null;
        _selectedLiveFilter += UpdateFilter3;
        await UpdateFilter3();
    }

    [RelayCommand]
    private async Task ApplyFilter4()
    {
        SkiaFilters.SaveChanges();
        _selectedLiveFilter = null;
        _selectedLiveFilter += UpdateFilter4;
        await UpdateFilter4();
    }

    [RelayCommand]
    private async Task ApplyFilter5()
    {
        SkiaFilters.SaveChanges();
        _selectedLiveFilter = null;
        _selectedLiveFilter += UpdateFilter5;
        await UpdateFilter5();
    }
    
    private async Task UpdateFilter1()
    {
        ImageSource = SkiaFilters.Sepia((float) SliderValue);
    }
    
    private async Task UpdateFilter2()
    {
        ImageSource = SkiaFilters.Blur((float)SliderValue * 10);
    }
    
    private async Task UpdateFilter3()
    {
        ImageSource = SkiaFilters.Brightness((float)SliderValue * 2);
    }
    
    private async Task UpdateFilter4()
    {
        ImageSource = SkiaFilters.Contrast((float) SliderValue);
    }
    
    private async Task UpdateFilter5()
    {
        ImageSource = SkiaFilters.Invert((float) SliderValue);
    }
    
    private async Task ApplyLiveFilterChange()
    {
        if (Math.Abs(SliderValue - _lastSliderValue) < 0.01) return;
        
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
                _backup = ImageSource.Copy();
                SkiaFilters.Original = ImageSource;
                SkiaFilters.LastEdit = ImageSource;
            }
        }
        catch (Exception ex)
        {
        }
    }

    [RelayCommand]
    private void ResetFilter()
    {
        ImageSource = _backup.Copy();
        SkiaFilters.Original = _backup.Copy();
        SkiaFilters.LastEdit = _backup.Copy();
        _selectedLiveFilter = null;
    }
}