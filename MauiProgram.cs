using Firebase.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IMP;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
            });

        // Rejestracja Firebase Database
        var firebaseUrl = "https://impdb-557fa-default-rtdb.europe-west1.firebasedatabase.app/";
        var firebaseClient = new FirebaseClient(firebaseUrl);

        // Rejestracja klienta HTTP dla żądań z OpenWeatherMap
        builder.Services.AddHttpClient("WeatherClient", client =>
        {
            client.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");
        });

#if DEBUG
        // Logowanie w trybie debugowania
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
    

}
