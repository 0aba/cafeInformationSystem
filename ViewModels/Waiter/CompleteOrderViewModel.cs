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
using cafeInformationSystem.Views.Waiter;

namespace cafeInformationSystem.ViewModels.Waiter;

public partial class CompleteOrderViewModel : ViewModelBase
{
    private Order _changeOrder;

    public CompleteOrderViewModel(string orderCode)
    {
        var context = DatabaseService.GetContext();

        var changeOrder = context.Order.FirstOrDefault(s => s.OrderCode == orderCode);

        if (changeOrder is null)
        {
            throw new Exception("Order card does not exist");
        }

        _changeOrder = changeOrder;
        OrderCode = _changeOrder.OrderCode;

        CostToPay = context.OrderOrderItem.Where(ooi => ooi.OrderId == changeOrder.Id)
                                          .Include(ooi => ooi.СertainOrderItem)
                                          .AsEnumerable().Sum(ooi => ooi.СertainOrderItem.Cost * ooi.AmountItems);

        BackToOrderCommand = new RelayCommand(ExecuteBackToOrder);
        CompleteOrderCommand = new RelayCommand(ExecuteCompleteOrder);
        AddCashReceiptOrderItemCommand = new RelayCommand(ExecuteAddCashReceiptOrderItem);
        RemoveSelectedCashReceiptOrderItemCommand = new RelayCommand(ExecuteRemoveSelectedCashReceiptOrderItem, CanExecuteRemoveSelectedCashReceiptOrderItem);
    }

    private string _orderCode = string.Empty;
    private decimal _costToPay;

    public ObservableCollection<CashReceiptOrderItem> _cashReceiptOrderItemTable = new();
    private CashReceiptOrderItem? _selectedCashReceiptOrderItem;
    private string _errorMessage = string.Empty;

    public string OrderCode
    {
        get => _orderCode;
        private set => SetProperty(ref _orderCode, value);
    }

    public decimal CostToPay
    {
        get => _costToPay;
        private set => SetProperty(ref _costToPay, value);
    }

    public ObservableCollection<CashReceiptOrderItem> CashReceiptOrderItemTable
    {
        get => _cashReceiptOrderItemTable;
        set => SetProperty(ref _cashReceiptOrderItemTable, value);
    }

    public CashReceiptOrderItem? SelectedCashReceiptOrderItem
    {
        get => _selectedCashReceiptOrderItem;
        set
        {
            SetProperty(ref _selectedCashReceiptOrderItem, value);
            // INFO! вызываем проверку на активность кнопки
            (RemoveSelectedCashReceiptOrderItemCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }
    }
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackToOrderCommand { get; }
    public ICommand CompleteOrderCommand { get; }
    public ICommand AddCashReceiptOrderItemCommand { get; }
    public ICommand RemoveSelectedCashReceiptOrderItemCommand { get; }

    private void ExecuteAddCashReceiptOrderItem()
    {
        var newCashReceiptOrderItem = new CashReceiptOrderItem();
        CashReceiptOrderItemTable.Add(newCashReceiptOrderItem);
        // INFO! выделяем новую строку
        SelectedCashReceiptOrderItem = newCashReceiptOrderItem;
    }

    private void ExecuteRemoveSelectedCashReceiptOrderItem()
    {
        if (SelectedCashReceiptOrderItem is not null)
        {
            CashReceiptOrderItemTable.Remove(SelectedCashReceiptOrderItem);
            SelectedCashReceiptOrderItem = null;
        }
    }

    private bool CanExecuteRemoveSelectedCashReceiptOrderItem()
    {
        return SelectedCashReceiptOrderItem is not null;
    }

    private void ExecuteBackToOrder()
    {
        Window window = new ChangeOrderWindow()
        {
            DataContext = new ChangeOrderViewModel(_changeOrder.OrderCode)
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void ExecuteCompleteOrder()
    {
        if (!ValidateInput())
        {
            return;
        }

        var context = DatabaseService.GetContext();

        _changeOrder.Status = OrderStatus.Paid;
        _changeOrder.TotalCost = CostToPay;

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

        foreach (var cashReceiptOrderItem in CashReceiptOrderItemTable)
        {
            var newCashReceiptOrderItem = new CashReceiptOrder
            {
                PayedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                PaymentAmount = cashReceiptOrderItem.PaymentAmount,
                OrderId = _changeOrder.Id,
                TypePay = cashReceiptOrderItem.TypePay 
            };
            
            context.CashReceiptOrder.Add(newCashReceiptOrderItem);
        }

        try
        {
            context.SaveChanges();
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка кассового ордена";
            return;
        }

        ExecuteBackToOrder();
    }

    private bool ValidateInput()
    {
        decimal totalAmountPay = CashReceiptOrderItemTable.Sum(c => c.PaymentAmount);

        if (totalAmountPay < CostToPay)
        {
            ErrorMessage = $"Недостаточно {CostToPay-totalAmountPay:N2} рублей";
            return false;
        }

        if (totalAmountPay > CostToPay)
        {
            ErrorMessage = $"Сумма больше необходимой на {totalAmountPay-CostToPay:N2} рублей";
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }
}
