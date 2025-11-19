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
using System.Collections.Generic;

namespace cafeInformationSystem.ViewModels.Waiter;

public partial class ChangeOrderViewModel : ViewModelBase
{
    private Order _changeOrder;

    public ChangeOrderViewModel(string orderCode)
    {
        var context = DatabaseService.GetContext();

        var changeOrder = context.Order.Include(s => s.Chef).Include(s => s.Table).FirstOrDefault(s => s.OrderCode == orderCode);

        if (changeOrder is null)
        {
            throw new Exception("Order card does not exist");
        }

        _changeOrder = changeOrder;

        OrderCode = _changeOrder.OrderCode;
        AmountClients = _changeOrder.AmountClients;
        TableCode = _changeOrder.Table.TableCode;
        ChefLogin = _changeOrder?.Chef?.Username;
        ChoiceStatusOrder = AvailableStatusOrder[(int)_changeOrder!.Status];
        StatusCookingOrder = _changeOrder.CookingStatus;
        Note = _changeOrder.Note;

        var orderOrderItems = context.OrderOrderItem.Where(ooi => ooi.OrderId == changeOrder.Id)
                                                    .Include(ooi => ooi.СertainOrderItem)
                                                    .ToList();

        foreach (var orderOrderItem in orderOrderItems)
        {
            _orderItemItemTable.Add(new OrderItemItem
            {
                Name = orderOrderItem.СertainOrderItem.Name,
                CostItem = orderOrderItem.СertainOrderItem.Cost,
                AmountItems = orderOrderItem.AmountItems
            });
        }

        BackToOrdersCommand = new RelayCommand(ExecuteBackToOrders);
        ChangeOrderCommand = new RelayCommand(ExecuteChangeOrder);
        CancelOrderCommand = new RelayCommand(ExecuteCancelOrder);
        CompleteOrderCommand = new RelayCommand(ExecuteCompleteOrder);
        GetCheckCommand = new RelayCommand(ExecuteGetCheck);
        AddOrderItemCommand = new RelayCommand(ExecuteAddOrderItem);
        RemoveSelectedOrderItemCommand = new RelayCommand(ExecuteRemoveSelectedOrderItem, CanExecuteRemoveSelectedOrderItem);
    }

    private string _orderCode = string.Empty;
    private int _amountClients = 1;
    private string? _chefLogin = string.Empty;

    public List<OrderStatusFilterItem> AvailableStatusOrder { get; } = new()
    {
        new OrderStatusFilterItem { Name = "Принят", Status = OrderStatus.Accepted },
        new OrderStatusFilterItem { Name = "Оплачен", Status = OrderStatus.Paid },
        new OrderStatusFilterItem { Name = "Отменен", Status = OrderStatus.Cancelled },
    };
    private OrderStatusFilterItem? _choiceStatusOrder;
    private bool _statusCookingOrder;
    
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

    public int AmountClients
    {
        get => _amountClients;
        set
        {
            try
            {
                _amountClients = value;
                
                OnPropertyChanged();
            }
            catch
            {
                _amountClients = 1;
                OnPropertyChanged();
            }
        }
    }

    public string? ChefLogin
    {
        get => _chefLogin;
        private set => SetProperty(ref _chefLogin, value);
    }

    public OrderStatusFilterItem? ChoiceStatusOrder
    {
        get => _choiceStatusOrder;
        private set => SetProperty(ref _choiceStatusOrder, value);
    }

    public bool StatusCookingOrder
    {
        get => _statusCookingOrder;
        private set => SetProperty(ref _statusCookingOrder, value);
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
    public ICommand ChangeOrderCommand { get; }
    public ICommand CancelOrderCommand { get; }
    public ICommand CompleteOrderCommand { get; }
    public ICommand GetCheckCommand { get; }
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

    private void ExecuteCancelOrder()
    {
        if (_changeOrder.Status == OrderStatus.Paid)
        {
            ErrorMessage = "Нельзя отменить уже оплаченный заказ";
            return;
        }
        if (_changeOrder.Status == OrderStatus.Cancelled)
        {
            ErrorMessage = "Нельзя отменить уже отмененный заказ";
            return;
        }

        var context = DatabaseService.GetContext();

        _changeOrder.Status = OrderStatus.Cancelled;

        context.Order.Update(_changeOrder);
        
        try
        {
            context.SaveChanges();
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка сохранения заказа";
            return;
        }
    }

    private void ExecuteCompleteOrder()
    {
        if (_changeOrder.Status == OrderStatus.Paid)
        {
            ErrorMessage = "Нельзя завершить уже оплаченный заказ";
            return;
        }
        if (_changeOrder.Status == OrderStatus.Cancelled)
        {
            ErrorMessage = "Нельзя завершить уже отмененный заказ";
            return;
        }
        if (_changeOrder.ChefId is null)
        {
            ErrorMessage = "Нельзя завершить заказ повар еще за него не взялся";
            return;
        }
        if (_changeOrder.CookingStatus == false)
        {
            ErrorMessage = "Нельзя завершить уже повар его еще не приготовил";
            return;
        }

        Window window = new CompleteOrderWindow()
        {
            DataContext = new CompleteOrderViewModel(_changeOrder.OrderCode)
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void ExecuteGetCheck()
    {
        if (_changeOrder.Status != OrderStatus.Paid)
        {
            ErrorMessage = "Только у оплаченного заказа есть чек";
            return;
        }
        
        //TODO!
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

    private void ExecuteChangeOrder()
    {
        if (!ValidateInput())
        {
            return;
        }

        var context = DatabaseService.GetContext();
        var currentUser = AuthStorage.CurrentUser;
        
        var table = context.Table.FirstOrDefault(t => t.TableCode == TableCode);

        _changeOrder.OrderCode = OrderCode.Trim();
        _changeOrder.AmountClients = AmountClients;
        _changeOrder.TableId = table!.Id;
        _changeOrder.Note = Note?.Trim();

        context.Order.Update(_changeOrder);
        
        try
        {
            context.SaveChanges();
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка сохранения заказа";
            return;
        }

        var existingOrderOrderItems = context.OrderOrderItem .Where(ooi => ooi.OrderId == _changeOrder.Id).ToList();
        
        context.OrderOrderItem.RemoveRange(existingOrderOrderItems);

        try
        {
            context.SaveChanges();
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка удаления старых позиций заказа";
            return;
        }

        foreach (var orderItem in OrderItemItemTable)
        {
            var existingOrderItem = context.OrderItem.FirstOrDefault(oi => oi.Name.Trim() == orderItem.Name.Trim());

            var orderOrderItem = new OrderOrderItem
            {
                OrderId = _changeOrder.Id,
                OrderItemId = existingOrderItem!.Id,
                AmountItems = orderItem.AmountItems
            };
            
            context.OrderOrderItem.Add(orderOrderItem);
        }

        try
        {
            context.SaveChanges();
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка сохранения новых позиций заказа";
            return;
        }

        ExecuteBackToOrders();
    }

    private bool ValidateInput()
    {
        if (_changeOrder.Status != OrderStatus.Accepted)
        {
            ErrorMessage = "Изменить можно только не завершенный заказ";
            return false;
        }

        if (string.IsNullOrWhiteSpace(OrderCode) || OrderCode.Length > 256)
        {
            ErrorMessage = "Обязательное поле код заказа длинной не более 256 символов";
            return false;
        }

        var context = DatabaseService.GetContext();

        if (OrderCode != _changeOrder.OrderCode)
        {
            var order = context.Order.AsNoTracking().FirstOrDefault(s => s.OrderCode == OrderCode.Trim());

            if (order is not null)
            {
                ErrorMessage = "Код заказа не уникальный";
                return false;
            }
        }

        if (AmountClients <= 0)
        {
            ErrorMessage = "Обязательное поле количество клиентов минимум один клиент";
            return false;
        }

        var currentUser = AuthStorage.CurrentUser;


        if (TableCode != _changeOrder.Table.TableCode)
        {
            var table = context.Table.AsNoTracking().FirstOrDefault(s => s.TableCode == TableCode && s.WaiterServiceId == currentUser!.Id);

            if (table is null)
            {
                ErrorMessage = "Столика с таким кодом не существует, либо за вами он не закреплен";
                return false;
            }
        }

        OrderItemItemTable = new ObservableCollection<OrderItemItem>(
            OrderItemItemTable
            .GroupBy(e => e.Name.Trim())
            .Select(g => g.First())
            .ToList()
        );

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

            if (orderItem.AmountItems <= 0)
            {
                ErrorMessage = $"Количество для позиции '{orderItem.Name}' должно быть больше 0";
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
