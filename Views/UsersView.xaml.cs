using System.ComponentModel;
using System.Windows.Controls;
using Erronka.ViewModels;
using System.Windows;

namespace Erronka.Views
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();

            // Avoid creating VM at design-time (prevents designer resolution errors)
            if (!DesignerProperties.GetIsInDesignMode(this))
                DataContext = new UsersViewModel();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsersViewModel vm && vm.SelectedUser != null && sender is PasswordBox pb)
            {
                vm.SelectedUser.PlainPassword = pb.Password;
            }
        }
    }
}