using System.ComponentModel;
using System.Windows;
using Erronka.ViewModels;

namespace Erronka.Views
{
    public partial class UserMainView : Window
    {
        public UserMainView()
        {
            InitializeComponent();

            // Avoid creating VM at design-time (prevents designer resolution errors)
            if (!DesignerProperties.GetIsInDesignMode(this))
                DataContext = new UserMainViewModel();
        }
    }
}
