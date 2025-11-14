using System.Windows;
using Erronka.ViewModels;

namespace Erronka.Views
{
    public partial class AdminMainView : Window
    {
        public AdminMainView()
        {
            InitializeComponent();

            // Assign VM at runtime; avoids designer instantiation problems
            DataContext = new AdminMainViewModel();
        }
    }
}