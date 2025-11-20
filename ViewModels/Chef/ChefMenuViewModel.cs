using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using cafeInformationSystem.Views.Chef;

namespace cafeInformationSystem.ViewModels.Chef;

public enum ChefMenuNavigatePage : short
{
    Orders = 1
}

public class ChefMenuViewModel : ViewModelBase
{
    public ChefMenuViewModel()
    {
        NavigateToOrdersCommand = new RelayCommand(() => NavigateTo(ChefMenuNavigatePage.Orders));

    }

    public ICommand NavigateToOrdersCommand { get; }

    private void NavigateTo(ChefMenuNavigatePage navigatePage)
    {
        Window window = new ChefMenuWindow();

        switch (navigatePage)
        {
            case ChefMenuNavigatePage.Orders:
                window = new OrdersWindow()
                {
                    DataContext = new OrdersViewModel()
                };
                break;
        }

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }
}
