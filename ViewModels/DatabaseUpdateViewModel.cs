using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using QuickRegister.Data;

namespace QuickRegister.ViewModels;

public class SelectableCsvItem : INotifyPropertyChanged
{
    public string FileName { get; }
    public string CsvType { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public SelectableCsvItem(string fileName, string csvType)
    {
        FileName = fileName;
        CsvType = csvType;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class DatabaseUpdateViewModel : INotifyPropertyChanged
{
    private readonly AppDbContext _db;
    private readonly CsvDiff _diff;

    public ObservableCollection<SelectableCsvItem> CsvItems { get; } = new();
    public Action? OnUpdateCompleted { get; set; }
    public Action? OnSkipRequested { get; set; }
    public ICommand UpdateCommand { get; }
    public ICommand SkipCommand { get; }

    public DatabaseUpdateViewModel(AppDbContext db, CsvDiff diff)
    {
        _db = db;
        _diff = diff;

        if (diff.HasMachineChanges)
            CsvItems.Add(new SelectableCsvItem("MachineLijst.csv", "machines"));
        if (diff.HasMedewerkerChanges)
            CsvItems.Add(new SelectableCsvItem("MedewerkerLijst.csv", "medewerkers"));
        if (diff.HasBedrijfChanges)
            CsvItems.Add(new SelectableCsvItem("BedrijvenLijst.csv", "bedrijven"));

        UpdateCommand = new RelayCommand(ApplyUpdate);
        SkipCommand = new RelayCommand(() => OnSkipRequested?.Invoke());
    }

    private void ApplyUpdate()
    {
        bool machines = CsvItems.Any(i => i.CsvType == "machines" && i.IsSelected);
        bool medewerkers = CsvItems.Any(i => i.CsvType == "medewerkers" && i.IsSelected);
        bool bedrijven = CsvItems.Any(i => i.CsvType == "bedrijven" && i.IsSelected);
        CsvDiffChecker.ApplyDiff(_db, _diff, machines, medewerkers, bedrijven);
        OnUpdateCompleted?.Invoke();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
