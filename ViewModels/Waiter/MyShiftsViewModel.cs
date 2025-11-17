using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using cafeInformationSystem.Models.Entities;
using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using cafeInformationSystem.Models.DataBase;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using cafeInformationSystem.Views.Waiter;
using cafeInformationSystem.Models.AuthService;

namespace cafeInformationSystem.ViewModels.Waiter;

public partial class MyShiftsViewModel : ViewModelBase
{
    public MyShiftsViewModel()
    {
        BackToWaiterMenuCommand = new RelayCommand(ExecuteBackToWaiterMenu);
        ApplyFiltersCommand = new RelayCommand(ExecuteApplyFilters);
    }

    private string _shiftCodeFilter = string.Empty;
    private DateTimeOffset _minStartShiftFilter = DateTimeOffset.Now.AddMonths(-1);
    private DateTimeOffset _maxEndShiftFilter = DateTimeOffset.Now.AddMonths(1);
    private bool _completionStatusFilter  = false;

    // INFO! ObservableCollection используется для ослеживания действий со списоком (Добавлени, изменение, удаление и так далее)
    // в данном случае можно было и просто List или ICollection/ использовать...
    public ObservableCollection<Shift> _shifts = new();

    private string _errorMessage = string.Empty;

    public string ShiftCodeFilter
    {
        get => _shiftCodeFilter;
        set => SetProperty(ref _shiftCodeFilter, value);
    }

    public DateTimeOffset MinStartShiftFilter
    {
        get => _minStartShiftFilter;
        set => SetProperty(ref _minStartShiftFilter, value);
    }

    public DateTimeOffset MaxEndShiftFilter
    {
        get => _maxEndShiftFilter;
        set => SetProperty(ref _maxEndShiftFilter, value);
    }

    public bool CompletionStatusFilter
    {
        get => _completionStatusFilter;
        set => SetProperty(ref _completionStatusFilter, value);
    }

    public ObservableCollection<Shift> Shifts
    {
        get => _shifts;
        private set
        {
            if (SetProperty(ref _shifts, value))
            {
                OnPropertyChanged(nameof(HasNoShifts));
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackToWaiterMenuCommand { get; }
    public ICommand ApplyFiltersCommand { get; }

    private void ExecuteBackToWaiterMenu()
    {
        Window window = new WaiterMenuWindow()
        {
            DataContext = new WaiterMenuViewModel()
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void ExecuteApplyFilters()
    {
        LoadShifts();
    }

    private void LoadShifts()
    {
        try
        {
            var context = DatabaseService.GetContext();

            var query = context.Shift.Include(s => s.Employees).AsNoTracking().AsQueryable();

            var currentUser = AuthStorage.CurrentUser;
            query = query.Where(s => s.Employees.Any(e => e.Id == currentUser!.Id));

            if (!string.IsNullOrWhiteSpace(ShiftCodeFilter))
            {
                query = query.Where(s => s.ShiftCode.Contains(ShiftCodeFilter));
            }

            query = query.Where(s => s.TimeStart >= MinStartShiftFilter.UtcDateTime);
            query = query.Where(s => s.TimeEnd <= MaxEndShiftFilter.UtcDateTime);

            var currentTime = DateTime.SpecifyKind(DateTimeOffset.Now.DateTime, DateTimeKind.Utc);

            query = query.Where(s => CompletionStatusFilter ? s.TimeEnd < currentTime : !(s.TimeEnd < currentTime));

            var shifts = query.ToList();

            Shifts = new ObservableCollection<Shift>(shifts);
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка загрузки столиков";
            Shifts = new();
        }
    }

    public bool HasNoShifts => Shifts.Count == 0;
}
