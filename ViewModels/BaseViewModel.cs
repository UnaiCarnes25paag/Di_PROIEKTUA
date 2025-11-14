using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Erronka.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        // Make event nullable to match INotifyPropertyChanged signature
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}