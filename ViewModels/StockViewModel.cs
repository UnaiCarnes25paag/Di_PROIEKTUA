using Dapper;
using Erronka.Data;
using Erronka.Models;
using Erronka.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Erronka.ViewModels
{
    public class StockViewModel : BaseViewModel
    {
        private readonly StockService _stockService;
        public ObservableCollection<Stock> Stocks { get; set; }
        public ObservableCollection<Product> Products { get; set; }

        private Stock _selectedStock;
        public Stock SelectedStock
        {
            get => _selectedStock;
            set { _selectedStock = value; OnPropertyChanged(); }
        }

        public ICommand NewCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public StockViewModel()
        {
            _stockService = new StockService();
            Stocks = new ObservableCollection<Stock>(_stockService.GetAll());
            Products = new ObservableCollection<Product>(LoadProducts());

            NewCommand = new RelayCommand(_ => NewStock());
            AddCommand = new RelayCommand(_ => AddStock());
            UpdateCommand = new RelayCommand(_ => UpdateStock(), _ => SelectedStock != null);
            DeleteCommand = new RelayCommand(_ => DeleteStock(), _ => SelectedStock != null);
        }

        private IEnumerable<Product> LoadProducts()
        {
            using var conn = Database.GetConnection();
            return conn.Query<Product>("SELECT * FROM Products ORDER BY Name").ToList();
        }

        private void NewStock()
        {
            SelectedStock = new Stock
            {
                ProductId = Products.FirstOrDefault()?.Id ?? 0,
                Quantity = 0,
                Location = string.Empty
            };

            Stocks.Add(SelectedStock);
            OnPropertyChanged(nameof(Stocks));
            OnPropertyChanged(nameof(SelectedStock));
        }

        private void AddStock()
        {
            if (SelectedStock == null) return;

            if (SelectedStock.Id == 0)
            {
                _stockService.AddStock(SelectedStock);
            }
            else
            {
                _stockService.UpdateStock(SelectedStock);
            }

            Refresh();
        }

        private void UpdateStock()
        {
            if (SelectedStock == null) return;

            _stockService.UpdateStock(SelectedStock);
            Refresh();
        }

        private void DeleteStock()
        {
            if (SelectedStock == null) return;

            _stockService.DeleteStock(SelectedStock.Id);
            SelectedStock = null;
            Refresh();
        }

        private void Refresh()
        {
            Stocks.Clear();
            foreach (var s in _stockService.GetAll())
                Stocks.Add(s);

            Products.Clear();
            foreach (var p in LoadProducts())
                Products.Add(p);

            OnPropertyChanged(nameof(Stocks));
            OnPropertyChanged(nameof(Products));
        }
    }
}