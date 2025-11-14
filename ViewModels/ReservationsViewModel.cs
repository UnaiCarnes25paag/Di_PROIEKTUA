using Dapper;
using Erronka.Data;
using Erronka.Models;
using Erronka.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Erronka.ViewModels
{
    public class ReservationsViewModel : BaseViewModel
    {
        private readonly ReservationService _reservationService;
        public ObservableCollection<Reservation> Reservations { get; set; }
        public ObservableCollection<Table> Tables { get; set; }

        private Reservation _selectedReservation;
        public Reservation SelectedReservation
        {
            get => _selectedReservation;
            set { _selectedReservation = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public ReservationsViewModel()
        {
            _reservationService = new ReservationService();

            Reservations = new ObservableCollection<Reservation>(_reservationService.GetAll());
            Tables = new ObservableCollection<Table>(LoadTables());

            AddCommand = new RelayCommand(_ => Add(), _ => SelectedReservation != null);
            UpdateCommand = new RelayCommand(_ => Update(), _ => SelectedReservation != null);
            DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedReservation != null);
        }

        private IEnumerable<Table> LoadTables()
        {
            using var conn = Database.GetConnection();
            return conn.Query<Table>("SELECT * FROM Tables").ToList();
        }

        private void Add()
        {
            if (SelectedReservation == null) return;
            var id = _reservationService.Create(SelectedReservation);
            SelectedReservation.Id = id;
            Refresh();
        }

        private void Update()
        {
            if (SelectedReservation == null) return;
            _reservationService.Update(SelectedReservation);
            Refresh();
        }

        private void Delete()
        {
            if (SelectedReservation == null) return;
            _reservationService.Delete(SelectedReservation.Id);
            SelectedReservation = null;
            Refresh();
        }

        private void Refresh()
        {
            Reservations.Clear();
            foreach (var r in _reservationService.GetAll())
                Reservations.Add(r);

            Tables = new ObservableCollection<Table>(LoadTables());
            OnPropertyChanged(nameof(Tables));
        }
    }
}