using Erronka.Models;
using Erronka.Services;
using System.Windows;
using System.Windows.Input;

namespace Erronka.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        private readonly AuthService _authService;

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(Login);
        }

        private void Login(object obj)
        {
            var user = _authService.Login(Username, Password);

            if (user == null)
            {
                ErrorMessage = "Erabiltzaile edo pasahitz okerra.";
                return;
            }

            App.CurrentUser = user;

            Window nextWindow;

            if (user.Role == "Admin")
                nextWindow = new Views.AdminMainView();
            else
                nextWindow = new Views.UserMainView();

            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = nextWindow;
            nextWindow.Show();
        }
    }
}
