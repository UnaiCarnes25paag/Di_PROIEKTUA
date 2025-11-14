using Erronka.Data;
using Erronka.Models;
using Erronka.Views;
using System.Windows;

namespace Erronka
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Crea/semilla tpv.db si hace falta
            Database.InitializeDatabase();

            // Mostrar login
            var login = new LoginView();
            Current.MainWindow = login;
            login.Show();
        }
    }
}