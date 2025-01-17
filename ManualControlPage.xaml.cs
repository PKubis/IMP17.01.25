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

        private void StartTimer(string sectionId)
        {
            if (_timers.ContainsKey(sectionId))
            {
                Application.Current.MainPage.DisplayAlert("Info", "Timer dla tej sekcji już działa!", "OK");
                return;
            }

            var section = Sections.FirstOrDefault(sec => sec.Id == sectionId);
            if (section == null) return;

            // Rozpoczęcie sterowania LED w Firebase
            _ = _databaseService.UpdateSectionStatusAsync(_userId, section.Id, "start", section.WateringType);

            var timer = new System.Timers.Timer(1000); // Timer co sekundę
            timer.Elapsed += async (s, e) =>
            {
                section.ElapsedTime++;
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
            var section = Sections.FirstOrDefault(sec => sec.Id == sectionId);
            if (section == null) return;

            if (_timers.TryGetValue(sectionId, out var timer))
            {
                timer.Stop();
                timer.Dispose();
                _timers.Remove(sectionId);
            }

            // Zatrzymanie sterowania LED w Firebase
            await _databaseService.UpdateSectionStatusAsync(_userId, section.Id, "stop", section.WateringType);

            section.ElapsedTime = 0;

            Device.BeginInvokeOnMainThread(() =>
            {
                var updatedSection = Sections.First(sec => sec.Id == sectionId);
                Sections[Sections.IndexOf(updatedSection)] = section;
            });

            // Zapisz zresetowany czas do Firebase
            await _databaseService.UpdateElapsedTimeAsync(_userId, section.Id, section.ElapsedTime);
        }
    }
}
