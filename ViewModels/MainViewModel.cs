using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using QuickRegister.Data;
using QuickRegister.Models;

namespace QuickRegister.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object? _currentView;
        public object? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public AppDbContext Db { get; }
        public Medewerker? CurrentUser { get; private set; }

        public MainViewModel(AppDbContext db)
        {
            Db = db;

            var diff = CsvDiffChecker.CheckDiff(db);
            if (diff.HasChanges)
            {
                var updateVM = new DatabaseUpdateViewModel(db, diff);
                updateVM.OnUpdateCompleted = ShowUserSelection;
                updateVM.OnSkipRequested = ShowUserSelection;
                CurrentView = updateVM;
            }
            else
            {
                ShowUserSelection();
            }
        }

        public void SwitchUser()
        {
            CurrentUser = null;
            CurrentView = new UserSelectionViewModel(Db, this);
        }

        public void ShowUserSelection()
        {
            var recentUser = MedewerkerRepository.GetRecentUser(Db);

            if (recentUser != null)
            {
                UserSelected(recentUser);
                return;
            }

            CurrentView = new UserSelectionViewModel(Db, this);
        }

        public void UserSelected(Medewerker user)
        {
            CurrentUser = user;
            ShowInterventieOverview();
        }

        public void ShowInterventieOverview()
        {
            if (CurrentUser == null)
                throw new InvalidOperationException("No current user set.");

            var overviewVM = new InterventieOverviewViewModel(Db, CurrentUser);

            overviewVM.TerugRequested = SwitchUser;
            overviewVM.NieuweInterventieRequested = () => ShowInterventieForm(null);
            overviewVM.OpenInterventieRequested = interventie =>
                ShowInterventieForm(interventie);

            CurrentView = overviewVM;
        }

        public void ShowInterventieForm(Interventie? interventie)
        {
            if (CurrentUser == null)
                throw new InvalidOperationException("No current user set.");

            var formVM = new InterventieFormViewModel(Db, CurrentUser, interventie);

            formVM.CloseRequested = ShowInterventieOverview;

            CurrentView = formVM;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
