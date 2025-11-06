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
using Avalonia.Media.Imaging;
using cafeInformationSystem.Models.DataBase.DataAccess;
using cafeInformationSystem.Models.Cryptography;
using cafeInformationSystem.Models.MediaService;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace cafeInformationSystem.ViewModels.Administrator;

public partial class NewOrderItemViewModel : ViewModelBase
{
    public NewOrderItemViewModel()
    {
        BackToOrderItemsCommand = new RelayCommand(ExecuteBackToOrderItems);
        CreateOrderItemCommand = new RelayCommand(ExecuteCreateOrderItem);
    }

    private string _name = string.Empty;
    private decimal _cost = 1;

    private string _errorMessage = string.Empty;

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
    public ICommand CreateOrderItemCommand { get; }

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

    private void ExecuteCreateOrderItem()
    {
        if (!ValidateInput())
        {
            return;
        }

        var context = DatabaseService.GetContext();

        var orderItem = new OrderItem
        {
            Name = Name,
            Cost = Cost
        };

        context.OrderItem.Add(orderItem);

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

        if (orderItem is not null)
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
