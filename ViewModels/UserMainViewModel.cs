using System.Windows;
using System.Windows.Input;

namespace Erronka.ViewModels
{
    public class UserMainViewModel : BaseViewModel
    {
        public BaseViewModel CurrentView { get; set; }

        public ICommand ShowOrdersCommand { get; }
        public ICommand ShowReservationsCommand { get; }
        public ICommand LogoutCommand { get; }

        public UserMainViewModel()
        {
            ShowOrdersCommand = new RelayCommand(_ => CurrentView = new OrdersViewModel());
            ShowReservationsCommand = new RelayCommand(_ => CurrentView = new ReservationsViewModel());
            LogoutCommand = new RelayCommand(_ =>
            {
                // Return to LoginView
                var login = new Erronka.Views.LoginView();
                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = login;
                login.Show();
            });
        }
    }
}