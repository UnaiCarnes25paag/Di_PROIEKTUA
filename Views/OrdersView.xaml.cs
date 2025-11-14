using System.Windows.Controls;
using Erronka.ViewModels;

namespace Erronka.Views
{
    public partial class OrdersView : UserControl
    {
        public OrdersView()
        {
            InitializeComponent();
            DataContext = new OrdersViewModel();
        }
    }
}