using System;
using cafeInformationSystem.Models.Entities;

namespace cafeInformationSystem.Models.AuthService;

public static class AuthStorage
{
    private static Employee? _currentUser;

    public static Employee? CurrentUser
    {
        get => _currentUser;
        set
        {
            if (_currentUser != value)
            {
                _currentUser = value;
                OnCurrentUserChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }

    public static void LogInByUser(Employee user)
    {
        CurrentUser = user;
    }

    public static void LogOut()
    {
        CurrentUser = null;
    }

    public static event EventHandler? OnCurrentUserChanged;

    public static bool IsAuthenticated => CurrentUser != null;
}
