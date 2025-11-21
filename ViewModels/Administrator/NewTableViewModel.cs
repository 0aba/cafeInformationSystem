using CommunityToolkit.Mvvm.Input;
using cafeInformationSystem.Models.Entities;
using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using cafeInformationSystem.Views.Administrator;
using cafeInformationSystem.Models.DataBase;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace cafeInformationSystem.ViewModels.Administrator;

public partial class NewTableViewModel : ViewModelBase
{
    public NewTableViewModel()
    {
        BackToTablesCommand = new RelayCommand(ExecuteBackToTables);
        CreateTableCommand = new RelayCommand(ExecuteCreateTable);
    }

    private string _tableCode = string.Empty;

    private string? _usernameWaiter = string.Empty;

    private string _errorMessage = string.Empty;

    public string TableCode
    {
        get => _tableCode;
        set => SetProperty(ref _tableCode, value);
    }

    public string? UsernameWaiter
    {
        get => _usernameWaiter;
        set => SetProperty(ref _usernameWaiter, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackToTablesCommand { get; }
    public ICommand CreateTableCommand { get; }

    private void ExecuteBackToTables()
    {
        Window window = new TablesWindow()
        {
            DataContext = new TablesViewModel()
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void ExecuteCreateTable()
    {
        if (!ValidateInput())
        {
            return;
        }

        var context = DatabaseService.GetContext();
        var employee = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == UsernameWaiter);

        var table = new Table
        {
            TableCode = TableCode,
            WaiterServiceId = employee?.Id
        };

        context.Table.Add(table);

        try
        {
            context.SaveChanges();
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка сохранения столика";
            return;
        }

        ExecuteBackToTables();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(TableCode) || TableCode.Length > 256)
        {
            ErrorMessage = "Обязательное поле код стола длинной не более 256 символов";
            return false;
        }
        
        var context = DatabaseService.GetContext();

        var table = context.Table.AsNoTracking().FirstOrDefault(t => t.TableCode == TableCode);

        if (table is not null)
        {
            ErrorMessage = "Код стола не уникальный";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(UsernameWaiter))
        {
            var employee = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == UsernameWaiter);

            if (employee is null)
            {
                ErrorMessage = "Сотрудник с таким логином не существует";
                return false;
            }

            if (employee.Role != EmployeeRole.Waiter)
            {
                ErrorMessage = "Только официант может быть назначен за столик";
                return false;
            }
        }

        ErrorMessage = string.Empty;
        return true;
    }
}
