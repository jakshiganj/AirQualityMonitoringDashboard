using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using AirQualityMonitoringDashboard.Repositories;
using AirQualityMonitoringDashboard.Models;

namespace AirQualityMonitoringDashboard.Services
{
    public class AQIDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IAQIDataRepository _aqiRepository;
        private readonly ISensorRepository _sensorRepository;
        private readonly string[] _apiUrls;

        public AQIDataService(HttpClient httpClient, IAQIDataRepository aqiRepository, ISensorRepository sensorRepository, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _aqiRepository = aqiRepository;
            _sensorRepository = sensorRepository;
            //_apiUrl = $"{configuration["AirQualityAPI:BaseUrl"]}?token={configuration["AirQualityAPI:ApiKey"]}";
            //_apiUrl = "https://api.waqi.info/feed/A44956/?token=6cd062a794368ba7c64fa81812167409b7bc3949";
            //_apiUrl = "https://api.waqi.info/feed/A132322/?token=6cd062a794368ba7c64fa81812167409b7bc3949";
            _apiUrls = new string[]
        {
            "https://api.waqi.info/feed/A44956/?token=6cd062a794368ba7c64fa81812167409b7bc3949",
            "https://api.waqi.info/feed/A132322/?token=6cd062a794368ba7c64fa81812167409b7bc3949"  // Add other API URLs here
        };
        }

        public async Task FetchAndStoreAirQualityData()
        {
            foreach (var apiUrl in _apiUrls)
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);


                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(json);

                    if (data["status"]?.ToString() == "ok")
                    {
                        double latitude = (double)data["data"]["city"]["geo"][0];
                        double longitude = (double)data["data"]["city"]["geo"][1];

                        // Find the sensor in the database
                        var sensor = await _sensorRepository.GetSensorByLocation(latitude, longitude);


                        if (sensor != null)
                        {
                            var reading = new AQIData
                            {
                                SensorId = sensor.Id,
                                AQI = ParseInt(data["data"]["aqi"]),
                                //AQI = (int)data["data"]["aqi"],
                                PM10 = data["data"]["iaqi"]["pm10"]?["v"]?.ToObject<float?>(),
                                PM25 = data["data"]["iaqi"]["pm25"]?["v"]?.ToObject<float?>(),
                                CO = data["data"]["iaqi"]["co"]?["v"]?.ToObject<float?>(),
                                NO2 = data["data"]["iaqi"]["no2"]?["v"]?.ToObject<float?>(),
                                O3 = data["data"]["iaqi"]["o3"]?["v"]?.ToObject<float?>(),
                                SO2 = data["data"]["iaqi"]["so2"]?["v"]?.ToObject<float?>(),
                                Temperature = data["data"]["iaqi"]["t"]?["v"]?.ToObject<float?>(),
                                Humidity = data["data"]["iaqi"]["h"]?["v"]?.ToObject<float?>(),
                                Pressure = data["data"]["iaqi"]["p"]?["v"]?.ToObject<float?>(),
                                WindSpeed = data["data"]["iaqi"]["w"]?["v"]?.ToObject<float?>(),
                                RecordedAt = DateTime.UtcNow
                            };

                            await _aqiRepository.AddReading(reading);
                        }
                    }
                }
            }
        }
    
    private int ParseInt(JToken value)
        {
            if (value == null)
            {
                return 0; // Default value if null
            }

            int parsedValue;
            if (int.TryParse(value.ToString(), out parsedValue))
            {
                return parsedValue;
            }

            // If parsing fails, handle the case (e.g., return a default value or log an error)
            return 0;
        }
    }
}
