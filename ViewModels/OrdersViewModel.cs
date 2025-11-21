using Dapper;
using Erronka.Data;
using Erronka.Models;
using Erronka.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Erronka.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly OrderService _orderService;
        private readonly TicketService _ticketService;
        private readonly ReservationService _reservationService;
        private readonly User _currentUser;

        public ObservableCollection<Reservation> Reservations { get; set; } = new();
        public ObservableCollection<Product> Products { get; set; } = new();
        public ObservableCollection<Product> Cart { get; set; } = new();

        private Reservation _selectedReservation;
        public Reservation SelectedReservation
        {
            get => _selectedReservation;
            set
            {
                _selectedReservation = value;
                OnPropertyChanged();

                IsReservationSelected = _selectedReservation != null;

                if (IsReservationSelected)
                    LoadProducts();
            }
        }

        private bool _isReservationSelected;
        public bool IsReservationSelected
        {
            get => _isReservationSelected;
            set { _isReservationSelected = value; OnPropertyChanged(); }
        }

        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand PayCommand { get; }

        public OrdersViewModel() : this(App.CurrentUser) { }

        public OrdersViewModel(User user)
        {
            _currentUser = user;

            _reservationService = new ReservationService();
            _orderService = new OrderService();
            _ticketService = new TicketService();

            AddToCartCommand = new RelayCommand(AddToCart);
            RemoveFromCartCommand = new RelayCommand(RemoveFromCart);
            PayCommand = new RelayCommand(Pay);

            LoadReservations();
        }

        private void LoadReservations()
        {
            var userRes = _reservationService
                .GetByUserIdWithTable(_currentUser.Id)
                .OrderBy(r => r.Date)
                .ThenBy(r => r.TimeSlot);

            Reservations = new ObservableCollection<Reservation>(userRes);
            OnPropertyChanged(nameof(Reservations));
        }

        private void LoadProducts()
        {
            using var conn = Database.GetConnection();

            var sql = @"SELECT 
                        p.Id, p.Name, p.Price,
                        COALESCE(s.Quantity,0) AS Stock
                        FROM Products p
                        LEFT JOIN Stock s ON p.Id = s.ProductId";

            var products = conn.Query<Product>(sql).ToList();

            Products = new ObservableCollection<Product>(products);
            OnPropertyChanged(nameof(Products));
        }

        private void AddToCart(object obj)
        {
            if (obj is Product p)
            {
                if (p.Stock <= 0)
                {
                    MessageBox.Show("Ez dago stock nahikorik.");
                    return;
                }

                Cart.Add(p);
                p.Stock--;

                OnPropertyChanged(nameof(Products));
                OnPropertyChanged(nameof(Cart));
            }
        }

        private void RemoveFromCart(object obj)
        {
            if (obj is Product product)
            {
                var existing = Cart.FirstOrDefault(x => x.Id == product.Id);

                if (existing != null)
                {
                    Cart.Remove(existing);
                    product.Stock++;

                    OnPropertyChanged(nameof(Products));
                    OnPropertyChanged(nameof(Cart));
                }
            }
        }

        private void Pay(object obj)
        {
            if (SelectedReservation == null)
            {
                MessageBox.Show("Aukeratu mahai bat lehenengo.");
                return;
            }

            if (!Cart.Any())
            {
                MessageBox.Show("Orga hutsa.");
                return;
            }

            var items = Cart
                .GroupBy(p => p.Id)
                .Select(g => new OrderItem
                {
                    ProductId = g.Key,
                    Quantity = g.Count()
                })
                .ToList();

            var order = new Order
            {
                TableId = SelectedReservation.TableId,
                UserId = _currentUser.Id,
                CreatedAt = DateTime.Now,
                Paid = false,
                Items = items
            };

            using var conn = Database.GetConnection();
            using var tran = conn.BeginTransaction();

            int orderId = -1;

            try
            {
                orderId = _orderService.CreateOrderTransactional(order, conn, tran);

                foreach (var item in items)
                {
                    int currentStock = conn.ExecuteScalar<int>(
                        "SELECT Quantity FROM Stock WHERE ProductId = @Pid",
                        new { Pid = item.ProductId },
                        tran
                    );

                    if (currentStock < item.Quantity)
                    {
                        tran.Rollback();
                        MessageBox.Show($"Ez dago stock nahikorik produktuarentzat (Produktua ID: {item.ProductId}).");
                        return;
                    }

                    conn.Execute(
                        "UPDATE Stock SET Quantity = Quantity - @Qty WHERE ProductId = @Pid",
                        new { Qty = item.Quantity, Pid = item.ProductId },
                        tran
                    );
                }

                conn.Execute(
                    "DELETE FROM Reservations WHERE Id = @Id",
                    new { Id = SelectedReservation.Id },
                    tran
                );

                conn.Execute(
                    "UPDATE Orders SET Paid = 1 WHERE Id = @Id",
                    new { Id = orderId },
                    tran
                );

                tran.Commit();
            }
            catch (Exception ex)
            {
                try { tran.Rollback(); } catch { }
                MessageBox.Show("Errorea ordainketan: " + ex.Message);
                return;
            }

            string path = _ticketService.GenerateTicket(orderId);
            MessageBox.Show($"Sortutako Ticketa: {path}");

            Cart.Clear();
            OnPropertyChanged(nameof(Cart));

            LoadReservations();
            LoadProducts();

            SelectedReservation = null;
            IsReservationSelected = false;

            MessageBox.Show("Ordainketa ondo gauzatu da.");
        }
    }
}
