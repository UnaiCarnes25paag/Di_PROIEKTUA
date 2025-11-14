using System.Collections.ObjectModel;
using System.Windows.Input;
using Erronka.Models;
using Erronka.Services;

namespace Erronka.ViewModels
{
    public class UsersViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        public ObservableCollection<User> Users { get; set; }

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set { _selectedUser = value; OnPropertyChanged(); }
        }

        public bool IsAdmin { get; set; }
        public bool IsUserReadOnly => !IsAdmin;

        public ICommand NewCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public UsersViewModel()
        {
            _userService = new UserService();

            IsAdmin = App.CurrentUser?.Role == "Admin";

            if (IsAdmin)
                Users = new ObservableCollection<User>(_userService.GetAllUsers());
            else
                Users = new ObservableCollection<User>(new[] { App.CurrentUser });

            NewCommand = new RelayCommand(_ => NewUser(), _ => IsAdmin);
            AddCommand = new RelayCommand(_ => AddUser(), _ => IsAdmin);
            UpdateCommand = new RelayCommand(_ => UpdateUser(), _ => IsAdmin && SelectedUser != null);
            DeleteCommand = new RelayCommand(_ => DeleteUser(), _ => IsAdmin && SelectedUser != null);
        }

        private void NewUser()
        {
            // Create an empty user and select it for editing
            SelectedUser = new User
            {
                Username = string.Empty,
                FullName = string.Empty,
                Role = "User" // sensible default
            };

            // Add to the UI collection so it appears in the grid immediately
            Users.Add(SelectedUser);
            OnPropertyChanged(nameof(Users));
            OnPropertyChanged(nameof(SelectedUser));
        }

        private void AddUser()
        {
            if (SelectedUser == null) return;

            // If a plain password was entered, hash it and set the hashed fields
            if (!string.IsNullOrEmpty(SelectedUser.PlainPassword))
            {
                PasswordHelper.CreatePasswordHash(SelectedUser.PlainPassword, out byte[] hash, out byte[] salt);
                SelectedUser.PasswordHash = hash;
                SelectedUser.PasswordSalt = salt;
            }

            if (SelectedUser.Id == 0)
            {
                // Insert new user
                _userService.AddUser(SelectedUser);
            }
            else
            {
                // If it already has an Id, treat as update
                _userService.UpdateUser(SelectedUser);
            }

            Refresh();
        }

        private void UpdateUser()
        {
            if (SelectedUser == null) return;

            // If a new plain password was entered, hash and update the stored fields
            if (!string.IsNullOrEmpty(SelectedUser.PlainPassword))
            {
                PasswordHelper.CreatePasswordHash(SelectedUser.PlainPassword, out byte[] hash, out byte[] salt);
                SelectedUser.PasswordHash = hash;
                SelectedUser.PasswordSalt = salt;
            }

            _userService.UpdateUser(SelectedUser);
            Refresh();
        }

        private void DeleteUser()
        {
            if (SelectedUser == null) return;
            _userService.DeleteUser(SelectedUser.Id);
            Refresh();
        }

        private void Refresh()
        {
            Users.Clear();
            foreach (var u in _userService.GetAllUsers())
                Users.Add(u);
        }
    }
}
