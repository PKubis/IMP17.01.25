using IMP.Models;
using IMP.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace IMP
{
    public partial class ManualControlPage : ContentPage
    {
        public ObservableCollection<Section> Sections { get; set; } = new ObservableCollection<Section>();

        private readonly string _userId;
        private readonly RealtimeDatabaseService _databaseService;
        private readonly Dictionary<string, System.Timers.Timer> _timers = new Dictionary<string, System.Timers.Timer>();

        public Command<string> StartCommand { get; }
        public Command<string> StopCommand { get; }

        public ManualControlPage(string userId)
        {
            InitializeComponent();

            _userId = userId;
            _databaseService = new RealtimeDatabaseService();

            StartCommand = new Command<string>(StartTimer);
            StopCommand = new Command<string>(StopTimer);

            BindingContext = this;

            LoadSectionsAsync(); // Pobranie danych
        }

        private async Task LoadSectionsAsync()
        {
            var sections = await _databaseService.GetSectionsAsync(_userId);
            Sections.Clear();
            foreach (var section in sections)
            {
                Sections.Add(section);
            }
        }

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

        private void StartTimer(string sectionId)
        {
            if (_timers.ContainsKey(sectionId))
            {
                Application.Current.MainPage.DisplayAlert("Info", "Timer dla tej sekcji już działa!", "OK");
                return;
            }

            var section = Sections.FirstOrDefault(sec => sec.Id == sectionId);
            if (section == null) return;

            // Rozpoczęcie sterowania LED
            Task.Run(async () =>
            {
                await _databaseService.UpdateSectionStatusAsync(_userId, section.Id, "start", section.WateringType);
            });

            var timer = new System.Timers.Timer(1000); // Timer co sekundę
            timer.Elapsed += async (s, e) =>
            {
                section.ElapsedTime++;
                section.CurrentWaterUsage = CalculateWaterUsageLiters(section.WateringType, section.ElapsedTime);
                section.CurrentWaterUsageCubicMeters = section.CurrentWaterUsage / 1000;

                // Aktualizacja danych sekcji w Firebase
                await _databaseService.UpdateElapsedTimeAsync(_userId, section.Id, section.ElapsedTime);

                Device.BeginInvokeOnMainThread(() =>
                {
                    var updatedSection = Sections.First(sec => sec.Id == sectionId);
                    Sections[Sections.IndexOf(updatedSection)] = section;
                });
            };

            timer.Start();
            _timers[sectionId] = timer;
        }

        private async void StopTimer(string sectionId)
        {
            // Znajdź sekcję
            var section = Sections.FirstOrDefault(sec => sec.Id == sectionId);
            if (section == null) return;

            // Zatrzymaj i usuń timer
            if (_timers.TryGetValue(sectionId, out var timer))
            {
                timer.Stop();
                timer.Dispose();
                _timers.Remove(sectionId);
            }

            // Obliczanie zużycia wody
            double waterUsageLiters = CalculateWaterUsageLiters(section.WateringType, section.ElapsedTime);
            double waterUsageCubicMeters = waterUsageLiters / 1000;

            // Dodaj wpis do historii
            var entry = new ManualHistoryEntry
            {
                SectionName = section.Name,
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Duration = section.ElapsedTime,
                WaterUsageLiters = waterUsageLiters, // Zużycie w litrach
                WaterUsageCubicMeters = waterUsageCubicMeters // Zużycie w m³
            };

            await _databaseService.AddManualHistoryAsync(_userId, entry);

            // Zaktualizuj status sekcji w Firebase
            await _databaseService.UpdateSectionStatusAsync(_userId, section.Id, "stop", section.WateringType);

            // Resetowanie danych sekcji
            section.ElapsedTime = 0;
            section.CurrentWaterUsage = 0;
            section.CurrentWaterUsageCubicMeters = 0;

            // Zaktualizuj sekcję na interfejsie użytkownika
            Device.BeginInvokeOnMainThread(() =>
            {
                var updatedSection = Sections.First(sec => sec.Id == sectionId);
                Sections[Sections.IndexOf(updatedSection)] = section;
            });

            // Aktualizuj dane w Firebase
            await _databaseService.UpdateElapsedTimeAsync(_userId, section.Id, 0);
        }

    }
}