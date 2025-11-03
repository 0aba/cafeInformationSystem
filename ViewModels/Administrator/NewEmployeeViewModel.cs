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

namespace cafeInformationSystem.ViewModels.Administrator;

public partial class NewEmployeeViewModel : ViewModelBase
{
    public NewEmployeeViewModel()
    {
        SelectedRoleFilter = AvailableRoles[0];
        BackToEmployeesCommand = new RelayCommand(ExecuteBackToEmployees);
        CreateEmployeeCommand = new RelayCommand(ExecuteCreateEmployee);
    }

    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _middleName = string.Empty;
    private string? _pathPhoto = null;
    private string? _pathScanEmploymentContract = null;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;

    private RoleFilterItem? _selectedRoleFilter;

    public List<RoleFilterItem> AvailableRoles { get; } = new()
    {
        new RoleFilterItem { Name = "Повар", Role = EmployeeRole.Chef },
        new RoleFilterItem { Name = "Официант", Role = EmployeeRole.Waiter },
    };

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public string MiddleName
    {
        get => _middleName;
        set => SetProperty(ref _middleName, value);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public RoleFilterItem? SelectedRoleFilter
    {
        get => _selectedRoleFilter;
        set => SetProperty(ref _selectedRoleFilter, value);
    }

    public ICommand BackToEmployeesCommand { get; }
    public ICommand CreateEmployeeCommand { get; }

    private void ExecuteBackToEmployees()
    {
        Window window = new EmployeesWindow()
        {
            DataContext = new EmployeesViewModel()
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    public void LoadPhoto(string filePath, string fileName)
    {
        var imagesMediaService = ImagesMediaService.GetMediaService();

        var dirSaveImage = imagesMediaService.GetPathDirectoryBasedOnCurrentDate();
        var newNameSaveImage = imagesMediaService.GeneratorNameLen64Image() + Path.GetExtension(fileName);

        imagesMediaService.SaveImage(dirSaveImage, newNameSaveImage, new Bitmap(filePath));
        var pathPhoto = Path.Combine(dirSaveImage, newNameSaveImage);
        _pathPhoto = pathPhoto;
    }
    
    public void LoadScanEmploymentContract(string filePath, string fileName)
    {
        var imagesMediaService = ImagesMediaService.GetMediaService();

        var dirSaveImage = imagesMediaService.GetPathDirectoryBasedOnCurrentDate();
        var newNameSaveImage = imagesMediaService.GeneratorNameLen64Image() + Path.GetExtension(fileName);

        imagesMediaService.SaveImage(dirSaveImage, newNameSaveImage, new Bitmap(filePath));
        var pathScanEmploymentContract = Path.Combine(dirSaveImage, newNameSaveImage);
        _pathScanEmploymentContract = pathScanEmploymentContract;
    }

    private void ExecuteCreateEmployee()
    {
        if (!ValidateInput())
        {
            return;
        }

        var employee = new Employee
        {
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            Photo = _pathPhoto,
            ScanEmploymentContract = _pathScanEmploymentContract,
            Username = Username,
            Password = Password,
            Role = SelectedRoleFilter!.Role ?? EmployeeRole.Waiter,
            WorkStatus = true
        };

        var context = DatabaseService.GetContext();

        employee.Password = PasswordHashing.HashPassword(employee.Password);

        context.Employee.Add(employee);

        try
        {
            context.SaveChanges();
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка сохранения пользователя";
            return;
        }

        ExecuteBackToEmployees();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || FirstName.Length > 128)
        {
            ErrorMessage = "Обязательное поле имя сотрудника длинной не более 128 символов";
            return false;
        }

        if (string.IsNullOrWhiteSpace(LastName) || LastName.Length > 128)
        {
            ErrorMessage = "Обязательное поле фамилию сотрудника длинной не более 128 символов";
            return false;
        }

        if (MiddleName.Length > 128)
        {
            ErrorMessage = "Отчество сотрудника длинной не более 128 символов";
            return false;
        }

        if (string.IsNullOrWhiteSpace(_pathPhoto))
        {
            ErrorMessage = "Необходимо указать фото";
            return false;
        }

        if (string.IsNullOrWhiteSpace(_pathScanEmploymentContract))
        {
            ErrorMessage = "Необходимо указать скан документа";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3 || Username.Length > 150)
        {
            ErrorMessage = "Обязательное поле имя пользователя должно быть от 3 до 150 символов";
            return false;
        }

        var userAlreadyExists = UtilsDataAccess.CheckExistsEmployee(Username);

        if (userAlreadyExists)
        {
            ErrorMessage = "Сотрудник с таким логином уже существует";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите пароль";
            return false;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают";
            return false;
        }

        if (Password.Length < 8)
        {
            ErrorMessage = "Пароль должен содержать минимум 8 символов";
            return false;
        }

        if (SelectedRoleFilter == null)
        {
            ErrorMessage = "Выберите роль сотрудника";
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }
}
