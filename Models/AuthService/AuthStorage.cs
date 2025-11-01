using System;
using cafeInformationSystem.Models.Entities;

namespace cafeInformationSystem.Models.AuthService
{
    public static class AuthStorage
    {
        private static Employee? _currentUser;

        public static Employee? CurrentUser
        {
            get
            {
                return _currentUser;
            }
            set
            {
                _currentUser = value;
                OnCurrentUserChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static void LogOut()
        {
            CurrentUser = null;
        }

        public static bool IsAuthenticated => CurrentUser != null;

        public static event EventHandler OnCurrentUserChanged = default!;
    }
}
