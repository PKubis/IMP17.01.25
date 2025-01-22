using Microsoft.Maui.Controls;
using IMP.ViewModels;

namespace IMP
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage(string userId)
        {
            InitializeComponent();

            // Przypisanie ViewModelu do BindingContext
            BindingContext = new SettingsViewModel(userId);
        }
    }
}
