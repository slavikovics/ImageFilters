using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace ImageFilters;

public interface IFilePickerService
{
    public Task<IStorageFile?> PickImageAsync();

    public Task<Bitmap> LoadImageFromStorageFileAsync(IStorageFile storageFile);
}