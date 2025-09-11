using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using SkiaSharp;

namespace ImageFilters;

public class FilePickerService : IFilePickerService
{
    private Window _mainWindow;
    
    public FilePickerService(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }
    
    public async Task<IStorageFile?> PickImageAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(_mainWindow);
            if (topLevel == null)
            {
                return null;
            }
                
            var filePickerOptions = new FilePickerOpenOptions
            {
                Title = "Select an image",
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new("Image Files")
                    {
                        Patterns =
                        [
                            "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", 
                            "*.tiff", "*.webp", "*.ico"
                        ],
                        AppleUniformTypeIdentifiers = ["public.image"],
                        MimeTypes = ["image/*"]
                    },
                    new("All Files") { Patterns = ["*"] }
                },
                AllowMultiple = false
            };
                
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(filePickerOptions);

            if (files.Count > 0 && files[0] is { } selectedFile)
            {
                return selectedFile;
            }
        }
        catch (Exception ex)
        {
        }
            
        return null;
    }
    
    public async Task<IStorageFile?> SaveImageAsync()
    {
        var topLevel = TopLevel.GetTopLevel(_mainWindow);
        if (topLevel == null) return null;
        
        var fileType = new FilePickerFileType("Image")
        {
            Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" },
            AppleUniformTypeIdentifiers = new[] { "image.png" },
            MimeTypes = new[] { "png/*", "jpg/*", "jpeg/*", "bmp/*", "gif/*" }
        };

        var options = new FilePickerSaveOptions
        {
            Title = "Save Image",
            FileTypeChoices = new[] { fileType },
            DefaultExtension = "png",
            ShowOverwritePrompt = true
        };
        
        return await topLevel.StorageProvider.SaveFilePickerAsync(options);
    }

    public async Task<Bitmap> LoadImageFromStorageFileAsync(IStorageFile storageFile)
    {
        if (storageFile == null) throw new ArgumentNullException(nameof(storageFile));

        using var stream = await storageFile.OpenReadAsync();
        var bitmap = new Bitmap(stream);
        return bitmap;
    }
}