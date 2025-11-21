using CommunityToolkit.Mvvm.Input;
using cafeInformationSystem.Models.Entities;
using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using cafeInformationSystem.Views.Chef;
using cafeInformationSystem.Models.DataBase;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.ObjectModel;
using cafeInformationSystem.ViewModels.Shared;
using cafeInformationSystem.Models.AuthService;

namespace cafeInformationSystem.ViewModels.Chef;

public partial class ChangeOrderViewModel : ViewModelBase
{
    private Order _changeOrder;

    public ChangeOrderViewModel(string orderCode)
    {
        var context = DatabaseService.GetContext();

        var changeOrder = context.Order.Include(o => o.Waiter).Include(o => o.Chef).Include(o => o.Table).Include(o => o.Shift)
                                       .Include(o => o.CashReceiptOrderItems)
                                       .FirstOrDefault(o => o.OrderCode == orderCode);

        if (changeOrder is null)
        {
            throw new Exception("Order card does not exist");
        }

        _changeOrder = changeOrder;

        OrderCode = _changeOrder.OrderCode;
        CreatedAt = _changeOrder.CreatedAt;
        AmountClients = _changeOrder.AmountClients;
        WaiterLogin = _changeOrder.Waiter.Username;
        ChefLogin = _changeOrder?.Chef?.Username;
        StatusCookingOrder = _changeOrder!.CookingStatus;
        Note = _changeOrder.Note;

        var orderOrderItems = context.OrderOrderItem.Where(ooi => ooi.OrderId == changeOrder.Id)
                                                    .Include(ooi => ooi.СertainOrderItem)
                                                    .ToList();

        foreach (var orderOrderItem in orderOrderItems)
        {
            _orderItems.Add(new OrderItemItem
            {
                Name = orderOrderItem.СertainOrderItem.Name,
                // CostItem = orderOrderItem.СertainOrderItem.Cost, // INFO! Не отображать в axml так как повару он не нужен
                AmountItems = orderOrderItem.AmountItems
            });
        }

        BackToOrdersCommand = new RelayCommand(ExecuteBackToOrders);
        TakeOrderCommand = new RelayCommand(ExecuteTakeOrder);
        SetOrderStatusCookedCommand = new RelayCommand(ExecuteSetOrderStatusCooked);
    }

    private string _orderCode = string.Empty;
    private DateTimeOffset _createdAt = new();
    public int _amountClients;
    private string _waiterLogin = string.Empty;
    private string? _chefLogin = string.Empty;
    private bool _statusCookingOrder;

    private ObservableCollection<OrderItemItem> _orderItems = new();

    private string? _note = string.Empty;
    private string _errorMessage = string.Empty;

    public string OrderCode
    {
        get => _orderCode;
        set => SetProperty(ref _orderCode, value);
    }

    public DateTimeOffset CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
    }

    public int AmountClients
    {
        get => _amountClients;
        set => SetProperty(ref _amountClients, value);
    }

    public string WaiterLogin
    {
        get => _waiterLogin;
        set => SetProperty(ref _waiterLogin, value);
    }

    public string? ChefLogin
    {
        get => _chefLogin;
        set => SetProperty(ref _chefLogin, value);
    }

    public bool StatusCookingOrder
    {
        get => _statusCookingOrder;
        set => SetProperty(ref _statusCookingOrder, value);
    }

    public ObservableCollection<OrderItemItem> OrderTable
    {
        get => _orderItems;
        set => SetProperty(ref _orderItems, value);
    }

    public string? Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackToOrdersCommand { get; }
    public ICommand TakeOrderCommand { get; }
    public ICommand SetOrderStatusCookedCommand { get; }

    private void ExecuteBackToOrders()
    {
        Window window = new OrdersWindow()
        {
            DataContext = new OrdersViewModel()
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void ExecuteTakeOrder()
    {
        var context = DatabaseService.GetContext();
        var currentUser = AuthStorage.CurrentUser;

        var freshOrder = context.Order.Include(o => o.Chef).FirstOrDefault(o => o.OrderCode == _changeOrder.OrderCode);

        if (freshOrder?.ChefId is not null && freshOrder.ChefId != currentUser!.Id)
        {
            ErrorMessage = $"Заказ уже взят поваром '{freshOrder!.Chef!.Username}'";
            return;
        }

        if (freshOrder!.ChefId == currentUser!.Id)
        {
            ErrorMessage = $"Вы уже взяли заказ";
            return;
        }

        freshOrder!.ChefId = currentUser!.Id;

        context.Order.Update(freshOrder);
        try
        {
            context.SaveChanges();
            ReloadPage();
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка сохранения заказа";
            return;
        }
    }

    private void ExecuteSetOrderStatusCooked()
    {
        var context = DatabaseService.GetContext();
        var currentUser = AuthStorage.CurrentUser;

        if (_changeOrder.ChefId != currentUser!.Id)
        {
            ErrorMessage = "Заказ должен быть закреплен за вами";
            return;
        }

        if (_changeOrder.ChefId == currentUser!.Id && _changeOrder.CookingStatus)
        {
            ErrorMessage = "Вы уже приготовили заказ";
            return;
        }

        _changeOrder.CookingStatus = true;

        context.Order.Update(_changeOrder);
        try
        {
            context.SaveChanges();

            ReloadPage();
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка сохранения статуса заказа";
            return;
        }
    }

    private void ReloadPage()
    {
        Window window = new ChangeOrderWindow()
        {
            DataContext = new ChangeOrderViewModel(OrderCode)
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }
}
