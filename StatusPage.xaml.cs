using Microsoft.Maui.Controls;
using IMP.ViewModels;

namespace IMP
{
    public partial class StatusPage : ContentPage
    {
        public StatusPage(string userId)
        {
            InitializeComponent();
            BindingContext = new StatusViewModel(Navigation, userId);
        }
    }
}
