using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using QuickRegister.Data;
using QuickRegister.Models;

namespace QuickRegister.ViewModels
{
    public class UserSelectionViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;
        private readonly MainViewModel _main;
        private readonly AppStateHelpers _helpers;

        public ObservableCollection<Medewerker> FilteredUsers { get; } = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
                FilterUsers();
            }
        }

        private Medewerker? _selectedUser;
        public Medewerker? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser == value) return;
                _selectedUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedUser));
            }
        }

        public bool HasSelectedUser => SelectedUser != null;

        private Medewerker? _recentUser;
        public Medewerker? RecentUser
        {
            get => _recentUser;
            set
            {
                if (_recentUser == value) return;
                _recentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasRecentUser));
            }
        }

        public bool HasRecentUser => RecentUser != null;

        public ICommand SelectUserCommand { get; }

        public UserSelectionViewModel(AppDbContext db, MainViewModel main)
        {
            _db = db;
            _main = main;

            // Command to select a user
            SelectUserCommand = new RelayCommand<Medewerker?>(SelectUser);

            // Load recent user from AppState
            RecentUser = MedewerkerRepository.GetRecentUser(_db);
            _helpers = new AppStateHelpers(_db);
            _helpers.EnsureUniekeTellingExists();

            // Auto-select the recent user if one exists
            if (RecentUser != null)
            {
                SelectUser(RecentUser);
                return;
            }
        }

        private void FilterUsers()
        {
            FilteredUsers.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
                return;

            try
            {
                // Get top 4 matches using repository
                var results = MedewerkerRepository.Search(_db, SearchText);

                foreach (var user in results)
                    FilteredUsers.Add(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FilterUsers] Exception: {ex}");
            }
        }

        private void SelectUser(Medewerker? user)
        {
            if (user == null) return;

            SelectedUser = user;
            RecentUser = user;

            // Save recent user in AppState
            MedewerkerRepository.SaveRecentUser(_db, user.Id);

            // Notify MainViewModel to switch to InterventieOverview
            _main.UserSelected(user);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
