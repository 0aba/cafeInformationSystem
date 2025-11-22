using CommunityToolkit.Mvvm.Input;
using cafeInformationSystem.Models.Entities;
using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using cafeInformationSystem.Models.DataBase;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.ObjectModel;
using cafeInformationSystem.ViewModels.Shared;
using cafeInformationSystem.Models.AuthService;
using cafeInformationSystem.Views.Waiter;

namespace cafeInformationSystem.ViewModels.Waiter;

public partial class NewOrderViewModel : ViewModelBase
{
    public NewOrderViewModel()
    {
        BackToOrdersCommand = new RelayCommand(ExecuteBackToOrders);
        CreateOrderCommand = new RelayCommand(ExecuteCreateOrder);
        AddOrderItemCommand = new RelayCommand(ExecuteAddOrderItem);
        RemoveSelectedOrderItemCommand = new RelayCommand(ExecuteRemoveSelectedOrderItem, CanExecuteRemoveSelectedOrderItem);
    }

    private string _orderCode = string.Empty;
    private int _amountClients = 1;
    private string _tableCode = string.Empty;
    private string? _note = string.Empty;

    public ObservableCollection<OrderItemItem> _orderItemItemTable = new();

    private OrderItemItem? _selectedOrderItem;
    private string _errorMessage = string.Empty;

    public string OrderCode
    {
        get => _orderCode;
        set => SetProperty(ref _orderCode, value);
    }

    public int? AmountClients
    {
        get => _amountClients;
        set
        {
            try
            {
                _amountClients = value ?? 1;
                
                OnPropertyChanged();
            }
            catch
            {
                _amountClients = 1;
                OnPropertyChanged();
            }
        }
    }

    public string TableCode
    {
        get => _tableCode;
        set => SetProperty(ref _tableCode, value);
    }

    public string? Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public ObservableCollection<OrderItemItem> OrderItemItemTable
    {
        get => _orderItemItemTable;
        set => SetProperty(ref _orderItemItemTable, value);
    }

    public OrderItemItem? SelectedOrderItem
    {
        get => _selectedOrderItem;
        set
        {
            SetProperty(ref _selectedOrderItem, value);
            // INFO! вызываем проверку на активность кнопки
            (RemoveSelectedOrderItemCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }
    }
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackToOrdersCommand { get; }
    public ICommand CreateOrderCommand { get; }
    public ICommand AddOrderItemCommand { get; }
    public ICommand RemoveSelectedOrderItemCommand { get; }

    private void ExecuteAddOrderItem()
    {
        var newOrderItem = new OrderItemItem();
        OrderItemItemTable.Add(newOrderItem);
        // INFO! выделяем новую строку
        SelectedOrderItem = newOrderItem;
    }

    private void ExecuteRemoveSelectedOrderItem()
    {
        if (SelectedOrderItem is not null)
        {
            OrderItemItemTable.Remove(SelectedOrderItem);
            SelectedOrderItem = null;
        }
    }

    private bool CanExecuteRemoveSelectedOrderItem()
    {
        return SelectedOrderItem is not null;
    }

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

    private void ExecuteCreateOrder()
    {
        if (!ValidateInput())
        {
            return;
        }

        var context = DatabaseService.GetContext();
        var currentUser = AuthStorage.CurrentUser;
        
        var activeShift = context.Shift.Include(s => s.Employees).AsNoTracking()
                                       .FirstOrDefault(s => s.TimeEnd > DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)
                                                            && s.TimeStart <= DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)
                                                            && s.Employees.Any(e => e.Id == currentUser!.Id));
        var table = context.Table.FirstOrDefault(t => t.TableCode == TableCode);

        var order = new Order
        {
            OrderCode = OrderCode.Trim(),
            CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
            AmountClients = AmountClients ?? 1,
            WaiterId = currentUser!.Id,
            TableId = table!.Id,
            ShiftId = activeShift!.Id,
            Status = OrderStatus.Accepted,
            CookingStatus = false,
            Note = Note?.Trim()
        };

        context.Order.Add(order);

        try
        {
            context.SaveChanges();
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка сохранения заказа";
            return;
        }

        foreach (var orderItem in OrderItemItemTable)
        {
            var existingOrderItem = context.OrderItem.FirstOrDefault(oi => oi.Name.Trim() == orderItem.Name.Trim());

            var orderOrderItem = new OrderOrderItem
            {
                OrderId = order!.Id,
                OrderItemId = existingOrderItem!.Id,
                AmountItems = orderItem.AmountItems
            };
            
            context.OrderOrderItem.Add(orderOrderItem);
        }

        try
        {
            context.SaveChanges();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ErrorMessage = "Ошибка сохранения товаров заказа";
            return;
        }

        ExecuteBackToOrders();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(OrderCode) || OrderCode.Length > 256)
        {
            ErrorMessage = "Обязательное поле код заказа длинной не более 256 символов";
            return false;
        }

        var context = DatabaseService.GetContext();

        var order = context.Order.AsNoTracking().FirstOrDefault(s => s.OrderCode == OrderCode.Trim());

        if (order is not null)
        {
            ErrorMessage = "Код заказа не уникальный";
            return false;
        }

        OrderItemItemTable = new ObservableCollection<OrderItemItem>(
            OrderItemItemTable
            .GroupBy(e => e.Name.Trim())
            .Select(g => g.First())
            .ToList()
        );

        if (AmountClients <= 0)
        {
            ErrorMessage = "Обязательное поле количество клиентов минимум один клиент";
            return false;
        }

        var currentUser = AuthStorage.CurrentUser;

        var table = context.Table.AsNoTracking().FirstOrDefault(s => s.TableCode == TableCode && s.WaiterServiceId == currentUser!.Id);

        if (table is null)
        {
            ErrorMessage = "Столика с таким кодом не существует, либо за вами он не закреплен";
            return false;
        }

        if (OrderItemItemTable.Count == 0)
        {
            ErrorMessage = "Добавьте хотя бы одну позицию в заказ";
            return false;
        }

        foreach (var orderItem in OrderItemItemTable)
        {
            if (string.IsNullOrWhiteSpace(orderItem.Name) || orderItem.Name.Length > 256)
            {
                ErrorMessage = "Название позиции не может быть пустым или длинной не более 256 символов";
                return false;
            }

            var checkOrderItem = context.OrderItem.AsNoTracking().FirstOrDefault(oi => oi.Name.Trim() == orderItem.Name.Trim());

            if (checkOrderItem is null)
            {
                ErrorMessage = $"Позиция '{orderItem.Name}' не найдена в меню";
                return false;
            }

            if (orderItem.AmountItems <= 0 && orderItem.AmountItems > short.MaxValue)
            {
                ErrorMessage = $"Количество для позиции '{orderItem.Name}' должно быть больше 0 и меньше 32767";
                return false;
            }
        }
        
        var activeShift = context.Shift.Include(s => s.Employees).AsNoTracking()
                                       .FirstOrDefault(s => s.TimeEnd > DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)
                                                            && s.TimeStart <= DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)
                                                            && s.Employees.Any(e => e.Id == currentUser!.Id));

        if (activeShift is null)
        {
            ErrorMessage = "У вас нет активной смены обратитесь администратору";
            return false;
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
