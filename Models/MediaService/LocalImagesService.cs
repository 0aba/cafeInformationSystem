using System;
using System.IO;
using Avalonia.Media.Imaging;

namespace cafeInformationSystem.Models.MediaService;

public class LocalImagesService : IImagesService
{
    private readonly string _ABSOLUTE_PATH_LOCAL_STORAGE;

    public LocalImagesService(string pathLocalStorage)
    {
        _ABSOLUTE_PATH_LOCAL_STORAGE = Path.GetFullPath(pathLocalStorage);

        try
        {
            bool directoryExists = Directory.Exists(_ABSOLUTE_PATH_LOCAL_STORAGE);

            if (directoryExists)
            {
                CheckRootDirPermissions(_ABSOLUTE_PATH_LOCAL_STORAGE);
            }
            else
            {
                Directory.CreateDirectory(_ABSOLUTE_PATH_LOCAL_STORAGE);
                CheckRootDirPermissions(_ABSOLUTE_PATH_LOCAL_STORAGE);
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to create folder for local media: {e.Message}", e);
        }
    }

    private void CheckRootDirPermissions(string dirPath)
    {
        string testFile = Path.Combine(dirPath, "test_access_permissions.tmp");

        File.WriteAllText(testFile, "test");
        File.Delete(testFile);
    }

    public override void SaveImage(string mediaPathToDir, string fullFileName, Bitmap image)
    {
        try
        {
            string fullMediaPath = Path.Combine(_ABSOLUTE_PATH_LOCAL_STORAGE, mediaPathToDir);

            if (!Directory.Exists(fullMediaPath))
            {
                Directory.CreateDirectory(fullMediaPath);
            }

            string fullFilePath = Path.Combine(fullMediaPath, fullFileName);

            using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
            {
                image.Save(fileStream);
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to save image: {e.Message}", e);
        }
    }

    public override Bitmap GetImage(string mediaPathToImage)
    {
        try
        {
            string fullMediaPath = Path.Combine(_ABSOLUTE_PATH_LOCAL_STORAGE, mediaPathToImage);

            if (!File.Exists(fullMediaPath))
            {
                throw new FileNotFoundException("Image not found");
            }
            
            return new Bitmap(fullMediaPath);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to load image: {e.Message}", e);
        }
    }

    public override void DeleteImage(string mediaPathToImage)
    {
        try
        {
            string fullMediaPath = Path.Combine(_ABSOLUTE_PATH_LOCAL_STORAGE, mediaPathToImage);

            if (!File.Exists(fullMediaPath))
            {
                throw new FileNotFoundException("Image not found");
            }
            
            File.Delete(fullMediaPath);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to delete image: {e.Message}", e);
        }
    }
}
