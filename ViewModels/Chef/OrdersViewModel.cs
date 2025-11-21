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
using System.Collections.Generic;
using cafeInformationSystem.ViewModels.Shared;
using cafeInformationSystem.Models.AuthService;
using cafeInformationSystem.Views.Chef;

namespace cafeInformationSystem.ViewModels.Chef;

public partial class OrdersViewModel : ViewModelBase
{
    public OrdersViewModel()
    {
        SelectedStatusCookingOrderFilter = AvailableStatusCookingOrder[0];
        SelectedAcceptedChefStatusFilter = AvailableAcceptedChefStatusOrder[0];

        BackToChefMenuCommand = new RelayCommand(ExecuteBackToChefMenu);
        ApplyFiltersCommand = new RelayCommand(ExecuteApplyFilters);
        OpenOrderCardCommand = new RelayCommand<string?>(ExecuteOpenOrderCard);
    }

    private string _orderCodeFilter = string.Empty;
    private DateTimeOffset _minCreatedAtFilter = DateTimeOffset.Now.AddMonths(-1);
    private DateTimeOffset _maxCreatedAtFilter = DateTimeOffset.Now.AddMonths(1);
    private int? _minAmountClientsFilter;
    private int? _maxAmountClientsFilter;

    public List<OrderCookingStatusFilterItem> AvailableStatusCookingOrder { get; } = new()
    {
        new OrderCookingStatusFilterItem { Name = "Все", Cooked = null },
        new OrderCookingStatusFilterItem { Name = "Готов", Cooked = true },
        new OrderCookingStatusFilterItem { Name = "Готовится", Cooked = false }
    };
    private OrderCookingStatusFilterItem? _selectedStatusCookingOrderFilter;

    public List<AcceptedChefStatusFilterItem> AvailableAcceptedChefStatusOrder { get; } = new()
    {
        new AcceptedChefStatusFilterItem { Name = "Все", Accepted = null },
        new AcceptedChefStatusFilterItem { Name = "Принятые мой", Accepted = true },
        new AcceptedChefStatusFilterItem { Name = "Никем не принятые", Accepted = false }
    };
    private AcceptedChefStatusFilterItem? _selectedAcceptedChefStatusFilter;

    // INFO! ObservableCollection используется для ослеживания действий со списоком (Добавлени, изменение, удаление и так далее)
    // в данном случае можно было и просто List или ICollection/ использовать...
    public ObservableCollection<Order> _orders = new();

    private string _errorMessage = string.Empty;

    public string OrderCodeFilter
    {
        get => _orderCodeFilter;
        set => SetProperty(ref _orderCodeFilter, value);
    }

    public DateTimeOffset MinCreatedAtFilter
    {
        get => _minCreatedAtFilter;
        set => SetProperty(ref _minCreatedAtFilter, value);
    }

    public DateTimeOffset MaxCreatedAtFilter
    {
        get => _maxCreatedAtFilter;
        set => SetProperty(ref _maxCreatedAtFilter, value);
    }

    public int? MinAmountClientsFilter
    {
        get => _minAmountClientsFilter;
        set => SetProperty(ref _minAmountClientsFilter, value);
    }

    public int? MaxAmountClientsFilter
    {
        get => _maxAmountClientsFilter;
        set => SetProperty(ref _maxAmountClientsFilter, value);
    }

    public OrderCookingStatusFilterItem? SelectedStatusCookingOrderFilter
    {
        get => _selectedStatusCookingOrderFilter;
        set => SetProperty(ref _selectedStatusCookingOrderFilter, value);
    }

    public AcceptedChefStatusFilterItem? SelectedAcceptedChefStatusFilter
    {
        get => _selectedAcceptedChefStatusFilter;
        set => SetProperty(ref _selectedAcceptedChefStatusFilter, value);
    }

    public ObservableCollection<Order> Orders
    {
        get => _orders;
        private set
        {
            if (SetProperty(ref _orders, value))
            {
                OnPropertyChanged(nameof(HasNoOrders));
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackToChefMenuCommand { get; }
    public ICommand ApplyFiltersCommand { get; }
    public ICommand OpenOrderCardCommand { get; }

    private void ExecuteBackToChefMenu()
    {
        Window window = new ChefMenuWindow()
        {
            DataContext = new ChefMenuViewModel()
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
        LoadOrders();
    }

    private void ExecuteOpenOrderCard(string? orderCode)
    {
        if (string.IsNullOrEmpty(orderCode))
        {
            throw new Exception("Order code is null or empty");
        }

        Window window = new ChangeOrderWindow()
        {
            DataContext = new ChangeOrderViewModel(orderCode)
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void LoadOrders()
    {
        try
        {
            var context = DatabaseService.GetContext();

            var query = context.Order.Include(o => o.Waiter).Include(o => o.Chef)
                                     .Include(o => o.Shift).ThenInclude(s => s.Employees)
                                     .AsNoTracking().AsQueryable();
            
            var currentUser = AuthStorage.CurrentUser;
            query = query.Where(o => o.ChefId == currentUser!.Id
                                     || (o.ChefId == null && o.Shift.Employees.Any(e => e.Id == currentUser.Id))
            );
            
            if (!string.IsNullOrWhiteSpace(OrderCodeFilter))
            {
                query = query.Where(o => o.OrderCode.Contains(OrderCodeFilter));
            }

            query = query.Where(o => o.CreatedAt >= MinCreatedAtFilter.UtcDateTime);
            query = query.Where(o => o.CreatedAt <= MaxCreatedAtFilter.UtcDateTime);

            if (MinAmountClientsFilter is not null)
            {
                query = query.Where(o => o.AmountClients >= MinAmountClientsFilter);
            }

            if (MaxAmountClientsFilter is not null)
            {
                query = query.Where(o => o.AmountClients <= MaxAmountClientsFilter);
            }

            if (SelectedStatusCookingOrderFilter?.Cooked is not null)
            {
                query = query.Where(o => o.CookingStatus == SelectedStatusCookingOrderFilter.Cooked);
            }

            if (SelectedAcceptedChefStatusFilter?.Accepted is true)
            {
                query = query.Where(o => o.ChefId == currentUser!.Id);
            }
            else if (SelectedAcceptedChefStatusFilter?.Accepted is false)
            {
                query = query.Where(o => o.ChefId == null);
            }

            ErrorMessage = string.Empty;
            var orders = query.ToList();

            Orders = new ObservableCollection<Order>(orders);
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка загрузки заказов";
            Orders = new();
        }
    }

    public bool HasNoOrders => Orders.Count == 0;
}
