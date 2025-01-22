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
        private readonly string _userId;

        // Komenda do nawigacji do sekcji
        public ICommand NavigateToSectionsCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand NavigateToStatusCommand { get; }
        public ICommand NavigateToHistoryCommand { get; } // Komenda do nawigacji do historii

        public HomeViewModel(INavigation navigation, string userId)
        {
            _navigation = navigation;
            _userId = userId;

            NavigateToSectionsCommand = new Command(async () => await NavigateToSections());
            NavigateToSettingsCommand = new Command(async () => await NavigateToSettings());
            NavigateToStatusCommand = new Command(async () => await NavigateToStatus());
            NavigateToHistoryCommand = new Command(async () => await navigation.PushAsync(new HistoryPage(userId)));
        }

        private async Task NavigateToSections()
        {
            await _navigation.PushAsync(new SectionsPage(_userId));
        }

        private async Task NavigateToSettings()
        {
            await _navigation.PushAsync(new SettingsPage(_userId));
        }

        private async Task NavigateToStatus()
        {
            await _navigation.PushAsync(new StatusPage(_userId));
        }

        private async Task NavigateToHistory()
        {
            await _navigation.PushAsync(new HistoryPage(_userId)); // Przekierowanie na HistoryPage
        }
    }
}
