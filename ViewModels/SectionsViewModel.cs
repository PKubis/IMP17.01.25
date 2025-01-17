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
                    Sections.Add(section);
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

            await _firebaseService.SaveSectionAsync(_userId, newSection);
            Sections.Add(newSection);

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

            section.Status = "stop";
            await _firebaseService.SaveSectionAsync(_userId, section);
            OnPropertyChanged(nameof(Sections));
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
    }
}
