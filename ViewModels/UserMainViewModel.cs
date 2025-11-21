using System.Windows;
using System.Windows.Input;
using Erronka.Views;

namespace Erronka.ViewModels
{
    public class UserMainViewModel : BaseViewModel
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand ShowOrdersCommand { get; }
        public ICommand ShowReservationsCommand { get; }
        public ICommand LogoutCommand { get; }

        public UserMainViewModel()
        {
            ShowOrdersCommand = new RelayCommand(_ =>
            {
                // ejemplo: crear la vista y asignarle su VM
                var v = new OrdersView();
                v.DataContext = new OrdersViewModel(App.CurrentUser);
                CurrentView = v;
            });

            ShowReservationsCommand = new RelayCommand(_ =>
            {
                var v = new ReservationsView();
                v.DataContext = new ReservationsViewModel(App.CurrentUser);
                CurrentView = v;
            });

            LogoutCommand = new RelayCommand(_ =>
            {
                var login = new Erronka.Views.LoginView();
                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = login;
                login.Show();
            });

            // Opcional: mostrar por defecto las reservas
            var defaultView = new ReservationsView();
            defaultView.DataContext = new ReservationsViewModel(App.CurrentUser);
            CurrentView = defaultView;
        }
    }
}
