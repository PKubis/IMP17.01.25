using FFImageLoading;
using FFImageLoading.Work;
using System.Threading.Tasks;
using IMP.Models;
using IMP.Services;

namespace IMP.ViewModels
{
    public class StatusViewModel : BindableObject
    {
        private readonly WeatherService _weatherService = new WeatherService();
        private readonly INavigation _navigation;

        private string _cityName = "Warsaw";
        public string CityName
        {
            get => _cityName;
            set
            {
                _cityName = value;
                OnPropertyChanged();
            }
        }

        private string _weatherInfo;
        public string WeatherInfo
        {
            get => _weatherInfo;
            set
            {
                _weatherInfo = value;
                OnPropertyChanged();
            }
        }

        private string _detailedWeatherInfo;
        public string DetailedWeatherInfo
        {
            get => _detailedWeatherInfo;
            set
            {
                _detailedWeatherInfo = value;
                OnPropertyChanged();
            }
        }

        private string _weatherIconUrl;
        public string WeatherIconUrl
        {
            get => _weatherIconUrl;
            set
            {
                _weatherIconUrl = value;
                OnPropertyChanged();
            }
        }

        public Command RefreshWeatherCommand { get; }

        public StatusViewModel(INavigation navigation, string userId)
        {
            _navigation = navigation;
            RefreshWeatherCommand = new Command(async () => await LoadWeatherData());
            LoadWeatherData();
        }

        private async Task LoadWeatherData()
        {
            try
            {
                var weather = await _weatherService.GetWeatherAsync(CityName);
                WeatherInfo = $"Temperatura: {weather.Main.Temperature}°C, Opis: {weather.Weather[0].Description}";

                DetailedWeatherInfo =
                    $"Wilgotność: {weather.Main.Humidity}%\n" +
                    $"Ciśnienie: {weather.Main.Pressure} hPa\n" +
                    $"Prędkość wiatru: {weather.Wind.Speed} m/s\n" +
                    $"Opady (1h): {weather.Rain?.RainfallLastHour ?? 0} mm"; // Obsługa braku opadów

                // Generowanie URL ikony na podstawie kodu ikony
                string iconCode = weather.Weather[0].Icon; // "04d", "01d", itd.
                WeatherIconUrl = "http://openweathermap.org/img/w/" + iconCode + ".png";
            }
            catch (Exception ex)
            {
                WeatherInfo = "Błąd w pobieraniu danych pogodowych";
                DetailedWeatherInfo = $"Błąd: {ex.Message}";
            }
        }
    }
}