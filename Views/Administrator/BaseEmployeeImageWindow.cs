using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace cafeInformationSystem.Views.Administrator;

public enum TypeImage : short
{
    Photo = 1,
    Contract = 2
}

public abstract class BaseEmployeeImageWindow : Window
{
    public event Action<string, string>? PhotoFileSelected;
    public event Action<string, string>? ContractFileSelected;

    protected abstract void UpdateDropZoneText(TypeImage type, string fileName);

    protected async Task SelectImage(TypeImage type, string title)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Изображения")
                {
                    Patterns = ["*.png", "*.jpg", "*.jpeg"],
                    MimeTypes = ["image/png", "image/jpeg"]
                }
            ]
        });

        if (files.Count > 0)
        {
            await HandleDroppedFile(files[0], type);
        }
    }

    protected async Task HandleDroppedFile(IStorageFile file, TypeImage type)
    {
        if (!IsImageFile(file.Name))
        {
            await ShowErrorDialog("Выберите файл в формате PNG, JPG, JPEG или PDF");
            return;
        }

        var filePath = file.Path.LocalPath;
        var fileName = file.Name;

        UpdateDropZoneText(type, fileName);

        if (type == TypeImage.Photo)
        {
            PhotoFileSelected?.Invoke(filePath, fileName);
        }
        else if (type == TypeImage.Contract)
        {
            ContractFileSelected?.Invoke(filePath, fileName);
        }

        await ShowSuccessDialog($"Файл {fileName} успешно загружен как {GetFileTypeName(type)}!");
    }

    protected async void OnPhotoDrop(object? sender, DragEventArgs e)
    {
        var files = e.Data.GetFiles();
        if (files is not null && files.Any())
        {
            var file = files.First();
            if (file is IStorageFile storageFile)
            {
                await HandleDroppedFile(storageFile, TypeImage.Photo);
            }
        }
    }

    protected async void OnContractDrop(object? sender, DragEventArgs e)
    {
        var files = e.Data.GetFiles();
        if (files is not null && files.Any())
        {
            var file = files.First();
            if (file is IStorageFile storageFile)
            {
                await HandleDroppedFile(storageFile, TypeImage.Photo);
            }
        }
    }

    protected void OnDragOver(object? sender, DragEventArgs e)
    {
        var hasValidFiles = e.Data.GetFiles()?.Any(file => file is IStorageFile storageFile && IsImageFile(storageFile.Name)) == true;

        e.DragEffects = hasValidFiles ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    protected bool IsImageFile(string fileName)
    {
        var extensions = new[] { ".png", ".jpg", ".jpeg" };
        return extensions.Any(ext => fileName.ToLower().EndsWith(ext));
    }

    protected async Task ShowErrorDialog(string message)
    {
        var dialog = new Window
        {
            Title = "Ошибка",
            Content = new TextBlock { Text = message },
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        await dialog.ShowDialog(this);
    }

    protected async Task ShowSuccessDialog(string message)
    {
        var dialog = new Window
        {
            Title = "Успех",
            Content = new TextBlock { Text = message },
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        await dialog.ShowDialog(this);
    }

    protected string GetFileTypeName(TypeImage type)
    {
        return type switch
        {
            TypeImage.Photo => "фото",
            TypeImage.Contract => "скан договора", 
            _ => "файл"
        };
    }
}