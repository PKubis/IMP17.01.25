using IMP.ViewModels;

namespace IMP
{
    public partial class SectionsPage : ContentPage
    {
        public SectionsPage(string userId)
        {
            InitializeComponent();
            BindingContext = new SectionsViewModel(userId);
        }
    }

}

