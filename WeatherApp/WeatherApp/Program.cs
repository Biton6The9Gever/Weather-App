using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WeatherApp
{
    public class weatherStats
    {
        public double temp_c { get; set; }
        public double temp_f { get; set; }
        public int is_day { get; set; }
        public double wind_kph { get; set; }
        public double wind_mph { get; set; }
        public int humidity { get; set; }
        public int cloud { get; set; }
        public double feelslike_c { get; set; }
        public double feelslike_f { get; set; }
    }

    public class IpLocationStats
    {
        public string city { get; set; }
    }

    // Wrappers for weather API response
    public class WeatherResponse
    {
        public weatherStats current { get; set; }
    }

    internal class Program
    {
        static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            Console.WriteLine("Would you like we fetch your city using your IP (y/n)");
            if (Console.ReadLine().ToLower() == "y")
            {
                GetPublicCity().GetAwaiter().GetResult();
            }
            else
            {
                UserPreference(location: "New York");
            }
        }

        public static void UserPreference(string location)
        {
            Console.WriteLine("Enter speed type (kph/mph)");
            string speedType = Console.ReadLine();
            Console.WriteLine("Enter degree type (c/f)");
            string degreeType = Console.ReadLine();
            if (speedType.ToLower() != "kph" && speedType.ToLower() != "mph")
                Console.WriteLine("Invalid input");
            else if (degreeType.ToLower() != "c" && degreeType.ToLower() != "f")
                Console.WriteLine("Invalid input");
            else
                RunWeather(degreeType, speedType, location).GetAwaiter().GetResult();
        }

        static async Task GetPublicCity()
        {
            string url = "https://api.ipify.org";
            try
            {
                HttpResponseMessage ipFinderResponse = await client.GetAsync(url);
                if (ipFinderResponse.IsSuccessStatusCode)
                {
                    string publicIP = await ipFinderResponse.Content.ReadAsStringAsync();
                    await GetCityByIp(publicIP);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error | error while trying to fetch your IP \n" + ex);
            }
        }

        static async Task GetCityByIp(string ip)
        {
            string url = $"http://ip-api.com/json/{ip}";
            try
            {
                HttpResponseMessage locationFinderResponse = await client.GetAsync(url);
                if (locationFinderResponse.IsSuccessStatusCode)
                {
                    string json = await locationFinderResponse.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<IpLocationStats>(json);
                    UserPreference(data.city);
                }
                else
                {
                    Console.WriteLine("Could not determine your city");
                    UserPreference("New York");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error | while fetching your City \n" + ex);
            }
        }

        static async Task RunWeather(string degreeType, string speedType, string location)
        {
            string apiKey = "cfa69a78c67b434181994718252004";
            Console.WriteLine("location");
            string url = $"http://api.weatherapi.com/v1/current.json?key={apiKey}&q={location}&aqi=no";
            try
            {
                HttpResponseMessage weatherResponse = await client.GetAsync(url);
                if (weatherResponse.IsSuccessStatusCode)
                {
                    string json = await weatherResponse.Content.ReadAsStringAsync();
                    WeatherResponse data = JsonSerializer.Deserialize<WeatherResponse>(json);

                    Console.WriteLine($"Temperature in {(degreeType == "c" ? "Celsius" : "Fahrenheit")}: {(degreeType == "c" ? data.current.temp_c.ToString() : data.current.temp_f.ToString())}");
                    Console.WriteLine($"Feels Like ({(degreeType == "c" ? "Celsius" : "Fahrenheit")}): {(degreeType == "c" ? data.current.feelslike_c.ToString() : data.current.feelslike_f.ToString())}");
                    Console.WriteLine($"Humidity: {data.current.humidity}%");
                    Console.WriteLine($"Cloud Cover: {data.current.cloud}%");
                    Console.WriteLine($"Wind ({(speedType.ToLower() == "kph" ? "kph" : "mph")}): {(speedType.ToLower() == "kph" ? data.current.wind_kph.ToString() : data.current.wind_mph.ToString())}");
                    Console.WriteLine($"Is Day: {(data.current.is_day == 1 ? "Yes" : "No")}");
                }
                else
                {
                    Console.WriteLine($"Failed to fetch weather data. Status code: {weatherResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error | error while trying to fetch your weather Data \n" + ex);
            }
        }
    }
}
