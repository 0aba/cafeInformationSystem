using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
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

public partial class ShiftsViewModel : ViewModelBase
{
    public ShiftsViewModel()
    {
        BackToAdministratorMenuCommand = new RelayCommand(ExecuteBackToAdministratorMenu);
        NewShiftCommand = new RelayCommand(ExecuteNewShift);
        ApplyFiltersCommand = new RelayCommand(ExecuteApplyFilters);
        OpenShiftCardCommand = new RelayCommand<string?>(ExecuteOpenShiftCard);

        MinStartShiftFilter = DateTimeOffset.Now.AddMonths(-1);
        MaxEndShiftFilter = DateTimeOffset.Now.AddMonths(1);
    }

    private string _shiftCodeFilter = string.Empty;
    private DateTimeOffset _minStartShiftFilter = new();
    private DateTimeOffset _maxEndShiftFilter = new();
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

    public ICommand BackToAdministratorMenuCommand { get; }
    public ICommand NewShiftCommand { get; }
    public ICommand ApplyFiltersCommand { get; }
    public ICommand OpenShiftCardCommand { get; }

    private void ExecuteBackToAdministratorMenu()
    {
        Window window = new AdministratorMenuWindow()
        {
            DataContext = new AdministratorMenuViewModel()
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
        LoadTables();
    }

    private void ExecuteNewShift()
    {
        Window window = new NewShiftWindow()
        {
            DataContext = new NewShiftViewModel()
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void ExecuteOpenShiftCard(string? shiftCode)
    {
        // TODO!
        // if (string.IsNullOrEmpty(tableCode))
        // {
        //     throw new Exception("Table code is null or empty");
        // }
        
        // Window window = new ChangeTableWindow()
        // {
        //     DataContext = new ChangeTableViewModel(tableCode)
        // };

        // if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        // {
        //     var currentWindow = desktop.MainWindow;

        //     desktop.MainWindow = window;
        //     desktop.MainWindow.Show();

        //     currentWindow?.Close();
        // }
    }

    private void LoadTables()
    {
        try
        {
            var context = DatabaseService.GetContext();

            var query = context.Shift.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(ShiftCodeFilter))
            {
                query = query.Where(s => s.ShiftCode.Contains(ShiftCodeFilter));
            }

            query = query.Where(s => s.TimeStart > MinStartShiftFilter.UtcDateTime);
            query = query.Where(s => s.TimeEnd < MaxEndShiftFilter.UtcDateTime);

            var currentTime = DateTimeOffset.UtcNow;
            query = query.Where(s => CompletionStatusFilter ? s.TimeEnd < currentTime : s.TimeEnd > currentTime);

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
