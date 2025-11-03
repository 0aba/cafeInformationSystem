using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
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

namespace cafeInformationSystem.ViewModels.Administrator;

public class RoleFilterItem
{
    public string Name { get; set; } = string.Empty;
    public EmployeeRole? Role { get; set; }
}

public partial class EmployeesViewModel : ViewModelBase
{
    public EmployeesViewModel()
    {
        SelectedRoleFilter = AvailableRoles[0];

        BackToAdministratorMenuCommand = new RelayCommand(ExecuteBackToAdministratorMenu);
        NewEmployeeCommand = new RelayCommand(ExecuteNewEmployee);
        ApplyFiltersCommand = new RelayCommand(ExecuteApplyFilters);
        OpenEmployeeCardCommand = new RelayCommand<string?>(ExecuteOpenEmployeeCard);
    }

    private string _firstNameFilter = string.Empty;

    private string _lastNameFilter = string.Empty;

    private string _middleNameFilter = string.Empty;

    private string _usernameFilter = string.Empty;

    private RoleFilterItem? _selectedRoleFilter;

    // INFO! ObservableCollection используется для ослеживания действий со списоком (Добавлени, изменение, удаление и так далее)
    // в данном случае можно было и просто List или ICollection/ использовать...
    public ObservableCollection<Employee> _employees = new();

    private bool _workStatusFilter = true;

    public List<RoleFilterItem> AvailableRoles { get; } = new()
    {
        new RoleFilterItem { Name = "Все", Role = null },
        new RoleFilterItem { Name = "Администратор", Role = EmployeeRole.Administrator },
        new RoleFilterItem { Name = "Повар", Role = EmployeeRole.Chef },
        new RoleFilterItem { Name = "Официант", Role = EmployeeRole.Waiter },
    };

    private string _errorMessage = string.Empty;

    public string FirstNameFilter
    {
        get => _firstNameFilter;
        set => SetProperty(ref _firstNameFilter, value);
    }

    public string LastNameFilter
    {
        get => _lastNameFilter;
        set => SetProperty(ref _lastNameFilter, value);
    }

    public string MiddleNameFilter
    {
        get => _middleNameFilter;
        set => SetProperty(ref _middleNameFilter, value);
    }

    public string UsernameFilter
    {
        get => _usernameFilter;
        set => SetProperty(ref _usernameFilter, value);
    }

    public RoleFilterItem? SelectedRoleFilter
    {
        get => _selectedRoleFilter;
        set => SetProperty(ref _selectedRoleFilter, value);
    }

    public bool WorkStatusFilter
    {
        get => _workStatusFilter;
        set => SetProperty(ref _workStatusFilter, value);
    }

    public ObservableCollection<Employee> Employees
    {
        get => _employees;
        private set
        {
            if (SetProperty(ref _employees, value))
            {
                OnPropertyChanged(nameof(HasNoEmployees));
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackToAdministratorMenuCommand { get; }
    public ICommand NewEmployeeCommand { get; }
    public ICommand ApplyFiltersCommand { get; }
    public ICommand OpenEmployeeCardCommand { get; }

    private void ExecuteBackToAdministratorMenu()
    {
        Window window = new AdministratorMenuWindow()
        {
            DataContext = new AdministratorMenuViewModel()
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void ExecuteApplyFilters()
    {
        LoadEmployees();
    }

    private void ExecuteNewEmployee()
    {
        Window window = new NewEmployeeWindow()
        {
            DataContext = new NewEmployeeViewModel()
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private void ExecuteOpenEmployeeCard(string? username)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new Exception("Username is null or empty");
        }
        // TODO!
        // перейти к пользователю
        // если его нет (Там обработать)
    }

    private void LoadEmployees()
    {
        try
        {
            var context = DatabaseService.GetContext();

            var query = context.Employee.AsQueryable();

            if (!string.IsNullOrWhiteSpace(FirstNameFilter))
            {
                query = query.Where(e => e.FirstName.Contains(FirstNameFilter));
            }


            if (!string.IsNullOrWhiteSpace(LastNameFilter))
            {
                query = query.Where(e => e.LastName.Contains(LastNameFilter));
            }

            if (!string.IsNullOrWhiteSpace(MiddleNameFilter)) 
            {
                query = query.Where(e => e.MiddleName != null && e.MiddleName.Contains(MiddleNameFilter));
            }

            if (!string.IsNullOrWhiteSpace(UsernameFilter)) 
            {
                query = query.Where(e => e.Username.Contains(UsernameFilter));
            }

            if (SelectedRoleFilter?.Role != null)
            {
                query = query.Where(e => e.Role == SelectedRoleFilter.Role);
            }

            query = query.Where(e => e.WorkStatus == WorkStatusFilter);

            var employees = query.ToList();

            Employees = new ObservableCollection<Employee>(employees);
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка загрузки сотрудников";
            Employees = new();
        }
    }

    public bool HasNoEmployees => Employees.Count == 0;
}
