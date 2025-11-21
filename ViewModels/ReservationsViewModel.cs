using Dapper;
using Erronka.Data;
using Erronka.Models;
using Erronka.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Erronka.ViewModels
{
    public class ReservationsViewModel : BaseViewModel
    {
        private readonly ReservationService _reservationService;
        private readonly User _currentUser;
        private readonly bool _isAdmin;

        public ObservableCollection<Reservation> Reservations { get; set; } = new ObservableCollection<Reservation>();
        public ObservableCollection<Table> Tables { get; set; } = new ObservableCollection<Table>();
        public ObservableCollection<Table> AvailableTables { get; set; } = new ObservableCollection<Table>();

        private Reservation _selectedReservation;
        public Reservation SelectedReservation
        {
            get => _selectedReservation;
            set
            {
                _selectedReservation = value;
                OnPropertyChanged();
                UpdateTablesIfPossible();
            }
        }

        public ICommand NewCommand { get; private set; }
        public ICommand AddCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        public ReservationsViewModel()
        {
            _isAdmin = true;
            _currentUser = null;
            _reservationService = new ReservationService();

            Tables = new ObservableCollection<Table>(LoadTables());
            RefreshCollections();

            NewCommand = new RelayCommand(_ => CreateNew());
            AddCommand = new RelayCommand(_ => Add(), _ => SelectedReservation != null);
            UpdateCommand = new RelayCommand(_ => Update(), _ => CanUpdate());
            DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedReservation != null);
        }

        public ReservationsViewModel(User user)
        {
            _isAdmin = false;
            _currentUser = user;
            _reservationService = new ReservationService();

            Tables = new ObservableCollection<Table>(LoadTables());
            RefreshCollections();

            NewCommand = new RelayCommand(_ => CreateNew());
            AddCommand = new RelayCommand(_ => Add(), _ => SelectedReservation != null);
            UpdateCommand = new RelayCommand(_ => Update(), _ => CanUpdate());
            DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedReservation != null && SelectedReservation.CreatedByUserId == _currentUser.Id);
        }

        private IEnumerable<Table> LoadTables()
        {
            using var conn = Database.GetConnection();
            return conn.Query<Table>("SELECT * FROM Tables").ToList();
        }

        private void LoadAvailableTables()
        {
            if (SelectedReservation == null) return;

            var tables = _reservationService.GetAvailableTables(
                SelectedReservation.Date,
                SelectedReservation.TimeSlot
            );

            AvailableTables.Clear();
            foreach (var t in tables)
                AvailableTables.Add(t);
        }

        private void RefreshCollections()
        {
            Reservations.Clear();
            if (_isAdmin)
            {
                foreach (var r in _reservationService.GetAll())
                    Reservations.Add(r);
            }
            else
            {
                if (_currentUser != null)
                {
                    foreach (var r in _reservationService.GetByUserIdWithTable(_currentUser.Id))
                        Reservations.Add(r);
                }
            }
        }

        private void CreateNew()
        {
            SelectedReservation = new Reservation
            {
                Date = DateTime.Today,
                TimeSlot = "13:00",
                CreatedByUserId = _isAdmin ? 0 : _currentUser.Id
            };

            OnPropertyChanged(nameof(SelectedReservation));
            LoadAvailableTables();

            SelectedReservation.TableId = AvailableTables.FirstOrDefault()?.Id ?? 0;
        }

        private void Add()
        {
            if (SelectedReservation == null) return;

            try
            {
                if (!_isAdmin)
                    SelectedReservation.CreatedByUserId = _currentUser.Id;

                var id = _reservationService.Create(SelectedReservation);
                SelectedReservation.Id = id;

                SelectedReservation.Table = Tables.First(t => t.Id == SelectedReservation.TableId);
                RefreshCollections();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }


        private bool CanUpdate()
        {
            if (SelectedReservation == null) return false;
            if (_isAdmin) return true;
            return SelectedReservation.CreatedByUserId == _currentUser.Id;
        }

        private void Update()
        {
            if (!CanUpdate()) return;

            try
            {
                _reservationService.Update(SelectedReservation);
                RefreshCollections();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void UpdateTablesIfPossible()
        {
            if (SelectedReservation == null) return;

            if (SelectedReservation.Date != default && !string.IsNullOrEmpty(SelectedReservation.TimeSlot))
            {
                LoadAvailableTables();
            }
        }

        private void Delete()
        {
            if (SelectedReservation == null) return;
            if (!_isAdmin && SelectedReservation.CreatedByUserId != _currentUser.Id) return;

            _reservationService.Delete(SelectedReservation.Id);
            SelectedReservation = null;
            RefreshCollections();
        }
    }
}
