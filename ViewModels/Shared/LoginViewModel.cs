using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using cafeInformationSystem.Models.DataBase.DataAccess;
using cafeInformationSystem.Models.Entities;
using cafeInformationSystem.Models.Cryptography;
using cafeInformationSystem.Views.Administrator;
using cafeInformationSystem.ViewModels.Administrator;
using cafeInformationSystem.Views.Chef;
using cafeInformationSystem.ViewModels.Chef;
using cafeInformationSystem.Views.Waiter;
using cafeInformationSystem.ViewModels.Waiter;
using System;
using cafeInformationSystem.Models.AuthService;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using cafeInformationSystem.Views.Shared;

namespace cafeInformationSystem.ViewModels.Shared;

public class LoginViewModel : ViewModelBase
{
    public LoginViewModel()
    {
        LoginCommand = new RelayCommand(ExecuteLogin);
    }

    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;

    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand LoginCommand { get; }

    private void ExecuteLogin()
    {
        if (string.IsNullOrWhiteSpace(Login))
        {
            ErrorMessage = "Поле 'Логин' обязательно к заполнению";
            return;
        }
        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Поле 'Пароль' обязательно к заполнению";
            return;
        }

        Employee? loginEmployee;

        try
        {
            loginEmployee = UtilsDataAccess.GetEmployee(Login);
        }
        catch (Exception)
        {
            ErrorMessage = "Пользователя не найден";
            return;
        }

        if (!loginEmployee.WorkStatus)
        {
            ErrorMessage = "Пользователя был уволен";
            return;
        }

        var isPasswordValid = PasswordHashing.VerifyPassword(Password, loginEmployee.Password);
        if (!isPasswordValid)
        {
            ErrorMessage = "Пароль не совпадает";
            return;
        }

        AuthStorage.LogInByUser(loginEmployee);

        Window window = new LoginWindow();

        switch (loginEmployee.Role)
        {
            case EmployeeRole.Administrator:
                window = new AdministratorMenuWindow()
                {
                    DataContext = new AdministratorMenuViewModel()
                };
                break;
            case EmployeeRole.Chef:
                window = new ChefMenuWindow()
                {
                    DataContext = new ChefMenuViewModel()
                };
                break;
            case EmployeeRole.Waiter:
                window = new WaiterMenuWindow()
                {
                    DataContext = new WaiterMenuViewModel()
                };
                break;
        }

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }
}
