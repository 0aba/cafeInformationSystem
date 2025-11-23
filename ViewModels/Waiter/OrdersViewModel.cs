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
using cafeInformationSystem.Views.Waiter;
using cafeInformationSystem.Models.AuthService;

namespace cafeInformationSystem.ViewModels.Waiter;

public partial class OrdersViewModel : ViewModelBase
{
    public OrdersViewModel()
    {
        SelectedStatusOrderFilter = AvailableStatusOrder[0];
        SelectedStatusCookingOrderFilter = AvailableStatusCookingOrder[0];

        BackToWaiterMenuCommand = new RelayCommand(ExecuteBackToWaiterMenu);
        NewOrderCommand = new RelayCommand(ExecuteNewOrder);
        ApplyFiltersCommand = new RelayCommand(ExecuteApplyFilters);
        OpenOrderCardCommand = new RelayCommand<string?>(ExecuteOpenOrderCard);
    }

    private string _orderCodeFilter = string.Empty;
    private DateTimeOffset _minCreatedAtFilter = DateTimeOffset.Now.AddMonths(-1);
    private DateTimeOffset _maxCreatedAtFilter = DateTimeOffset.Now.AddMonths(1);
    private string _tableCodeFilter = string.Empty;
    private string _chefLoginFilter = string.Empty;
    private string _shiftCodeFilter = string.Empty;

    public List<OrderStatusFilterItem> AvailableStatusOrder { get; } = new()
    {
        new OrderStatusFilterItem { Name = "Все", Status = null },
        new OrderStatusFilterItem { Name = "Принят", Status = OrderStatus.Accepted },
        new OrderStatusFilterItem { Name = "Оплачен", Status = OrderStatus.Paid },
        new OrderStatusFilterItem { Name = "Отменен", Status = OrderStatus.Cancelled },
    };
    private OrderStatusFilterItem? _selectedStatusOrderFilter;
    public List<OrderCookingStatusFilterItem> AvailableStatusCookingOrder { get; } = new()
    {
        new OrderCookingStatusFilterItem { Name = "Все", Cooked = null },
        new OrderCookingStatusFilterItem { Name = "Готов", Cooked = true },
        new OrderCookingStatusFilterItem { Name = "Готовится", Cooked = false }
    };
    private OrderCookingStatusFilterItem? _selectedStatusCookingOrderFilter;

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

    public string TableCodeFilter
    {
        get => _tableCodeFilter;
        set => SetProperty(ref _tableCodeFilter, value);
    }

    public string ChefLoginFilter
    {
        get => _chefLoginFilter;
        set => SetProperty(ref _chefLoginFilter, value);
    }

    public string ShiftCodeFilter
    {
        get => _shiftCodeFilter;
        set => SetProperty(ref _shiftCodeFilter, value);
    }

    public OrderStatusFilterItem? SelectedStatusOrderFilter
    {
        get => _selectedStatusOrderFilter;
        set => SetProperty(ref _selectedStatusOrderFilter, value);
    }

        public OrderCookingStatusFilterItem? SelectedStatusCookingOrderFilter
    {
        get => _selectedStatusCookingOrderFilter;
        set => SetProperty(ref _selectedStatusCookingOrderFilter, value);
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

    public ICommand BackToWaiterMenuCommand { get; }
    public ICommand NewOrderCommand { get; }
    public ICommand ApplyFiltersCommand { get; }
    public ICommand OpenOrderCardCommand { get; }

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
        LoadOrders();
    }

    private void ExecuteNewOrder()
    {
        Window window = new NewOrderWindow()
        {
            DataContext = new NewOrderViewModel()
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
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

            var query = context.Order.Include(o => o.Waiter).Include(o => o.Chef).Include(o => o.Table)
                                     .AsNoTracking().AsQueryable();
            
            var currentUser = AuthStorage.CurrentUser;
            query = query.Where(o => o.WaiterId == currentUser!.Id);

            if (!string.IsNullOrWhiteSpace(OrderCodeFilter))
            {
                query = query.Where(o => o.OrderCode.Contains(OrderCodeFilter));
            }

            query = query.Where(o => o.CreatedAt >= MinCreatedAtFilter.UtcDateTime);
            query = query.Where(o => o.CreatedAt <= MaxCreatedAtFilter.UtcDateTime);

            var currentTime = DateTime.SpecifyKind(DateTimeOffset.Now.DateTime, DateTimeKind.Utc);

            if (!string.IsNullOrWhiteSpace(TableCodeFilter))
            {
                var table = context.Table.AsNoTracking().FirstOrDefault(t => t.TableCode == TableCodeFilter);

                if (table is null)
                {
                    ErrorMessage = "Столик с таким кодом не существует";
                    return;
                }

                query = query.Where(o => o.TableId == table.Id);
            }

            if (!string.IsNullOrWhiteSpace(ChefLoginFilter))
            {
                var chef = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == ChefLoginFilter);

                if (chef is null)
                {
                    ErrorMessage = "Повар с таким логином не существует";
                    return;
                }

                query = query.Where(o => o.ChefId == chef.Id);
            }

            if (!string.IsNullOrWhiteSpace(ShiftCodeFilter))
            {
                var shift = context.Shift.AsNoTracking().FirstOrDefault(e => e.ShiftCode == ShiftCodeFilter);

                if (shift is null)
                {
                    ErrorMessage = "Смена с таким кодом не существует";
                    return;
                }

                query = query.Where(o => o.ShiftId == shift.Id);
            }

            if (SelectedStatusOrderFilter?.Status is not null)
            {
                query = query.Where(o => o.Status == SelectedStatusOrderFilter!.Status);
            }

            if (SelectedStatusCookingOrderFilter?.Cooked is not null)
            {
                query = query.Where(o => o.CookingStatus == SelectedStatusCookingOrderFilter.Cooked);
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
