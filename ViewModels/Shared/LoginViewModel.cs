using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

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

        // TODO! 
        // 0 поверить существание пользователя
        // 1 проверить пароль
        // 2 авторизоваться
        // 3 изменить окно на соотвествующее роли (Выход по плану функция в общим шаблоне 'хедера')
    }
}
