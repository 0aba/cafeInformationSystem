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
    Shifts = 2,
    Tables = 3,
    Orders = 4,
    Reports = 5
}

public class AdministratorMenuViewModel : ViewModelBase
{
    public AdministratorMenuViewModel()
    {
        NavigateToEmployeesCommand = new RelayCommand(() => NavigateTo(AdministratorMenuNavigatePage.Employees));
        NavigateToTablesCommand = new RelayCommand(() => NavigateTo(AdministratorMenuNavigatePage.Tables));
        NavigateToShiftsCommand = new RelayCommand(() => NavigateTo(AdministratorMenuNavigatePage.Shifts));
        NavigateToOrdersCommand = new RelayCommand(() => NavigateTo(AdministratorMenuNavigatePage.Orders));
        NavigateToReportsCommand = new RelayCommand(() => NavigateTo(AdministratorMenuNavigatePage.Reports));
    }

    public ICommand NavigateToEmployeesCommand { get; }
    public ICommand NavigateToTablesCommand { get; }
    public ICommand NavigateToShiftsCommand { get; }
    public ICommand NavigateToOrdersCommand { get; }
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
            case AdministratorMenuNavigatePage.Tables:
                window = new TablesWindow()
                {
                    DataContext = new TablesViewModel()
                };
                break;
            case AdministratorMenuNavigatePage.Shifts:
                window = new ShiftsWindow()
                {
                    DataContext = new ShiftsViewModel()
                };
                break;
            case AdministratorMenuNavigatePage.Orders:
                // TODO! 2
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
