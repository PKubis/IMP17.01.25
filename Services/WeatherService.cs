using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using IMP.Models;

namespace IMP.Services
{
    public class WeatherService
    {
        private const string ApiKey = "45da252491bfcfd3d1e8525f594af666"; // Wprowadź swój klucz API OpenWeather
        private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";

        public async Task<WeatherData> GetWeatherAsync(string city)
        {
            using var client = new HttpClient();
            var url = $"{BaseUrl}?q={city}&units=metric&appid={ApiKey}";

            var response = await client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<WeatherData>(response);
        }
    }
}
