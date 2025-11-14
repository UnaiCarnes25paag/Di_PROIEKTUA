using Dapper;
using Erronka.Data;
using Erronka.Models;
using Erronka.Services;
using System;
using System.Collections.Generic;
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
        private readonly User _currentUser;

        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<Product> Cart { get; set; }

        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand PayCommand { get; }

        // Parameterless ctor for places that construct OrdersViewModel without a user
        public OrdersViewModel() : this(App.CurrentUser) { }

        public OrdersViewModel(User currentUser)
        {
            _currentUser = currentUser;
            _orderService = new OrderService();
            _ticketService = new TicketService();

            Products = new ObservableCollection<Product>(LoadAllProducts());
            Cart = new ObservableCollection<Product>();

            AddToCartCommand = new RelayCommand(AddToCart);
            RemoveFromCartCommand = new RelayCommand(RemoveFromCart);
            PayCommand = new RelayCommand(Pay);
        }

        private IEnumerable<Product> LoadAllProducts()
        {
            using var conn = Database.GetConnection();
            // Join with Stock to populate Product.Stock (Stock table exists in DB)
            var sql = @"SELECT p.Id, p.Name, p.Price, COALESCE(s.Quantity, 0) AS Stock
                        FROM Products p
                        LEFT JOIN Stock s ON p.Id = s.ProductId";
            return conn.Query<Product>(sql).ToList();
        }

        private void UpdateProductStock(Product product)
        {
            using var conn = Database.GetConnection();
            // Update the Stock table quantity for the product
            conn.Execute("UPDATE Stock SET Quantity = @Quantity WHERE ProductId = @ProductId",
                new { Quantity = product.Stock, ProductId = product.Id });
        }

        private void AddToCart(object obj)
        {
            if (obj is Product product && product.Stock > 0)
            {
                Cart.Add(product);
                product.Stock--;
                UpdateProductStock(product);
                OnPropertyChanged(nameof(Products));
            }
        }

        private void RemoveFromCart(object obj)
        {
            if (obj is Product product)
            {
                // remove only a single instance from the cart
                var toRemove = Cart.FirstOrDefault(p => p.Id == product.Id);
                if (toRemove != null)
                {
                    Cart.Remove(toRemove);
                    product.Stock++;
                    UpdateProductStock(product);
                    OnPropertyChanged(nameof(Products));
                }
            }
        }

        private void Pay(object obj)
        {
            // Total as decimal, convert product.Price (double) to decimal explicitly
            decimal total = 0m;
            foreach (var p in Cart)
                total += (decimal)p.Price;

            // Build Order with aggregated items (product counts)
            var grouped = Cart.GroupBy(p => p.Id)
                              .Select(g => new OrderItem
                              {
                                  ProductId = g.Key,
                                  Quantity = g.Count()
                              }).ToList();

            var order = new Order
            {
                TableId = null,
                UserId = _currentUser?.Id ?? 0,
                CreatedAt = DateTime.Now,
                Paid = false,
                Items = grouped
            };

            // Persist order using OrderService
            int orderId = _orderService.CreateOrder(order);

            // Generate ticket (writes file and returns path)
            var ticketPath = _ticketService.GenerateTicket(orderId);
            MessageBox.Show($"Ticket generado: {ticketPath}", "Ticket", MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear cart and reload products (stock changes were already persisted)
            Cart.Clear();
            Products = new ObservableCollection<Product>(LoadAllProducts());
            OnPropertyChanged(nameof(Products));
        }
    }
}