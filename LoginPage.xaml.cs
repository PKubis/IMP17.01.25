using System;
using Microsoft.Maui.Controls;
using IMP.ViewModels;
using Firebase.Auth;

namespace IMP
{
    public partial class LoginPage : ContentPage
    {
        private bool _isPasswordVisible = false;

        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel(Navigation); // Przekazanie nawigacji do ViewModelu
        }

        private void OnTogglePasswordVisibilityClicked(object sender, EventArgs e)
        {
            // Przełączanie widoczności hasła
            _isPasswordVisible = !_isPasswordVisible;
            PasswordEntry.IsPassword = !_isPasswordVisible;
            ((ImageButton)sender).Source = _isPasswordVisible ? "eye_off.png" : "eye.png";
        }

        private async void OnLoginTapped(object sender, EventArgs e)
        {
            // Przekierowanie obsługi do ViewModelu (logika znajduje się w LoginViewModel)
            var loginViewModel = (LoginViewModel)BindingContext;

            if (await loginViewModel.LoginAsync()) // Metoda LoginAsync w ViewModelu obsługuje proces logowania
            {
                // Przekierowanie po pomyślnym logowaniu
                await Navigation.PushAsync(new HomePage(loginViewModel.UserId));
            }
            else
            {
                // Wyświetlenie komunikatu o błędzie w przypadku niepowodzenia
                await DisplayAlert("Login Failed", "Invalid email or password", "OK");
            }
        }

        private async void OnRegisterTapped(object sender, EventArgs e)
        {
            // Przekierowanie do strony rejestracji
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}
