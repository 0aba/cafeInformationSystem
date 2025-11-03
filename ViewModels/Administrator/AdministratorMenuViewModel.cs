using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using cafeInformationSystem.Views.Administrator;
using cafeInformationSystem.ViewModels.Administrator;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace cafeInformationSystem.ViewModels.Administrator;

public enum AdministratorMenuNavigatePage : short
{
    Employees = 1,
    Orders = 2,
    Shifts = 3,
    Reports = 4
}

public class AdministratorMenuViewModel : ViewModelBase
{
    public AdministratorMenuViewModel()
    {
        NavigateToEmployeesCommand = new RelayCommand(() => NavigateTo(AdministratorMenuNavigatePage.Employees));
        NavigateToOrdersCommand = new RelayCommand(() => NavigateTo(AdministratorMenuNavigatePage.Orders));
        NavigateToShiftsCommand = new RelayCommand(() => NavigateTo(AdministratorMenuNavigatePage.Shifts));
        NavigateToReportsCommand = new RelayCommand(() => NavigateTo(AdministratorMenuNavigatePage.Reports));
    }

    public ICommand NavigateToEmployeesCommand { get; }
    public ICommand NavigateToOrdersCommand { get; }
    public ICommand NavigateToShiftsCommand { get; }
    public ICommand NavigateToReportsCommand { get; }

    private void NavigateTo(AdministratorMenuNavigatePage navigatePage)
    {
        Window window = new AdministratorMenuWindow();

        switch (navigatePage)
        {
            case AdministratorMenuNavigatePage.Employees:
                window = new EmployeesWindow()
                {
                    DataContext = new EmployeesViewModel()
                };
                break;
            case AdministratorMenuNavigatePage.Orders:
                // TODO! 2
                break;
            case AdministratorMenuNavigatePage.Shifts:
                // TODO! 3
                break;
            case AdministratorMenuNavigatePage.Reports:
                // TODO! 4
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
