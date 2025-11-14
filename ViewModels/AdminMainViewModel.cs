using System.Windows;
using System.Windows.Input;

namespace Erronka.ViewModels
{
    public class AdminMainViewModel : BaseViewModel
    {
        private BaseViewModel? _currentView;
        public BaseViewModel? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand ShowUsersCommand { get; }
        public ICommand ShowStockCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminMainViewModel()
        {
            ShowUsersCommand = new RelayCommand(_ => CurrentView = new UsersViewModel());
            ShowStockCommand = new RelayCommand(_ => CurrentView = new StockViewModel());
            LogoutCommand = new RelayCommand(_ =>
            {
                var login = new Views.LoginView();
                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = login;
                login.Show();
            });
        }
    }
}