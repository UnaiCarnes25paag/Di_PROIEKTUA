using System.Windows;
using Erronka.ViewModels;

namespace Erronka.Views
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}