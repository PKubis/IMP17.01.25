using Firebase.Auth;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using Firebase.Database.Query;

namespace IMP.ViewModels
{
    internal class LoginViewModel : INotifyPropertyChanged
    {
        private readonly string webApiKey = "AIzaSyDNtwI02aWPPvuGGK22Hm8LskD6soyIpZY";
        private readonly INavigation _navigation;
        private string userName;
        private string userPassword;
        private string userId;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand RegisterBtn { get; }
        public ICommand LoginBtn { get; }

        public string UserName
        {
            get => userName;
            set
            {
                userName = value;
                RaisePropertyChanged(nameof(UserName));
            }
        }

        public string UserPassword
        {
            get => userPassword;
            set
            {
                userPassword = value;
                RaisePropertyChanged(nameof(UserPassword));
            }
        }

        public string UserId
        {
            get => userId;
            private set
            {
                userId = value;
                RaisePropertyChanged(nameof(UserId));
            }
        }

        public LoginViewModel(INavigation navigation)
        {
            _navigation = navigation;
            RegisterBtn = new Command(RegisterBtnTappedAsync);
            LoginBtn = new Command(async () => await LoginAsync());
        }

        public async Task<bool> LoginAsync()
        {
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(webApiKey));
            try
            {
                // Logowanie użytkownika
                var auth = await authProvider.SignInWithEmailAndPasswordAsync(UserName, UserPassword);

                // Zapisz token w SecureStorage
                string token = auth.FirebaseToken;
                await SecureStorage.SetAsync("firebase_token", token);

                UserId = auth.User.LocalId;
                string email = auth.User.Email;

                Console.WriteLine($"Logging in UserId: {UserId}, Email: {email}");

                // Sprawdź dane użytkownika w Firebase
                var firebaseClient = new Firebase.Database.FirebaseClient("https://impdb-557fa-default-rtdb.europe-west1.firebasedatabase.app");
                var existingUser = await firebaseClient.Child("users").Child(UserId).OnceSingleAsync<object>();

                if (existingUser == null)
                {
                    // Dodaj użytkownika tylko, jeśli nie istnieje
                    var user = new { Email = email, UserId };
                    await firebaseClient.Child("users").Child(UserId).PutAsync(user);
                    Console.WriteLine("User added to Firebase.");
                }
                else
                {
                    Console.WriteLine("User already exists in Firebase.");
                }

                // Przekierowanie na stronę HomePage po udanym logowaniu
                await _navigation.PushAsync(new HomePage(UserId));

                return true;
            }
            catch (Exception ex)
            {
                // Wyświetl komunikat o błędzie logowania
                Console.WriteLine($"Login failed: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Błąd logowania", "Sprawdź poprawność danych.", "OK");
                return false;
            }
        }



        private async void RegisterBtnTappedAsync(object obj)
        {
            // Przekierowanie do strony rejestracji
            await _navigation.PushAsync(new RegisterPage());
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}