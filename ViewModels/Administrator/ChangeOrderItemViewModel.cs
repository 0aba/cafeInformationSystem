using CommunityToolkit.Mvvm.Input;
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

public partial class ChangeOrderItemViewModel : ViewModelBase
{
    private OrderItem _orderItem;

    public ChangeOrderItemViewModel(string nameOrderItem)
    {
        var context = DatabaseService.GetContext();

        var changeOrderItem = context.OrderItem.FirstOrDefault(oi => oi.Name == nameOrderItem);

        if (changeOrderItem is null)
        {
            throw new Exception("Order item card does not exist");
        }

        _orderItem = changeOrderItem;

        CreatedAt = _orderItem.CreatedAt;
        Name = _orderItem.Name;
        Cost = _orderItem.Cost;

        BackToOrderItemsCommand = new RelayCommand(ExecuteBackToOrderItems);
        ChangeOrderItemCommand = new RelayCommand(ExecuteChangeOrderItem);
    }

    private DateTime _createdAt;

    private string _name = string.Empty;

    private decimal _cost;
    private string _errorMessage = string.Empty;

    public DateTime CreatedAt
    {
        get => _createdAt;
        private set => SetProperty(ref _createdAt, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public decimal Cost
    {
        get => _cost;
        set => SetProperty(ref _cost, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackToOrderItemsCommand { get; }
    public ICommand ChangeOrderItemCommand { get; }

    private void ExecuteBackToOrderItems()
    {
        Window window = new OrderItemsWindow()
        {
            DataContext = new OrderItemsViewModel()
        };


        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void ExecuteChangeOrderItem()
    {
        if (!ValidateInput())
        {
            return;
        }

        var context = DatabaseService.GetContext();

        _orderItem.Name = Name;
        _orderItem.Cost = Cost;

        context.OrderItem.Update(_orderItem);

        try
        {
            context.SaveChanges();
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка сохранения товара";
            return;
        }

        ExecuteBackToOrderItems();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Length > 256)
        {
            ErrorMessage = "Обязательное поле название товара длинной не более 256 символов";
            return false;
        }
        
        var context = DatabaseService.GetContext();

        var orderItem = context.OrderItem.AsNoTracking().FirstOrDefault(oi => oi.Name == Name);

        if (Name != _orderItem.Name && orderItem is not null)
        {
            ErrorMessage = "Название товара не уникально";
            return false;
        }

        if (Cost < 0)
        {
            ErrorMessage = "У товара обязательно должна быть цена";
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }
}
