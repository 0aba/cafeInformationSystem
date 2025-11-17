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

public partial class MyTablesViewModel : ViewModelBase
{
    public MyTablesViewModel()
    {
        BackToWaiterMenuCommand = new RelayCommand(ExecuteBackToWaiterMenu);
        ApplyFiltersCommand = new RelayCommand(ExecuteApplyFilters);
    }

    private string _tableCodeFilter = string.Empty;

    // INFO! ObservableCollection используется для ослеживания действий со списоком (Добавлени, изменение, удаление и так далее)
    // в данном случае можно было и просто List или ICollection/ использовать...
    public ObservableCollection<Table> _tables = new();

    private string _errorMessage = string.Empty;

    public string TableCodeFilter
    {
        get => _tableCodeFilter;
        set => SetProperty(ref _tableCodeFilter, value);
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
        LoadTables();
    }

    private void LoadTables()
    {
        try
        {
            var context = DatabaseService.GetContext();

            var query = context.Table.Include(t => t.WaiterService).Include(t => t.WaiterService)
                                     .AsNoTracking().AsQueryable();

            var currentUser = AuthStorage.CurrentUser;
            query.Where(t => t.WaiterServiceId == currentUser!.Id);

            if (!string.IsNullOrWhiteSpace(TableCodeFilter))
            {
                query = query.Where(t => t.TableCode.Contains(TableCodeFilter));
            }

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
