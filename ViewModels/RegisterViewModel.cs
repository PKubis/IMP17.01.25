using Firebase.Auth;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using IMP.Services;

namespace IMP.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private string email;
        private string password;
        private string repeatPassword;
        private readonly RealtimeDatabaseService _realtimeDatabaseService;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand RegisterUser { get; }

        public string Email
        {
            get => email;
            set
            {
                email = value;
                RaisePropertyChanged(nameof(Email));
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
                RaisePropertyChanged(nameof(Password));
            }
        }

        public string RepeatPassword
        {
            get => repeatPassword;
            set
            {
                repeatPassword = value;
                RaisePropertyChanged(nameof(RepeatPassword));
            }
        }

        public RegisterViewModel()
        {
            _realtimeDatabaseService = new RealtimeDatabaseService();
            RegisterUser = new Command(RegisterUserTappedAsync);
        }

        private async void RegisterUserTappedAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(RepeatPassword))
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", "Wszystkie pola są wymagane.", "OK");
                return;
            }

            if (Password != RepeatPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", "Hasła nie są zgodne.", "OK");
                return;
            }

            try
            {
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyDNtwI02aWPPvuGGK22Hm8LskD6soyIpZY"));
                var auth = await authProvider.CreateUserWithEmailAndPasswordAsync(Email, Password);

                string userId = auth.User.LocalId;
                string email = auth.User.Email;

                // Dodaj użytkownika do Firebase Realtime Database
                await _realtimeDatabaseService.AddUserAsync(userId, email);

                // Przekierowanie do logowania
                await Application.Current.MainPage.Navigation.PushAsync(new LoginPage());
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", ex.Message, "OK");
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
