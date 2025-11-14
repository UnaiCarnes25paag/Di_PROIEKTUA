using System.Windows.Controls;
using Erronka.ViewModels;

namespace Erronka.Views
{
    public partial class ReservationsView : UserControl
    {
        public ReservationsView()
        {
            InitializeComponent();
            DataContext = new ReservationsViewModel();
        }
    }
}