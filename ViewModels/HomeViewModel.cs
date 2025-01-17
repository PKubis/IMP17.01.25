using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Firebase.Auth;

namespace IMP.ViewModels
{
    public class HomeViewModel : BindableObject
    {
        private readonly INavigation _navigation;
        private readonly string _userId; // Zmiana z userEmail na userId

        // Komenda do nawigacji do sekcji
        public ICommand NavigateToSectionsCommand { get; }

        // Komenda do nawigacji do ustawień
        public ICommand NavigateToSettingsCommand { get; }
        // Komenda do nawigacji do status
        public ICommand NavigateToStatusCommand { get; }

        public HomeViewModel(INavigation navigation, string userId)
        {
            _navigation = navigation;
            _userId = userId; // Przypisujemy userId

            // Inicjalizacja komendy dla sekcji
            NavigateToSectionsCommand = new Command(async () => await NavigateToSections());

            // Inicjalizacja komendy dla ustawień
            NavigateToSettingsCommand = new Command(async () => await NavigateToSettings());

            // Inicjalizacja komendy dla ustawień
            NavigateToStatusCommand = new Command(async () => await NavigateToStatus());
        }

        private async Task NavigateToSections()
        {
            // Przechodzi do strony sekcji
            await _navigation.PushAsync(new SectionsPage(_userId)); // Przekazujemy userId do SectionsPage
        }

        private async Task NavigateToSettings()
        {
            // Przechodzi do strony ustawień
            await _navigation.PushAsync(new SettingsPage(_userId)); // Przekazujemy userId do SettingsPage
        }
        private async Task NavigateToStatus()
        {
            await _navigation.PushAsync(new StatusPage(_userId)); // Przekazujemy userId do StatusPage
        }
    }
    
}
