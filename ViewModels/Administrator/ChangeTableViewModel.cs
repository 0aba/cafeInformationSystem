using CommunityToolkit.Mvvm.Input;
using cafeInformationSystem.Models.Entities;
using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using cafeInformationSystem.Views.Administrator;
using cafeInformationSystem.Models.DataBase;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace cafeInformationSystem.ViewModels.Administrator;

public partial class ChangeTableViewModel : ViewModelBase
{
    private Table _changeTable;

    public ChangeTableViewModel(string tableCode)
    {
        var context = DatabaseService.GetContext();

        var changeTable = context.Table.Include(t => t.WaiterService).FirstOrDefault(t => t.TableCode == tableCode);

        if (changeTable is null)
        {
            throw new Exception("Table card does not exist");
        }

        _changeTable = changeTable;

        TableCode = _changeTable.TableCode;
        UsernameWaiter = _changeTable.WaiterService?.Username;

        BackToTablesCommand = new RelayCommand(ExecuteBackToTables);
        ChangeTableCommand = new RelayCommand(ExecuteChangeTable);
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
    public ICommand ChangeTableCommand { get; }

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

    private void ExecuteChangeTable()
    {
        if (!ValidateInput())
        {
            return;
        }

        var context = DatabaseService.GetContext();
        var employee = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == UsernameWaiter);

        _changeTable.TableCode = TableCode;
        _changeTable.WaiterServiceId = employee?.Id;

        context.Table.Update(_changeTable);

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
        var table = context.Table.AsNoTracking().FirstOrDefault(e => e.TableCode == TableCode);

        if (TableCode != _changeTable.TableCode && table is not null)
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
