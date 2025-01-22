using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Text.Json;
using Microsoft.Maui.Storage;



namespace IMP.ViewModels
{
    public class SettingsViewModel : BindableObject
    {
        private readonly string _userId;

        // Właściwość UserId dostępna w XAML
        public string UserId => _userId;

        public ICommand NavigateToSectionsControlCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand LogoutCommand { get; }

        public SettingsViewModel(string userId)
        {
            _userId = userId;

            // Inicjalizacja komend
            NavigateToSectionsControlCommand = new Command(NavigateToSectionsControl);  
            DeleteAccountCommand = new Command(DeleteAccount);
            ChangePasswordCommand = new Command(async () => await ChangePasswordAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
        }

        // Funkcja do nawigacji do strony sterowania sekcjami
        private async void NavigateToSectionsControl()
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(new ManualControlPage(_userId));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", $"Nie udało się otworzyć strony sterowania sekcjami: {ex.Message}", "OK");
            }
        }

        // Funkcja do nawigacji do strony zmiany hasła
        private async void NavigateToChangePassword()
        {
            try
            {
                // Implementacja zmiany hasła - strona do stworzenia
                //await Application.Current.MainPage.Navigation.PushAsync(new ChangePasswordPage(_userId));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", $"Nie udało się otworzyć strony zmiany hasła: {ex.Message}", "OK");
            }
        }

       

        // Funkcja do usuwania konta
        private async void DeleteAccount()
        {
            bool confirmDelete = await Application.Current.MainPage.DisplayAlert(
                "Usuń konto",
                "Czy na pewno chcesz usunąć swoje konto? Ta akcja jest nieodwracalna.",
                "Tak", "Nie");

            if (!confirmDelete) return;

            try
            {
                // Połączenie z Firebase Database
                var firebaseClient = new FirebaseClient("https://impdb-557fa-default-rtdb.europe-west1.firebasedatabase.app");

                // Sprawdź, czy użytkownik istnieje w bazie danych
                var user = await firebaseClient
                    .Child("users")
                    .Child(_userId)
                    .OnceSingleAsync<User>();

                if (user == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Błąd", "Nie znaleziono użytkownika o podanym ID.", "OK");
                    return;
                }

                // Usuń dane użytkownika z Firebase Database
                await firebaseClient
                    .Child("users")
                    .Child(_userId)
                    .DeleteAsync();

                // Usuń użytkownika z Firebase Authentication
                bool authDeleted = await DeleteUserFromFirebaseAuth();
                if (!authDeleted)
                {
                    await Application.Current.MainPage.DisplayAlert("Błąd", "Nie udało się usunąć konta z Firebase Authentication.", "OK");
                    return;
                }

                // Powiadomienie o sukcesie
                await Application.Current.MainPage.DisplayAlert("Sukces", "Konto zostało usunięte.", "OK");

                // Przekierowanie do strony logowania
                await Application.Current.MainPage.Navigation.PopToRootAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", $"Wystąpił błąd podczas usuwania konta: {ex.Message}", "OK");
            }
        }

        private async Task<bool> DeleteUserFromFirebaseAuth()
        {
            try
            {
                // Pobierz token użytkownika z SecureStorage
                var token = await SecureStorage.GetAsync("firebase_token");

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Błąd", "Brak tokena użytkownika. Zaloguj się ponownie.", "OK");
                    return false;
                }

                // Przygotuj żądanie do Firebase REST API
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"https://identitytoolkit.googleapis.com/v1/accounts:delete?key=AIzaSyDNtwI02aWPPvuGGK22Hm8LskD6soyIpZY"),
                    Content = new StringContent($"{{\"idToken\":\"{token}\"}}", System.Text.Encoding.Default, "application/json")
                };

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {error}");
                    return false;
                }

                // Usuń token z SecureStorage
                SecureStorage.Remove("firebase_token");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return false;
            }
        }
        public async Task ChangePasswordAsync()
        {
            try
            {
                // Pobierz token użytkownika z SecureStorage
                var token = await SecureStorage.GetAsync("firebase_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Błąd", "Brak tokena użytkownika. Zaloguj się ponownie.", "OK");
                    return;
                }

                // Wprowadź nowe hasło (możesz zastąpić nawiązaniem do formularza lub popupu)
                string newPassword = await Application.Current.MainPage.DisplayPromptAsync(
                    "Zmień hasło",
                    "Podaj nowe hasło:",
                    "OK",
                    "Anuluj",
                    placeholder: "Nowe hasło",
                    maxLength: 50,
                    keyboard: Keyboard.Text);

                if (string.IsNullOrEmpty(newPassword))
                {
                    await Application.Current.MainPage.DisplayAlert("Anulowano", "Zmiana hasła została anulowana.", "OK");
                    return;
                }

                // Przygotowanie zapytania do Firebase REST API
                var client = new HttpClient();
                var requestBody = new
                {
                    idToken = token,
                    password = newPassword,
                    returnSecureToken = true
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:update?key=AIzaSyDNtwI02aWPPvuGGK22Hm8LskD6soyIpZY", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    // Wyświetlenie potwierdzenia
                    await Application.Current.MainPage.DisplayAlert("Sukces", "Hasło zostało zmienione.", "OK");
                }
                else
                {
                    // Obsługa błędów
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {errorContent}");
                    await Application.Current.MainPage.DisplayAlert("Błąd", "Nie udało się zmienić hasła. Spróbuj ponownie.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Błąd", $"Wystąpił błąd podczas zmiany hasła: {ex.Message}", "OK");
            }
        }
        private async Task LogoutAsync()
        {
            try
            {
                // Usuń token z SecureStorage
                SecureStorage.Remove("firebase_token");

                // Wyświetl komunikat potwierdzający wylogowanie
                await Application.Current.MainPage.DisplayAlert("Wylogowano", "Zostałeś pomyślnie wylogowany.", "OK");

                // Przekierowanie do strony logowania
                await Application.Current.MainPage.Navigation.PopToRootAsync(); // Powrót do strony początkowej (LoginPage)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout failed: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Błąd", "Wystąpił problem podczas wylogowywania.", "OK");
            }
        }
    }
    
}



    

    // Klasa User dopasowana do struktury bazy danych
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

