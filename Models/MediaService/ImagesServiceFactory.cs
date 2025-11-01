using System;

namespace cafeInformationSystem.Models.MediaService;

public static class ImagesMediaService
{
    private static IImagesService? _mediaService = null;

    public static IImagesService GetMediaService()
    {
        if (_mediaService == null)
        {
            var typeStorage = Environment.GetEnvironmentVariable("TYPE_STORAGE")!;
            switch (typeStorage)
            {
                case "local":
                    _mediaService = new LocalImagesService(Environment.GetEnvironmentVariable("PATH_LOCAL_MEDIA")!);
                    break;
                default:
                    throw new Exception("Storage type not found");
            }
        }
        
        return _mediaService;
    }
}
