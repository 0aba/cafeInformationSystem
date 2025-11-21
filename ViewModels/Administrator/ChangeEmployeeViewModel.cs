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
using System.Linq;
using Avalonia.Media.Imaging;
using System.IO;
using cafeInformationSystem.Models.Cryptography;
using Avalonia.Platform;
using cafeInformationSystem.Models.MediaService;

namespace cafeInformationSystem.ViewModels.Administrator;

public partial class ChangeEmployeeViewModel : ViewModelBase
{
    private const string _DEFAULT_PHOTO_USER_ASSETS_PATH = "avares://cafeInformationSystem/Assets/Default/default-employee-photo.png";
    private const string _DEFAULT_NO_CONTRACT_ASSETS_PATH = "avares://cafeInformationSystem/Assets/Default/default-no-contract.jpeg";

    private Employee _changeEmployee;

    public ChangeEmployeeViewModel(string username)
    {
        var context = DatabaseService.GetContext();

        var changeEmployee = context.Employee.FirstOrDefault(e => e.Username == username);

        if (changeEmployee is null)
        {
            throw new Exception("User card does not exist");
        }

        if (!string.IsNullOrWhiteSpace(changeEmployee.Photo))
        {
            PhotoImage = ImagesMediaService.GetMediaService().GetImage(changeEmployee.Photo);
        }
        else
        {
            using var streamPhoto = AssetLoader.Open(new Uri(_DEFAULT_PHOTO_USER_ASSETS_PATH));
            PhotoImage = new Bitmap(streamPhoto);
        }

        if (!string.IsNullOrWhiteSpace(changeEmployee.ScanEmploymentContract))
        {
            ContractImage = ImagesMediaService.GetMediaService().GetImage(changeEmployee.ScanEmploymentContract);
        }
        else
        {
            using var streamNoContract = AssetLoader.Open(new Uri(_DEFAULT_NO_CONTRACT_ASSETS_PATH));
            ContractImage = new Bitmap(streamNoContract);
        }

        _changeEmployee = changeEmployee;

        FirstName = _changeEmployee.FirstName;
        LastName = _changeEmployee.LastName;
        MiddleName = _changeEmployee.MiddleName;
        _pathPhoto = _changeEmployee.Photo;
        _pathScanEmploymentContract = _changeEmployee.ScanEmploymentContract;
        Username = _changeEmployee.Username;
        SelectedRoleFilter = AvailableRoles[0];
        _workStatus = _changeEmployee.WorkStatus;

        BackToEmployeesCommand = new RelayCommand(ExecuteBackToEmployees);
        ChangeEmployeeCommand = new RelayCommand(ExecuteChangeEmployee);
    }

    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _middleName = string.Empty;
    private string? _pathPhoto = null;
    private string? _pathScanEmploymentContract = null;
    private string _username = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;

    private bool _workStatus;

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
        private set => SetProperty(ref _username, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public bool WorkStatus
    {
        get => _workStatus;
        set => SetProperty(ref _workStatus, value);
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

    public Bitmap PhotoImage { get; }
    public Bitmap ContractImage { get; }

    public ICommand BackToEmployeesCommand { get; }
    public ICommand ChangeEmployeeCommand { get; }

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

    private void ExecuteChangeEmployee()
    {
        if (!ValidateInput())
        {
            return;
        }

        _changeEmployee.FirstName = FirstName;
        _changeEmployee.LastName = LastName;
        _changeEmployee.MiddleName = MiddleName;
        _changeEmployee.Photo = _pathPhoto;
        _changeEmployee.ScanEmploymentContract = _pathScanEmploymentContract;
        _changeEmployee.Role = SelectedRoleFilter!.Role ?? EmployeeRole.Waiter;
        _changeEmployee.WorkStatus = WorkStatus;
        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            _changeEmployee.Password = PasswordHashing.HashPassword(NewPassword);
        }

        var context = DatabaseService.GetContext();

        context.Employee.Update(_changeEmployee);

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

        if (string.IsNullOrWhiteSpace(MiddleName) || LastName.Length > 128)
        {
            ErrorMessage = "Обязательное поле фамилию сотрудника длинной не более 128 символов";
            return false;
        }

        if (string.IsNullOrWhiteSpace(LastName) || LastName.Length > 128)
        {
            ErrorMessage = "Обязательное поле фамилию сотрудника длинной не более 128 символов";
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

        if (!NewPassword.Equals(string.Empty) && NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают";
            return false;
        }

        if (!NewPassword.Equals(string.Empty) && NewPassword.Length < 8)
        {
            ErrorMessage = "Пароль должен содержать минимум 8 символов";
            return false;
        }

        if (SelectedRoleFilter == null)
        {
            ErrorMessage = "Выберите роль сотрудника";
            return false;
        }

        if (_changeEmployee.Role == EmployeeRole.Administrator)
        {
            ErrorMessage = "Ошибка сотрудник является администратором";
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }
}
