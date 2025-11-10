using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using cafeInformationSystem.Views.Waiter;
using cafeInformationSystem.ViewModels.Waiter;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace cafeInformationSystem.ViewModels.Waiter;

public enum WaiterMenuNavigatePage : short
{
    OrderItems = 1,
    Orders = 2
}

public class WaiterMenuViewModel : ViewModelBase
{
    public WaiterMenuViewModel()
    {
        NavigateToOrderItemsCommand = new RelayCommand(() => NavigateTo(WaiterMenuNavigatePage.OrderItems));
        NavigateToOrdersCommand = new RelayCommand(() => NavigateTo(WaiterMenuNavigatePage.Orders));
    }

    public ICommand NavigateToOrderItemsCommand { get; }
    public ICommand NavigateToOrdersCommand { get; }

    private void NavigateTo(WaiterMenuNavigatePage navigatePage)
    {
        Window window = new WaiterMenuWindow();

        switch (navigatePage)
        {
            case WaiterMenuNavigatePage.OrderItems:
                window = new OrderItemsWindow()
                {
                    DataContext = new OrderItemsViewModel()
                };
                break;
            case WaiterMenuNavigatePage.Orders:
                // TODO! 
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
