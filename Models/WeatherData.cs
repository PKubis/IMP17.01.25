using Newtonsoft.Json;
using System.Collections.Generic;

namespace IMP.Models
{
    public class WeatherData
    {
        [JsonProperty("main")]
        public MainWeatherData Main { get; set; }

        [JsonProperty("weather")]
        public List<WeatherDescription> Weather { get; set; }

        [JsonProperty("wind")]
        public Wind Wind { get; set; }

        [JsonProperty("rain")]
        public Rain Rain { get; set; }
    }

    public class MainWeatherData
    {
        [JsonProperty("temp")]
        public double Temperature { get; set; }

        [JsonProperty("humidity")]
        public int Humidity { get; set; }

        [JsonProperty("pressure")]
        public int Pressure { get; set; }
    }

    public class WeatherDescription
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; } // Kod ikony pogodowej
    }

    public class Wind
    {
        [JsonProperty("speed")]
        public double Speed { get; set; }
    }

    public class Rain
    {
        [JsonProperty("1h")]
        public double RainfallLastHour { get; set; } // Opady w ostatniej godzinie
    }
}
