using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media;
using CannyFilter;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using SpectralImages;

namespace ImageFilters.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFilePickerService _filePickerService;

    [ObservableProperty] private SKBitmap _imageSource;

    private SKBitmap _backup;

    [ObservableProperty] private double _sliderValue = 0;

    private double _lastSliderValue = 0;

    private delegate Task SelectedFilter();

    private SelectedFilter? _selectedLiveFilter;

    private SkiaFilters? _currentFilter;

    private Color _selectedColor;

    private Color _defaultColor;

    private Uri _source;

    [ObservableProperty] private SolidColorBrush _filter1Brush;

    [ObservableProperty] private SolidColorBrush _filter2Brush;

    [ObservableProperty] private SolidColorBrush _filter3Brush;

    [ObservableProperty] private SolidColorBrush _filter4Brush;

    [ObservableProperty] private SolidColorBrush _filter5Brush;

    [ObservableProperty] private SolidColorBrush _stereoBrush;

    [ObservableProperty] private SolidColorBrush _cannyFilterBrush;

    [ObservableProperty] private SolidColorBrush _cannyRegionsBrush;

    [ObservableProperty] private bool _isImageLoaded;

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
        CannyFilterBrush = new SolidColorBrush(_defaultColor);
        CannyRegionsBrush = new SolidColorBrush(_defaultColor);
        StereoBrush = new SolidColorBrush(_defaultColor);
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
    private async Task ApplyStereo()
    {
        _currentFilter?.SaveChanges();
        _selectedLiveFilter = null;
        _selectedLiveFilter += UpdateStereo;
        DisableAllSelections();
        StereoBrush.Color = _selectedColor;
        await UpdateStereo();
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

    [RelayCommand]
    private async Task ApplyFilter6()
    {
        _currentFilter?.SaveChanges();
        _selectedLiveFilter = null;
        DisableAllSelections();
        if (_currentFilter is null) return;
        ImageSource = _currentFilter.Grayscale();
    }

    [RelayCommand]
    private async Task AdjustBrightnessUsingTarget()
    {
        _currentFilter?.SaveChanges();
        _selectedLiveFilter = null;
        DisableAllSelections();

        try
        {
            var source = await _filePickerService.PickImageAsync();
            if (source != null)
            {
                await using Stream stream = await source.OpenReadAsync();
                var reference = SKBitmap.Decode(stream);
                ImageSource = ImageAdjuster.AdjustBrightnessContrast(ImageSource, reference);
            }
        }
        catch (Exception ex)
        {
        }
    }

    [RelayCommand]
    private async Task ApplyCannyFilter()
    {
        _currentFilter?.SaveChanges();
        _selectedLiveFilter = null;
        DisableAllSelections();

        CannyDetector cannyDetector = new();
        CannyFilterBrush.Color = _selectedColor;
        var filledBitmap = await Task.Run(() => cannyDetector.DetectToBitmap(ImageSource));
        // ImageSource = ImageUtils.MergeBitmaps(ImageSource, filledBitmap);
        ImageSource = filledBitmap;
    }

    [RelayCommand]
    private async Task ApplyCannyRegionsFilter()
    {
        _currentFilter?.SaveChanges();
        _selectedLiveFilter = null;
        DisableAllSelections();

        var cannyDetector = new CannyDetector();
        CannyFilterBrush.Color = _selectedColor;

        var edgesBitmap = await Task.Run(() => cannyDetector.DetectToBitmap(ImageSource));
        var closedEdges = await Task.Run(() => Morphology.Close(edgesBitmap, radius: 10));
        var filledBitmap = await Task.Run(() => RegionFiller.FillRegions(closedEdges, new SKColor(100, 100, 100)));
        ImageSource = ImageUtils.MergeBitmaps(ImageSource, filledBitmap);
        ImageSource = filledBitmap;
    }


    private async Task UpdateFilter1()
    {
        if (_currentFilter is null) return;
        ImageSource = _currentFilter.Sepia((float)SliderValue);
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
        ImageSource = _currentFilter.Contrast((float)SliderValue);
    }

    private async Task UpdateFilter5()
    {
        if (_currentFilter is null) return;
        ImageSource = _currentFilter.Invert((float)SliderValue);
    }

    private async Task UpdateStereo()
    {
        if (_currentFilter is null) return;
        var shift = Convert.ToInt32(Math.Round(SkiaFilters.SmartClamp((float)SliderValue, 0f, 30f)));
        ImageSource = Stereo.CreateAnaglyphImage(_backup, shift);
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
                _source = source.Path;
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
    private async Task DetectObjects()
    {
        if (string.IsNullOrEmpty(_source.AbsolutePath) || !IsImageLoaded)
        {
            return;
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(2);

            using var formData = new MultipartFormDataContent();

            byte[] imageBytes;
            using (var fileStream = File.OpenRead(_source.AbsolutePath))
            {
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            formData.Add(imageContent, "image", Path.GetFileName(_source.AbsolutePath));

            var response = await httpClient.PostAsync("http://127.0.0.1:8000/detect/", formData);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<DetectionResult>(jsonResponse);

                if (result?.Status == "success" && !string.IsNullOrEmpty(result.DetectedImagePath))
                {
                    var processedImageResponse =
                        await httpClient.GetAsync(
                            $"http://127.0.0.1:8000/results/{Path.GetFileName(result.DetectedImagePath)}");

                    if (processedImageResponse.IsSuccessStatusCode)
                    {
                        var processedImageBytes = await processedImageResponse.Content.ReadAsByteArrayAsync();
                        using var stream = new MemoryStream(processedImageBytes);
                        ImageSource = SKBitmap.Decode(stream);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            //
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

    [RelayCommand]
    private async Task SaveImage()
    {
        try
        {
            var path = await _filePickerService.SaveImageAsync();
            if (path != null)
            {
                var extension = Path.GetExtension(path.Name).ToLower();
                await using var stream = await path.OpenWriteAsync();
                ImageSource.Encode(stream, SKEncodedImageFormat.Png, 300);
            }
        }
        catch (Exception e)
        {
        }
    }
}