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

public partial class OrderItemsViewModel : ViewModelBase
{
    public OrderItemsViewModel()
    {
        BackToAdministratorMenuCommand = new RelayCommand(ExecuteBackToAdministratorMenu);
        NewOrderItemCommand = new RelayCommand(ExecuteNewOrderItem);
        ApplyFiltersCommand = new RelayCommand(ExecuteApplyFilters);
        OpenOrderItemCardCommand = new RelayCommand<string?>(ExecuteOpenOrderItemCard);
    }

    private string _nameFilter = string.Empty;
    private decimal? _minCostFilter = null;
    private decimal? _maxCostFilter = null;

    // INFO! ObservableCollection используется для ослеживания действий со списоком (Добавлени, изменение, удаление и так далее)
    // в данном случае можно было и просто List или ICollection/ использовать...
    public ObservableCollection<OrderItem> _orderItems = new();

    private string _errorMessage = string.Empty;

    public string NameFilter
    {
        get => _nameFilter;
        set => SetProperty(ref _nameFilter, value);
    }

    public decimal? MinCostFilter
    {
        get => _minCostFilter;
        set => SetProperty(ref _minCostFilter, value);
    }

    public decimal? MaxCostFilter
    {
        get => _maxCostFilter;
        set => SetProperty(ref _maxCostFilter, value);
    }

    public ObservableCollection<OrderItem> OrderItems
    {
        get => _orderItems;
        private set
        {
            if (SetProperty(ref _orderItems, value))
            {
                OnPropertyChanged(nameof(HasNoOrderItems));
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackToAdministratorMenuCommand { get; }
    public ICommand NewOrderItemCommand { get; }
    public ICommand ApplyFiltersCommand { get; }
    public ICommand OpenOrderItemCardCommand { get; }

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
        LoadOrderItems();
    }

    private void ExecuteNewOrderItem()
    {
        Window window = new NewOrderItemWindow()
        {
            DataContext = new NewOrderItemViewModel()
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void ExecuteOpenOrderItemCard(string? nameOrderItem)
    {
        if (string.IsNullOrEmpty(nameOrderItem))
        {
            throw new Exception("Name order item is null or empty");
        }
        
        Window window = new ChangeOrderItemWindow()
        {
            DataContext = new ChangeOrderItemViewModel(nameOrderItem)
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void LoadOrderItems()
    {
        try
        {
            var context = DatabaseService.GetContext();

            var query = context.OrderItem.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(NameFilter))
            {
                query = query.Where(oi => oi.Name.Contains(NameFilter));
            }

            if (MinCostFilter is not null)
            {
                query = query.Where(oi => oi.Cost > MinCostFilter);
            }

            if (MaxCostFilter is not null)
            {
                query = query.Where(oi => oi.Cost < MaxCostFilter);
            }

            var orderItems = query.ToList();

            OrderItems = new ObservableCollection<OrderItem>(orderItems);
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка загрузки товаров";
            OrderItems = new();
        }
    }

    public bool HasNoOrderItems => OrderItems.Count == 0;
}
