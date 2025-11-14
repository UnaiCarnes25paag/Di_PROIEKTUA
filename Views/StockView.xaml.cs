using System.ComponentModel;
using System.Windows.Controls;
using Erronka.ViewModels;

namespace Erronka.Views
{
    public partial class StockView : UserControl
    {
        public StockView()
        {
            InitializeComponent();

            // Avoid creating VM at design-time (prevents designer resolution errors)
            if (!DesignerProperties.GetIsInDesignMode(this))
                DataContext = new StockViewModel();
        }
    }
}