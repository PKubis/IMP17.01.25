using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using IMP.Models;
using IMP.Services;
using Microsoft.Maui.Controls;

namespace IMP.ViewModels
{
    public class SectionsViewModel : BaseViewModel
    {
        private readonly RealtimeDatabaseService _firebaseService;
        private string? _userId;
        private System.Timers.Timer _refreshTimer;

        public SectionsViewModel()
        {
            _firebaseService = new RealtimeDatabaseService();
            Sections = new ObservableCollection<Section>();
            AvailablePipes = new ObservableCollection<string> { "Rura 16mm", "Rura 25mm", "Rura 32mm" };

            ToggleDayCommand = new Command<string>(ToggleDay);
            AddSectionCommand = new Command(async () => await AddSection());
            DeleteSectionCommand = new Command<string>(async id => await DeleteSection(id));
            EditSectionCommand = new Command<string>(async id => await EditSection(id));
            StopSectionCommand = new Command<string>(async id => await StopSection(id));
        }
        public ObservableCollection<ScheduledHistoryEntry> ScheduledHistory { get; set; } = new ObservableCollection<ScheduledHistoryEntry>();
        public ObservableCollection<ManualHistoryEntry> ManualHistory { get; set; } = new ObservableCollection<ManualHistoryEntry>();

        public SectionsViewModel(string userId) : this()
        {
            _userId = userId;
            LoadSectionsAsync();

            // Timer do regularnego odświeżania danych
            _refreshTimer = new System.Timers.Timer(1000);
            _refreshTimer.Elapsed += async (s, e) =>
            {
                await LoadSectionsAsync();
            };
            _refreshTimer.Start();
        }

        public ObservableCollection<Section> Sections { get; set; }
        public ObservableCollection<string> AvailablePipes { get; set; }

        public ICommand ToggleDayCommand { get; }
        public ICommand AddSectionCommand { get; }
        public ICommand DeleteSectionCommand { get; }
        public ICommand EditSectionCommand { get; }
        public ICommand StopSectionCommand { get; }

        private string _sectionName = string.Empty;
        public string SectionName
        {
            get => _sectionName;
            set => SetProperty(ref _sectionName, value);
        }

        private string _startTime = string.Empty;
        public string StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        private string _duration = string.Empty;
        public string Duration
        {
            get => _duration;
            set
            {
                if (int.TryParse(value, out int result))
                {
                    _duration = result.ToString();
                    SetProperty(ref _duration, _duration);
                }
            }
        }

        private string _selectedPipe = string.Empty;
        public string SelectedPipe
        {
            get => _selectedPipe;
            set => SetProperty(ref _selectedPipe, value);
        }

        private Dictionary<string, string> _dayColors = new();
        public Dictionary<string, string> DayColors
        {
            get => _dayColors;
            private set => SetProperty(ref _dayColors, value);
        }

        private readonly List<string> _selectedDays = new();

        private void ToggleDay(string day)
        {
            if (_selectedDays.Contains(day))
            {
                _selectedDays.Remove(day);
                DayColors[day] = "LightGray";
            }
            else
            {
                _selectedDays.Add(day);
                DayColors[day] = "Teal";
            }
            OnPropertyChanged(nameof(DayColors));
        }

        private async Task LoadSectionsAsync()
        {
            if (string.IsNullOrEmpty(_userId)) return;

            var sections = await _firebaseService.GetSectionsAsync(_userId);
            Device.BeginInvokeOnMainThread(() =>
            {
                Sections.Clear();
                foreach (var section in sections)
                {
                    // Oblicz bieżące zużycie w litrach i m³
                    section.CurrentWaterUsage = CalculateWaterUsageLiters(section.WateringType, section.ElapsedTime);
                    section.CurrentWaterUsageCubicMeters = CalculateWaterUsageCubicMeters(section.WateringType, section.ElapsedTime);

                    // Całkowite zużycie w litrach i m³
                    section.TotalWaterUsageLiters = CalculateWaterUsageLiters(section.WateringType, section.Duration * 60);
                    section.TotalWaterUsageCubicMeters = section.TotalWaterUsageLiters / 1000;

                    Sections.Add(section);
                }
            });
        }
        private async void LoadHistoryAsync()
        {
            var scheduledHistory = await _firebaseService.GetScheduledHistoryAsync(_userId);
            var manualHistory = await _firebaseService.GetManualHistoryAsync(_userId);

            Device.BeginInvokeOnMainThread(() =>
            {
                ScheduledHistory.Clear();
                foreach (var entry in scheduledHistory)
                {
                    ScheduledHistory.Add(entry);
                }

                ManualHistory.Clear();
                foreach (var entry in manualHistory)
                {
                    ManualHistory.Add(entry);
                }
            });
        }







        public void StopRefreshTimer()
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
                _refreshTimer = null;
            }
        }

        private async Task AddSection()
        {
            if (string.IsNullOrWhiteSpace(SectionName) || string.IsNullOrWhiteSpace(StartTime) ||
                string.IsNullOrWhiteSpace(Duration) || string.IsNullOrWhiteSpace(SelectedPipe))
                return;

            var newSection = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = SectionName,
                StartTime = StartTime,
                Duration = int.Parse(Duration),
                SelectedDays = string.Join(", ", _selectedDays),
                WateringType = SelectedPipe,
                Status = "stop"
            };

            // Dodaj sekcję do Firebase tylko wtedy, gdy nie istnieje
            if (!Sections.Any(s => s.Id == newSection.Id))
            {
                await _firebaseService.SaveSectionAsync(_userId, newSection);
                Device.BeginInvokeOnMainThread(() =>
                {
                    Sections.Add(newSection);
                });
            }

            SectionName = string.Empty;
            StartTime = string.Empty;
            Duration = string.Empty;
            SelectedPipe = string.Empty;
            _selectedDays.Clear();
        }

        private async Task EditSection(string sectionId)
        {
            var section = Sections.FirstOrDefault(s => s.Id == sectionId);
            if (section == null) return;

            string newName = await Application.Current.MainPage.DisplayPromptAsync("Edytuj sekcję", "Podaj nową nazwę sekcji:", initialValue: section.Name);
            if (string.IsNullOrWhiteSpace(newName)) return;

            string newStartTime = await Application.Current.MainPage.DisplayPromptAsync("Edytuj czas rozpoczęcia", "Podaj nowy czas rozpoczęcia (HH:mm):", initialValue: section.StartTime);
            if (string.IsNullOrWhiteSpace(newStartTime)) return;

            string newDuration = await Application.Current.MainPage.DisplayPromptAsync("Edytuj czas trwania", "Podaj nowy czas trwania (minuty):", initialValue: section.Duration.ToString());
            if (string.IsNullOrWhiteSpace(newDuration)) return;

            if (!int.TryParse(newDuration, out int duration))
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", "Czas trwania musi być liczbą całkowitą.", "OK");
                return;
            }

            var newDays = await Application.Current.MainPage.DisplayPromptAsync("Edytuj dni", "Podaj nowe dni tygodnia oddzielone przecinkami (np. pn, wt, śr):", initialValue: section.SelectedDays);
            if (string.IsNullOrWhiteSpace(newDays)) return;

            string newPipe = await Application.Current.MainPage.DisplayPromptAsync("Edytuj rurę", "Podaj nowy typ rury (16mm, 25mm, 32mm):", initialValue: section.WateringType);
            if (string.IsNullOrWhiteSpace(newPipe)) return;

            // Aktualizacja danych w sekcji
            section.Name = newName;
            section.StartTime = newStartTime;
            section.Duration = duration;
            section.SelectedDays = newDays;
            section.WateringType = newPipe;

            await _firebaseService.SaveSectionAsync(_userId, section);

            Device.BeginInvokeOnMainThread(() =>
            {
                var index = Sections.IndexOf(section);
                if (index >= 0)
                {
                    Sections[index] = section;
                }
            });
        }
        private async Task StopSection(string sectionId)
        {
            var section = Sections.FirstOrDefault(s => s.Id == sectionId);
            if (section == null) return;

            // Obliczenie rzeczywistego czasu trwania sekcji w sekundach
            int actualElapsedTime = section.ElapsedTime > 0 ? section.ElapsedTime : 1; // Upewnij się, że jest przynajmniej 1 sekunda

            // Zatrzymanie sekcji
            section.Status = "stop";

            // Oblicz całkowite zużycie na podstawie rzeczywistego czasu działania
            section.TotalWaterUsageLiters = CalculateWaterUsageLiters(section.WateringType, actualElapsedTime);
            section.TotalWaterUsageCubicMeters = section.TotalWaterUsageLiters / 1000;

            // Zapis do Firebase
            await _firebaseService.SaveSectionAsync(_userId, section);
            await _firebaseService.UpdateTotalWaterUsageAsync(_userId, section.Id, section.TotalWaterUsageLiters);
            await _firebaseService.AddScheduledHistoryAsync(_userId, new ScheduledHistoryEntry
            {
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                SectionName = section.Name,
                Duration = actualElapsedTime, // Zapisanie rzeczywistego czasu trwania
                WaterUsageLiters = section.TotalWaterUsageLiters,
                WaterUsageCubicMeters = section.TotalWaterUsageCubicMeters
            });

            Device.BeginInvokeOnMainThread(() =>
            {
                var index = Sections.IndexOf(section);
                if (index >= 0)
                {
                    Sections[index] = section;
                }
            });
        }



        private async Task DeleteSection(string sectionId)
        {
            var section = Sections.FirstOrDefault(s => s.Id == sectionId);
            if (section == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Usuń sekcję", $"Czy na pewno chcesz usunąć sekcję \"{section.Name}\"?", "Tak", "Nie");
            if (!confirm) return;

            await _firebaseService.DeleteSectionAsync(_userId, sectionId);
            Sections.Remove(section);
        }
        private readonly Dictionary<string, double> _waterUsageRates = new()
{
    { "Rura 16mm", 600 },  // Litry na godzinę
    { "Rura 25mm", 2500 }, // Litry na godzinę
    { "Rura 32mm", 3300 }  // Litry na godzinę
};

        // Metoda do obliczania bieżącego zużycia w litrach
        // Metoda obliczania bieżącego zużycia w litrach na podstawie czasu
        private double CalculateWaterUsageLiters(string wateringType, int elapsedTimeInSeconds)
        {
            double waterFlowRate = wateringType switch
            {
                "Rura 16mm" => 600.0 / 3600, // Litry na sekundę
                "Rura 25mm" => 2500.0 / 3600,
                "Rura 32mm" => 3300.0 / 3600,
                _ => 0.0
            };

            return waterFlowRate * elapsedTimeInSeconds; // Zużycie w litrach
        }

        // Metoda obliczania bieżącego zużycia w metrach sześciennych na podstawie czasu
        private double CalculateWaterUsageCubicMeters(string wateringType, int elapsedTimeInSeconds)
        {
            return CalculateWaterUsageLiters(wateringType, elapsedTimeInSeconds) / 1000; // Zamiana litrów na m³
        }

    }
}
