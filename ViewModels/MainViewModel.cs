using Erronka.ViewModels;
using System.Windows.Input;

namespace Erronka.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel? _currentView;
        public BaseViewModel? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand ShowStockCommand { get; }
        public ICommand ShowUsersCommand { get; }
        public ICommand ShowReservationsCommand { get; }
        public ICommand ShowOrdersCommand { get; }
        public ICommand ExitCommand { get; }

        public MainViewModel()
        {
            ShowStockCommand = new RelayCommand(_ => CurrentView = new StockViewModel());
            ShowUsersCommand = new RelayCommand(_ => CurrentView = new UsersViewModel());
            ShowReservationsCommand = new RelayCommand(_ => CurrentView = new ReservationsViewModel());
            ShowOrdersCommand = new RelayCommand(_ => CurrentView = new OrdersViewModel());
            ExitCommand = new RelayCommand(_ => System.Windows.Application.Current.Shutdown());
        }
    }
}