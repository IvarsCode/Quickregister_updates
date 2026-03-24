using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using QuickRegister.Data;
using QuickRegister.Models;

namespace QuickRegister.ViewModels
{
    public class InterventieOverviewViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;
        private readonly Medewerker _currentUser;

        public string GebruikerNaam => _currentUser.Naam;

        public ObservableCollection<InterventieItemViewModel> Interventies { get; }
            = new ObservableCollection<InterventieItemViewModel>();

        // Commands
        public ICommand TerugCommand { get; }
        public ICommand NieuweInterventieCommand { get; }
        public ICommand OpenInterventieCommand { get; }
        public ICommand ToggleArchiefCommand { get; }

        // Navigation callbacks (set by MainViewModel)
        public Action? TerugRequested { get; set; }
        public Action? NieuweInterventieRequested { get; set; }
        public Action<Interventie>? OpenInterventieRequested { get; set; }

        public InterventieOverviewViewModel(AppDbContext db, Medewerker currentUser)
        {
            _db = db;
            _currentUser = currentUser;

            TerugCommand = new RelayCommand(() => TerugRequested?.Invoke());
            NieuweInterventieCommand = new RelayCommand(() => NieuweInterventieRequested?.Invoke());
            OpenInterventieCommand = new RelayCommand<InterventieItemViewModel>(item =>
            {
                if (item != null)
                    OpenInterventieRequested?.Invoke(item.Interventie);
            });
            ToggleArchiefCommand = new RelayCommand(() =>
            {
                ShowArchief = !ShowArchief;
            });

            LoadInterventies();
        }

        private bool _showArchief = false;
        public bool ShowArchief
        {
            get => _showArchief;
            set
            {
                _showArchief = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ArchiefButtonLabel));
                OnPropertyChanged(nameof(ShowNieuweInterventieButton));
                LoadInterventies();
            }
        }

        public string ArchiefButtonLabel => ShowArchief ? "Actieve interventies" : "Archief";
        public bool ShowNieuweInterventieButton => !ShowArchief;

        private void LoadInterventies()
        {
            Interventies.Clear();

            var result = ShowArchief
                ? InterventieRepository.GetFilteredArchived(_db, SelectedFilter, SearchText, FromDate, ToDate)
                : InterventieRepository.GetFiltered(_db, SelectedFilter, SearchText, FromDate, ToDate);

            foreach (var interventie in result)
                Interventies.Add(new InterventieItemViewModel(interventie));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Filter options
        public Array FilterTypes => Enum.GetValues(typeof(InterventieFilterType));

        private InterventieFilterType _selectedFilter = InterventieFilterType.Bedrijfsnaam;
        public InterventieFilterType SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                _selectedFilter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTextSearch));
                OnPropertyChanged(nameof(IsDateSearch));
                LoadInterventies();
            }
        }

        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                LoadInterventies();
            }
        }

        private DateTimeOffset? _fromDate;
        public DateTimeOffset? FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged();
                LoadInterventies();
            }
        }

        private DateTimeOffset? _toDate;
        public DateTimeOffset? ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged();
                LoadInterventies();
            }
        }

        public bool IsTextSearch =>
            SelectedFilter == InterventieFilterType.Bedrijfsnaam ||
            SelectedFilter == InterventieFilterType.Machine;

        public bool IsDateSearch =>
            SelectedFilter == InterventieFilterType.Datum;
    }

    public class InterventieItemViewModel
    {
        public Interventie Interventie { get; }

        public InterventieItemViewModel(Interventie interventie)
        {
            Interventie = interventie;
        }

        public string Machine => Interventie.Machine;
        public string Bedrijfsnaam => Interventie.BedrijfNaam;

        public DateTime DatumRecentsteCall
        {
            get
            {
                var mostRecentCall = Interventie.Calls
                    .Where(c => c.StartCall.HasValue)
                    .OrderByDescending(c => c.StartCall)
                    .FirstOrDefault();

                return mostRecentCall?.StartCall ?? DateTime.MinValue;
            }
        }

        public int AantalCalls => Interventie.Calls?.Count ?? 0;

        public string InterneNotitiesKort
        {
            get
            {
                var mostRecentCall = Interventie.Calls?
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefault();
                return Trim(mostRecentCall?.InterneNotities);
            }
        }

        public string ExterneNotitiesKort
        {
            get
            {
                var mostRecentCall = Interventie.Calls?
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefault();
                return Trim(mostRecentCall?.ExterneNotities);
            }
        }

        private static string Trim(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return text.Length <= 330 ? text : text.Substring(0, 330) + "...";
        }
    }
}
