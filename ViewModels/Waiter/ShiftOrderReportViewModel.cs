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

public class ShiftOrderReportItem
{
    public string OrderCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal? TotalCost { get; set; }
    public int AmountClients { get; set; }
    public string TableCode { get; set; } = string.Empty;
    public string? ChefLogin { get; set; }
    public OrderStatusFilterItem Status { get; set; } = AvailableStatusOrder[0];
    public bool CookingStatus { get; set; }

    static public List<OrderStatusFilterItem> AvailableStatusOrder { get; } = new()
    {
        new OrderStatusFilterItem { Name = "Принят", Status = OrderStatus.Accepted },
        new OrderStatusFilterItem { Name = "Оплачен", Status = OrderStatus.Paid },
        new OrderStatusFilterItem { Name = "Отменен", Status = OrderStatus.Cancelled },
    };
}

public partial class ShiftOrderReportViewModel : ViewModelBase
{
    public ShiftOrderReportViewModel()
    {
        BackToWaiterMenuCommand = new RelayCommand(ExecuteBackToWaiterMenu);
        GetShiftOrderReportCommand = new RelayCommand(ExecuteGetShiftOrderReport);
    }

    private string _shiftCode = string.Empty;

    public ObservableCollection<ShiftOrderReportItem> _shiftOrderReportItemTable = new();
    private string _errorMessage = string.Empty;

    public string ShiftCode
    {
        get => _shiftCode;
        set => SetProperty(ref _shiftCode, value);
    }

    public ObservableCollection<ShiftOrderReportItem> ShiftOrderReportItemTable
    {
        get => _shiftOrderReportItemTable;
        private set => SetProperty(ref _shiftOrderReportItemTable, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackToWaiterMenuCommand { get; }
    public ICommand GetShiftOrderReportCommand { get; }

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

    private void ExecuteGetShiftOrderReport()
    {
        if (!ValidateInput())
        {
            return;
        }

        try
        {
            var context = DatabaseService.GetContext();
            var currentUser = AuthStorage.CurrentUser;
            var shift = context.Shift.AsNoTracking().FirstOrDefault(e => e.ShiftCode == ShiftCode);

            var ordersWaiterShift = context.Order.Include(o => o.Waiter).Include(o => o.Chef).Include(o => o.Table)
                                                 .AsNoTracking()
                                                 .Where(o => o.ShiftId == shift!.Id && o.WaiterId == currentUser!.Id)
                                                 .ToList();

            var reportItems = ordersWaiterShift.Select(order => new ShiftOrderReportItem
            {
                OrderCode = order.OrderCode,
                CreatedAt = order.CreatedAt,
                ClosedAt = order?.ClosedAt,
                TotalCost = order!.TotalCost,
                AmountClients = order.AmountClients,
                TableCode = order.Table.TableCode,
                ChefLogin = order.Chef?.Username,
                Status = ShiftOrderReportItem.AvailableStatusOrder[(int) order.Status - 1],
                CookingStatus = order.CookingStatus
            }).ToList();

            ShiftOrderReportItemTable = new ObservableCollection<ShiftOrderReportItem>(reportItems);
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка при получении отчета";
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(ShiftCode))
        {
            ErrorMessage = "Код смены обязателен для получения отчета по смене";
            return false;
        }

        var context = DatabaseService.GetContext();
        var shift = context.Shift.Include(s => s.Employees).AsNoTracking().FirstOrDefault(e => e.ShiftCode == ShiftCode);

        if (shift is null)
        {
            ErrorMessage = "Смена с таким кодом не существует";
            return false;
        }

        var currentUser = AuthStorage.CurrentUser;
        bool isEmployeeInShift = shift.Employees.Any(e => e.Id == currentUser!.Id);
        if (!isEmployeeInShift)
        {
            ErrorMessage = "Вы не работали в эту смену";
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }
}
