using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using cafeInformationSystem.Models.AuthService;
using Avalonia;
using cafeInformationSystem.Views.Shared;
using Avalonia.Controls.ApplicationLifetimes;
using cafeInformationSystem.Models.MediaService;
using System;
using Avalonia.Platform;

namespace cafeInformationSystem.ViewModels.ControlsApp;

public class HeaderViewModel : ViewModelBase
{
    private const string _CAFE_LOGO_ASSETS_PATH = "avares://cafeInformationSystem/Assets/cafe-information-system-logo.ico";
    private const string _DEFAULT_PHOTO_USER_ASSETS_PATH = "avares://cafeInformationSystem/Assets/Default/default-employee-photo.png";

    public HeaderViewModel()
    {
        using var stream = AssetLoader.Open(new Uri(_CAFE_LOGO_ASSETS_PATH));
        AppIcon = new Bitmap(stream);
                
        LogoutCommand = new RelayCommand(ExecuteLogout);
        LoadUserPhoto();

        AuthStorage.OnCurrentUserChanged += OnUserChanged;
    }

    public Bitmap AppIcon { get; }
    private Bitmap? _userPhoto;

    public Bitmap? UserPhoto
    {
        get => _userPhoto;
        set => SetProperty(ref _userPhoto, value);
    }

    public ICommand LogoutCommand { get; }

    private void ExecuteLogout()
    {
        AuthStorage.LogOut();

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = new LoginWindow();
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }
    
    private void OnUserChanged(object? sender, EventArgs e)
    {
        LoadUserPhoto();
    }

    private void LoadUserPhoto()
    {
        if (AuthStorage.CurrentUser?.Photo is not null)
        {
            try
            {
                UserPhoto = ImagesMediaService.GetMediaService().GetImage(AuthStorage.CurrentUser.Photo);
            }
            catch (Exception e)
            {
                using var stream = AssetLoader.Open(new Uri(_DEFAULT_PHOTO_USER_ASSETS_PATH));
                UserPhoto = new Bitmap(stream);
                Console.WriteLine($"Unable to download user photo from media service: {e.Message}", e);
            }
        }
        else
        {
            using var stream = AssetLoader.Open(new Uri(_DEFAULT_PHOTO_USER_ASSETS_PATH));
            UserPhoto = new Bitmap(stream);
        }
    }
}
