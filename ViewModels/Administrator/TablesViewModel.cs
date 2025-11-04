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

public partial class TablesViewModel : ViewModelBase
{
    public TablesViewModel()
    {
        BackToAdministratorMenuCommand = new RelayCommand(ExecuteBackToAdministratorMenu);
        NewTableCommand = new RelayCommand(ExecuteNewTable);
        ApplyFiltersCommand = new RelayCommand(ExecuteApplyFilters);
        OpenTableCardCommand = new RelayCommand<string?>(ExecuteOpenTableCard);
    }

    private string _tableCodeFilter = string.Empty;

    private string _usernameWaiterFilter = string.Empty;
    private bool _statusAssignedWaiterFilter = true;

    // INFO! ObservableCollection используется для ослеживания действий со списоком (Добавлени, изменение, удаление и так далее)
    // в данном случае можно было и просто List или ICollection/ использовать...
    public ObservableCollection<Table> _tables = new();

    private string _errorMessage = string.Empty;

    public string TableCodeFilter
    {
        get => _tableCodeFilter;
        set => SetProperty(ref _tableCodeFilter, value);
    }

    public string UsernameWaiterFilter
    {
        get => _usernameWaiterFilter;
        set => SetProperty(ref _usernameWaiterFilter, value);
    }

    public bool StatusAssignedWaiterFilter
    {
        get => _statusAssignedWaiterFilter;
        set => SetProperty(ref _statusAssignedWaiterFilter, value);
    }

    public ObservableCollection<Table> Tables
    {
        get => _tables;
        private set
        {
            if (SetProperty(ref _tables, value))
            {
                OnPropertyChanged(nameof(HasNoTables));
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackToAdministratorMenuCommand { get; }
    public ICommand NewTableCommand { get; }
    public ICommand ApplyFiltersCommand { get; }
    public ICommand OpenTableCardCommand { get; }

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

    private void ExecuteNewTable()
    {
        Window window = new NewTableWindow()
        {
            DataContext = new NewTableViewModel()
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void ExecuteOpenTableCard(string? tableCode)
    {
        if (string.IsNullOrEmpty(tableCode))
        {
            throw new Exception("Table code is null or empty");
        }
        
        Window window = new ChangeTableWindow()
        {
            DataContext = new ChangeTableViewModel(tableCode)
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void LoadTables()
    {
        try
        {
            var context = DatabaseService.GetContext();

            var query = context.Table.Include(t => t.WaiterService)
                                     .AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(TableCodeFilter))
            {
                query = query.Where(t => t.TableCode.Contains(TableCodeFilter));
            }

            if (!string.IsNullOrWhiteSpace(UsernameWaiterFilter))
            {
                query = query.Where(t => t.WaiterService != null && t.WaiterService.Username.Contains(UsernameWaiterFilter));
            }
            
            query = query.Where(e => StatusAssignedWaiterFilter ? e.WaiterService != null : e.WaiterService == null);


            var tables = query.ToList();

            Tables = new ObservableCollection<Table>(tables);
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка загрузки столиков";
            Tables = new();
        }
    }

    public bool HasNoTables => Tables.Count == 0;
}
