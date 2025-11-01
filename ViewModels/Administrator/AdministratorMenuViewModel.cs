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

namespace cafeInformationSystem.ViewModels.Administrator;

public class AdministratorMenuViewModel : ViewModelBase
{
    public AdministratorMenuViewModel()
    {
        //LogoutCommand = new RelayCommand(ExecuteLogout);
    }


    //public ICommand LogoutCommand { get; }

    // private void ExecuteLogout()
    // {
        
    // }

}
