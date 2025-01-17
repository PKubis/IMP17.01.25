using IMP.ViewModels;
using Firebase.Auth;
using IMP.Services;

namespace IMP
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        public HomePage(string userId)
        {
            InitializeComponent();
            BindingContext = new HomeViewModel(Navigation, userId); // Przekazujemy userId
        }
    }
}
