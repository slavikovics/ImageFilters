using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media;
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

    private SkiaFilters? _currentFilter = null;

    private Color _selectedColor;

    private Color _defaultColor;

    [ObservableProperty]
    private SolidColorBrush _filter1Brush;

    [ObservableProperty]
    private SolidColorBrush _filter2Brush;

    [ObservableProperty]
    private SolidColorBrush _filter3Brush;

    [ObservableProperty]
    private SolidColorBrush _filter4Brush;

    [ObservableProperty]
    private SolidColorBrush _filter5Brush;

    [ObservableProperty]
    private bool _isImageLoaded;

    partial void OnSliderValueChanged(double value)
    {
        Task.Run(ApplyLiveFilterChange);
    }

    public MainWindowViewModel(IFilePickerService filePickerService)
    {
        _filePickerService = filePickerService;
        IsImageLoaded = false;
        SetUpColors();
    }

    private void SetUpColors()
    {
        _selectedColor = Color.FromRgb(30, 30, 30);
        _defaultColor = Color.FromRgb(51, 51, 51);

        DisableAllSelections();
    }

    private void DisableAllSelections()
    {
        Filter1Brush = new SolidColorBrush(_defaultColor);
        Filter2Brush = new SolidColorBrush(_defaultColor);
        Filter3Brush = new SolidColorBrush(_defaultColor);
        Filter4Brush = new SolidColorBrush(_defaultColor);
        Filter5Brush = new SolidColorBrush(_defaultColor);
    }

    [RelayCommand]
    private async Task ApplyFilter1()
    {
        _currentFilter?.SaveChanges();
        _selectedLiveFilter = null;
        _selectedLiveFilter += UpdateFilter1;
        DisableAllSelections();
        Filter1Brush.Color = _selectedColor;
        await UpdateFilter1();
    }

    [RelayCommand]
    private async Task ApplyFilter2()
    {
        _currentFilter?.SaveChanges();
        _selectedLiveFilter = null;
        _selectedLiveFilter += UpdateFilter2;
        DisableAllSelections();
        Filter2Brush.Color = _selectedColor;
        await UpdateFilter2();
    }

    [RelayCommand]
    private async Task ApplyFilter3()
    {
        _currentFilter?.SaveChanges();
        _selectedLiveFilter = null;
        _selectedLiveFilter += UpdateFilter3;
        DisableAllSelections();
        Filter3Brush.Color = _selectedColor;
        await UpdateFilter3();
    }

    [RelayCommand]
    private async Task ApplyFilter4()
    {
        _currentFilter?.SaveChanges();
        _selectedLiveFilter = null;
        _selectedLiveFilter += UpdateFilter4;
        DisableAllSelections();
        Filter4Brush.Color = _selectedColor;
        await UpdateFilter4();
    }

    [RelayCommand]
    private async Task ApplyFilter5()
    {
        _currentFilter?.SaveChanges();
        _selectedLiveFilter = null;
        _selectedLiveFilter += UpdateFilter5;
        DisableAllSelections();
        Filter5Brush.Color = _selectedColor;
        await UpdateFilter5();
    }
    
    private async Task UpdateFilter1()
    {
        if (_currentFilter is null) return;
        ImageSource = _currentFilter.Sepia((float) SliderValue);
    }
    
    private async Task UpdateFilter2()
    {
        if (_currentFilter is null) return;
        ImageSource = _currentFilter.Blur((float)SliderValue * 10);
    }
    
    private async Task UpdateFilter3()
    {
        if (_currentFilter is null) return;
        ImageSource = _currentFilter.Brightness((float)SliderValue * 2);
    }
    
    private async Task UpdateFilter4()
    {
        if (_currentFilter is null) return;
        ImageSource = _currentFilter.Contrast((float) SliderValue);
    }
    
    private async Task UpdateFilter5()
    {
        if (_currentFilter is null) return;
        ImageSource = _currentFilter.Invert((float) SliderValue);
    }
    
    private async Task ApplyLiveFilterChange()
    {
        if (Math.Abs(SliderValue - _lastSliderValue) < 1) return;
        
        _lastSliderValue = SliderValue;
        if (_selectedLiveFilter is null) return;

        await _selectedLiveFilter.Invoke();
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
                _currentFilter = new SkiaFilters(ImageSource);
                IsImageLoaded = true;
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
        if (_currentFilter is null) return;
        _currentFilter.Original = _backup.Copy();
        _currentFilter.LastEdit = _backup.Copy();
        DisableAllSelections();
        _selectedLiveFilter = null;
    }
}