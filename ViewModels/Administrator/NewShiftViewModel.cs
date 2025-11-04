using CommunityToolkit.Mvvm.Input;
using cafeInformationSystem.Models.Entities;
using System.Collections.Generic;
using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using cafeInformationSystem.Views.Administrator;
using cafeInformationSystem.Models.DataBase;
using Avalonia.Media.Imaging;
using cafeInformationSystem.Models.DataBase.DataAccess;
using cafeInformationSystem.Models.Cryptography;
using cafeInformationSystem.Models.MediaService;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.ObjectModel;

namespace cafeInformationSystem.ViewModels.Administrator;

public class EmployeeShiftTable
{
    public string Username { get; set; } = string.Empty;
}

public partial class NewShiftViewModel : ViewModelBase
{
    public NewShiftViewModel()
    {
        BackToShiftsCommand = new RelayCommand(ExecuteBackToShifts);
        CreateShiftCommand = new RelayCommand(ExecuteCreateShift);
        AddEmployeeCommand = new RelayCommand(ExecuteAddEmployee);
        RemoveSelectedEmployeeCommand = new RelayCommand(ExecuteRemoveSelectedEmployee, CanExecuteRemoveSelectedEmployee);
    }

    private List<Employee> _employeeShits = new();

    private string _shiftCode = string.Empty;
    private DateTimeOffset _dateShift = DateTimeOffset.Now;
    private TimeSpan _startShift = new TimeSpan(8, 0, 0);
    private TimeSpan _endShiftFilter = new TimeSpan(16, 0, 0);

    public ObservableCollection<EmployeeShiftTable> _employeeShiftTable = new();

    private EmployeeShiftTable? _selectedEmployee;
    private string _errorMessage = string.Empty;

    public string ShiftCode
    {
        get => _shiftCode;
        set => SetProperty(ref _shiftCode, value);
    }

    // INFO! дата, на которую будет назначена смена
    public DateTimeOffset DateShift
    {
        get => _dateShift;
        set => SetProperty(ref _dateShift, value);
    }

    // INFO! время начала смены
    public TimeSpan StartShift
    {
        get => _startShift;
        set => SetProperty(ref _startShift, value);
    }

    // INFO! время конца смены
    public TimeSpan EndShift
    {
        get => _endShiftFilter;
        set => SetProperty(ref _endShiftFilter, value);
    }

    public ObservableCollection<EmployeeShiftTable> EmployeeShiftTable
    {
        get => _employeeShiftTable;
        set => SetProperty(ref _employeeShiftTable, value);
    }

    public EmployeeShiftTable? SelectedEmployee
    {
        get => _selectedEmployee;
        set
        {
            SetProperty(ref _selectedEmployee, value);
            // INFO! вызываем проверку на активность кнопки
            (RemoveSelectedEmployeeCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }
    }
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackToShiftsCommand { get; }
    public ICommand CreateShiftCommand { get; }
    public ICommand AddEmployeeCommand { get; }
    public ICommand RemoveSelectedEmployeeCommand { get; }

    private void ExecuteAddEmployee()
    {
        var newEmployee = new EmployeeShiftTable();
        EmployeeShiftTable.Add(newEmployee);
        // INFO! выделяем новую строку
        SelectedEmployee = newEmployee;
    }

    private void ExecuteRemoveSelectedEmployee()
    {
        if (SelectedEmployee != null)
        {
            EmployeeShiftTable.Remove(SelectedEmployee);
            SelectedEmployee = null;
        }
    }

    private bool CanExecuteRemoveSelectedEmployee()
    {
        return SelectedEmployee != null;
    }

    private void ExecuteBackToShifts()
    {
        Window window = new ShiftsWindow()
        {
            DataContext = new ShiftsViewModel()
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void ExecuteCreateShift()
    {
        if (!ValidateInput())
        {
            return;
        }

        var context = DatabaseService.GetContext();

        var timeStart = DateShift.Date + StartShift;
        var timeEnd = DateShift.Date + EndShift;

        var shift = new Shift
        {
            ShiftCode = ShiftCode,
            TimeStart = timeStart,
            TimeEnd = timeEnd,
            Employees = _employeeShits
        };

        context.Shift.Add(shift);

        try
        {
            context.SaveChanges();
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка сохранения смены";
            return;
        }

        ExecuteBackToShifts();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(ShiftCode) || ShiftCode.Length > 256)
        {
            ErrorMessage = "Обязательное поле код стола длинной не более 256 символов";
            return false;
        }

        var context = DatabaseService.GetContext();

        var shift = context.Shift.AsNoTracking().FirstOrDefault(s => s.ShiftCode == ShiftCode);

        if (shift is not null)
        {
            ErrorMessage = "Код смены не уникальный";
            return false;
        }

        if (EmployeeShiftTable.Count < 4 || EmployeeShiftTable.Count > 7)
        {
            ErrorMessage = "В смене должно быть от 4 до 7 сотрудников";
            return false;
        }

        var dateShift = DateShift.UtcDateTime;
        var currentTime = DateTime.Now;

        if (dateShift.Date < currentTime.Date || dateShift.Date > currentTime.Date.AddDays(5))
        {
            ErrorMessage = "Смены можно устанавливать только на 5 дней вперед";
            return false;
        }

        var startTime = DateShift.Date + StartShift;
        var endTime = DateShift.Date + EndShift;

        if (startTime >= endTime)
        {
            ErrorMessage = "Время начала смены должно быть раньше времени окончания";
            return false;
        }

        var shiftDuration = endTime - startTime;
        if (shiftDuration.TotalHours > 8)
        {
            ErrorMessage = "Смена должна длиться не более 8 часов";
            return false;
        }

        foreach(var employeeShift in EmployeeShiftTable)
        {
            try
            {
                var employee = UtilsDataAccess.GetEmployee(employeeShift.Username);

                if (employee.Role != EmployeeRole.Waiter && employee.Role != EmployeeRole.Chef)
                {
                    ErrorMessage = $"Сотрудника {employeeShift.Username} не является поваром и не является официантом";
                    return false;
                }
                _employeeShits.Add(employee);
            }
            catch (Exception)
            {
                ErrorMessage = $"Сотрудника {employeeShift.Username} не существует";
                return false;
            }
        }

        ErrorMessage = string.Empty;
        return true;
    }
}
