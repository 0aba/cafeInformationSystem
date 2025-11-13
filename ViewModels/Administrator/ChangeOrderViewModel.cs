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
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.ObjectModel;

namespace cafeInformationSystem.ViewModels.Administrator;

public class OrderItemItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public short AmountItems { get; set; }
}

public class CashReceiptOrderItem
{
    public decimal PaymentAmount { get; set; }
    public bool TypePay { get; set; }
}

public partial class ChangeOrderViewModel : ViewModelBase
{
    private Order _changeOrder;

    public ChangeOrderViewModel(string orderCode)
    {
        var context = DatabaseService.GetContext();

        var changeOrder = context.Order.Include(o => o.Waiter).Include(o => o.Chef).Include(o => o.Table)
                                       .Include(s => s.CashReceiptOrderItems)
                                       .FirstOrDefault(s => s.OrderCode == orderCode);

        if (changeOrder is null)
        {
            throw new Exception("Order card does not exist");
        }

        _changeOrder = changeOrder;

        OrderCode = _changeOrder.OrderCode;
        CreatedAt = _changeOrder.CreatedAt;
        ClosedAt = _changeOrder.ClosedAt;
        Cost = _changeOrder.TotalCost;
        AmountClients = _changeOrder.AmountClients;
        WaiterLogin = _changeOrder.Waiter.Username;
        TableCode = _changeOrder.Table.TableCode;
        ChefLogin = _changeOrder?.Chef?.Username;
        ChoiceStatusOrder = AvailableStatusOrder[(int)_changeOrder!.Status];
        StatusCookingOrder = _changeOrder.CookingStatus;
        Note = _changeOrder.Note;

        foreach (var cashReceiptOrder in _changeOrder.CashReceiptOrderItems)
        {
            _cashReceiptOrders.Add(new CashReceiptOrderItem 
            { 
                PaymentAmount = cashReceiptOrder.PaymentAmount,
                TypePay = cashReceiptOrder.TypePay
            });
        }
        
        var orderOrderItems = context.OrderOrderItem
        .Where(ooi => ooi.OrderId == changeOrder.Id)
        .Include(ooi => ooi.СertainOrderItem)
        .ToList();

        foreach (var orderOrderItem in orderOrderItems)
        {
            _orderItems.Add(new OrderItemItem
            {
                Name = orderOrderItem.СertainOrderItem.Name,
                Cost = orderOrderItem.СertainOrderItem.Cost,
                AmountItems = orderOrderItem.AmountItems
            });
        }


        BackToOrdersCommand = new RelayCommand(ExecuteBackToOrders);
        ChangeOrderCommand = new RelayCommand(ExecuteChangeOrder);
    }

    private string _orderCode = string.Empty;
    private DateTimeOffset _createdAt = new();
    private DateTimeOffset? _closedAt = new();
    public decimal? _cost = null;
    public int _amountClients;
    private string _waiterLogin = string.Empty;
    private string _tableCode = string.Empty;
    private string? _chefLogin = string.Empty;

    public List<OrderStatusFilterItem> AvailableStatusOrder { get; } = new()
    {
        new OrderStatusFilterItem { Name = "Принят", Status = OrderStatus.Accepted },
        new OrderStatusFilterItem { Name = "Оплачен", Status = OrderStatus.Paid },
        new OrderStatusFilterItem { Name = "Отменен", Status = OrderStatus.Cancelled },
    };
    private OrderStatusFilterItem? _choiceStatusOrder;
    private bool _statusCookingOrder;

    private ObservableCollection<CashReceiptOrderItem> _cashReceiptOrders = new();
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

    public DateTimeOffset? ClosedAt
    {
        get => _closedAt;
        set => SetProperty(ref _closedAt, value);
    }

    public decimal? Cost
    {
        get => _cost;
        set => SetProperty(ref _cost, value);
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

    public string TableCode
    {
        get => _tableCode;
        set => SetProperty(ref _tableCode, value);
    }

    public string? ChefLogin
    {
        get => _chefLogin;
        set => SetProperty(ref _chefLogin, value);
    }

    public OrderStatusFilterItem? ChoiceStatusOrder
    {
        get => _choiceStatusOrder;
        set => SetProperty(ref _choiceStatusOrder, value);
    }

    public bool StatusCookingOrder
    {
        get => _statusCookingOrder;
        set => SetProperty(ref _statusCookingOrder, value);
    }

    public ObservableCollection<CashReceiptOrderItem> СashReceiptOrdersTable
    {
        get => _cashReceiptOrders;
        set => SetProperty(ref _cashReceiptOrders, value);
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
    public ICommand ChangeOrderCommand { get; }

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

    private void ExecuteChangeOrder()
    {
        if (!ValidateInput())
        {
            return;
        }

        var context = DatabaseService.GetContext();

        if (WaiterLogin != _changeOrder.Waiter.Username)
        {
            var waiter = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == WaiterLogin);

            _changeOrder.WaiterId = waiter!.Id;
        }

        if (TableCode != _changeOrder.Table.TableCode)
        {
            var table = context.Table.AsNoTracking().FirstOrDefault(t => t.TableCode == TableCode);

            _changeOrder.TableId = table!.Id;
        }

        if (!string.IsNullOrEmpty(ChefLogin) && ChefLogin != _changeOrder?.Chef?.Username)
        {
            var chef = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == ChefLogin);

            _changeOrder!.ChefId = chef!.Id;
        } 
        else
        {
            _changeOrder.ChefId = null;
        }

        _changeOrder.Note = Note;
        

        try
        {
            context.SaveChanges();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ErrorMessage = "Ошибка сохранения заказа";
            return;
        }

        ExecuteBackToOrders();
    }

    private bool ValidateInput()
    {
        var context = DatabaseService.GetContext();

        if (WaiterLogin != _changeOrder.Waiter.Username)
        {
            var waiter = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == WaiterLogin);

            if (waiter is null)
            {
                ErrorMessage = "Официанта с таким логином не существует";
                return false;
            }
        }

        if (TableCode != _changeOrder.Table.TableCode)
        {
            var table = context.Table.AsNoTracking().FirstOrDefault(t => t.TableCode == TableCode);

            if (table is null)
            {
                ErrorMessage = "Столик с таким кодом не существует";
                return false;
            }
        }

        if (!string.IsNullOrEmpty(ChefLogin) && ChefLogin != _changeOrder?.Chef?.Username)
        {
            var chef = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == ChefLogin);

            if (chef is null)
            {
                ErrorMessage = "Повар с таким логином не существует";
                return false;
            }
        }

        if (!string.IsNullOrEmpty(Note) && Note.Length > 512)
        {
                ErrorMessage = "Заметка к заказу должна быть не более 512 символов";
                return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }
}
