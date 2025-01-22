using System;
using Microsoft.Maui.Controls;
using IMP.ViewModels;

namespace IMP
{
    public partial class RegisterPage : ContentPage
    {
        private bool _isPasswordVisible = false;
        private bool _isRepeatPasswordVisible = false;

        public RegisterPage()
        {
            InitializeComponent();
            BindingContext = new RegisterViewModel();
        }

        private void OnTogglePasswordVisibilityClicked(object sender, EventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            PasswordEntry.IsPassword = !_isPasswordVisible;
            ((ImageButton)sender).Source = _isPasswordVisible ? "eye_off.png" : "eye.png";
        }

        private void OnToggleRepeatPasswordVisibilityClicked(object sender, EventArgs e)
        {
            _isRepeatPasswordVisible = !_isRepeatPasswordVisible;
            RepeatPasswordEntry.IsPassword = !_isRepeatPasswordVisible;
            ((ImageButton)sender).Source = _isRepeatPasswordVisible ? "eye_off.png" : "eye.png";
        }

        private async void OnLoginTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }
    }
}
