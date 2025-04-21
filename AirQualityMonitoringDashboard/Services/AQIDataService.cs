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

        public AQIDataService(
            HttpClient httpClient,
            IAQIDataRepository aqiRepository,
            ISensorRepository sensorRepository,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _aqiRepository = aqiRepository;
            _sensorRepository = sensorRepository;
            _apiUrls = configuration.GetSection("AirQualityAPI:Urls").Get<string[]>();
        }

        public async Task FetchAndStoreAirQualityData()
        {
            foreach (var apiUrl in _apiUrls)
            {
                try
                {
                    HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(json);

                    if (data["status"]?.ToString() == "ok")
                    {
                        double latitude = (double)data["data"]["city"]["geo"][0];
                        double longitude = (double)data["data"]["city"]["geo"][1];

                        var sensor = await _sensorRepository.GetSensorByLocation(latitude, longitude);
                        if (sensor != null)
                        {
                            var reading = new AQIData
                            {
                                SensorId = sensor.Id,
                                AQI = ParseInt(data["data"]["aqi"]),
                                PM10 = ParseFloat(data["data"]["iaqi"]["pm10"]?["v"]),
                                PM25 = ParseFloat(data["data"]["iaqi"]["pm25"]?["v"]),
                                CO = ParseFloat(data["data"]["iaqi"]["co"]?["v"]),
                                NO2 = ParseFloat(data["data"]["iaqi"]["no2"]?["v"]),
                                O3 = ParseFloat(data["data"]["iaqi"]["o3"]?["v"]),
                                SO2 = ParseFloat(data["data"]["iaqi"]["so2"]?["v"]),
                                Temperature = ParseFloat(data["data"]["iaqi"]["t"]?["v"]),
                                Humidity = ParseFloat(data["data"]["iaqi"]["h"]?["v"]),
                                Pressure = ParseFloat(data["data"]["iaqi"]["p"]?["v"]),
                                WindSpeed = ParseFloat(data["data"]["iaqi"]["w"]?["v"]),
                                RecordedAt = DateTime.UtcNow
                            };

                            await _aqiRepository.AddReading(reading);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching data from {apiUrl}: {ex.Message}");
                }
            }
        }

        private int ParseInt(JToken value)
        {
            if (value == null) return 0;
            return int.TryParse(value.ToString(), out int result) ? result : 0;
        }

        private float? ParseFloat(JToken value)
        {
            if (value == null) return null;
            return float.TryParse(value.ToString(), out float result) ? result : null;
        }
    }
}