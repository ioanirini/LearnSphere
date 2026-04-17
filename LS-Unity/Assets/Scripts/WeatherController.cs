using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class WeatherController : MonoBehaviour
{
    [Header("Particle Systems")]
    public GameObject rainParticleSystem;
    public GameObject snowParticleSystem;
    public GameObject cloudVisuals;

    [Header("API Configuration")]
    public string latitude = "39.15";
    public string longitude = "20.98";
    private string apiUrl = "https://api.open-meteo.com/v1/forecast";

    void Start()
    {
        // Start fetching weather data
        StartCoroutine(GetWeatherData());
    }

    IEnumerator GetWeatherData()
    {
        // Construct the full API URL
        string url = $"{apiUrl}?latitude={latitude}&longitude={longitude}&current=rain,snowfall,cloud_cover&timezone=auto";
        Debug.Log($"Requesting weather data from: {url}");

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Debug raw JSON response
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log($"Weather API Response: {jsonResponse}");

                // Process the response
                ProcessWeatherData(jsonResponse);
            }
            else
            {
                Debug.LogError($"Weather API Error: {webRequest.error}");
            }
        }
    }

    void ProcessWeatherData(string jsonResponse)
    {
        try
        {
            // Parse JSON using Unity's JsonUtility
            WeatherData weatherData = JsonUtility.FromJson<WeatherData>(jsonResponse);
            if (weatherData != null && weatherData.current != null)
            {
                // Debug parsed data
                Debug.Log($"Parsed Weather Data: Rain = {weatherData.current.rain}, Snowfall = {weatherData.current.snowfall}, Cloud Cover = {weatherData.current.cloud_cover}");

                // Manage weather effects
                bool isRaining = weatherData.current.rain > 0;
                bool isSnowing = weatherData.current.snowfall > 0;
                bool isCloudy = weatherData.current.cloud_cover > 50; // Cloudy if cloud cover > 50%

                // Activate or deactivate effects
                rainParticleSystem.SetActive(isRaining);
                snowParticleSystem.SetActive(isSnowing);
                cloudVisuals.SetActive(isCloudy);

                // Debug weather status
                if (isRaining) Debug.Log("It's raining!");
                if (isSnowing) Debug.Log("It's snowing!");
                if (isCloudy) Debug.Log("It's cloudy!");
                if (!isRaining && !isSnowing && !isCloudy) Debug.Log("Weather is clear.");
            }
            else
            {
                Debug.LogWarning("Weather data is null or invalid.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error processing weather data: {ex.Message}");
        }
    }
}

[System.Serializable]
public class WeatherData
{
    public CurrentWeather current;
}

[System.Serializable]
public class CurrentWeather
{
    public float rain;       // Rainfall amount in mm
    public float snowfall;   // Snowfall amount in cm
    public float cloud_cover; // Cloud cover percentage
}
