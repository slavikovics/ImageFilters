using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ImageFilters.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFilePickerService _filePickerService;

    [ObservableProperty]
    private IImage? _imageSource = null;
    
    [ObservableProperty]
    private double _sliderValue = 0;
    
    [ObservableProperty]
    private SolidColorBrush _uploadButtonBrush = new SolidColorBrush(Colors.CornflowerBlue);
    
    [ObservableProperty]
    private SolidColorBrush _firstFilterButtonBrush = new SolidColorBrush(Colors.CornflowerBlue);

    public MainWindowViewModel(IFilePickerService filePickerService)
    {
        _filePickerService = filePickerService;
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
            }
        }
        catch (Exception ex)
        {
            // ignored
        }
    }
}