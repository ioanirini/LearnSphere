using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI; // For Unity UI
using TMPro; // For TextMeshPro
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Text.RegularExpressions;

public class GrafanaDataFetcher : MonoBehaviour
{
    [Header("API Configuration")]
    private string apiUrl = "https://grafana.ath.hcmr.gr:8080/api/public/dashboards/4b812965f316402ab6838c7ec8992593/panels/10/query";


    [Header("Request Parameters")]
    public string timezone = "Europe/Athens";
    public int intervalMs = 3600000; // 1-hour interval
    public int maxDataPoints = 675;

    [Header("Game Objects")]
    public GameObject objectA; // Active when water level < 0.1m
    public GameObject objectB; // Active when water level >= 0.1m

    [Header("UI Elements")]
    public Text waterDepthText; // For Unity UI Text
    public TextMeshProUGUI waterDepthTMP; // For TextMeshPro

    private double lastWaterDepth = 0.0;

    void Start()
    {
        //new code
        if (waterDepthText != null)
            waterDepthText.text = "Water Depth: 0.00 meters";
        if (waterDepthTMP != null)
            waterDepthTMP.text = "Water Depth: 0.00 meters";


        // Dynamically calculate timestamps
        long toTimestamp = GetCurrentUnixTimestampMs(); // Current time in milliseconds
        long fromTimestamp = toTimestamp - (5 * 60 * 60 * 1000); // 5 hours earlier

        // Start fetching data
        StartCoroutine(FetchGrafanaData(fromTimestamp, toTimestamp));
    }

    IEnumerator FetchGrafanaData(long fromTimestamp, long toTimestamp)
    {
        // Construct JSON payload
        string jsonPayload = $"{{\"intervalMs\":{intervalMs},\"maxDataPoints\":{maxDataPoints},\"timeRange\":{{\"from\":\"{fromTimestamp}\",\"to\":\"{toTimestamp}\",\"timezone\":\"{timezone}\"}}}}";
        Debug.Log($"Request Payload: {jsonPayload}");

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Raw JSON Response: {request.downloadHandler.text}");
                ProcessResponse(request.downloadHandler.text);
                WaterLevel(); // Call WaterLevel after processing the response
                UpdateCanvasDisplay(); // Call the new method to update the canvas
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
                //new code
                lastWaterDepth = 0.0;
                WaterLevel();
                UpdateCanvasDisplay();
            }
        }
    }

    void ProcessResponse(string jsonResponse)
    {
        Debug.Log($"Raw JSON Response: {jsonResponse}");

        try
        {
            // Match the "values" array using Regex
            Match match = Regex.Match(jsonResponse, "\"values\":\\[\\[(.*?)\\],\\[(.*?)\\]\\]");

            if (match.Success)
            {
                // Extract the two arrays
                string[] timestampsArray = match.Groups[1].Value.Split(',');
                string[] waterDepthsArray = match.Groups[2].Value.Split(',');

                // Get the last values
                double lastTimestamp = double.Parse(timestampsArray[^1], CultureInfo.InvariantCulture);
                lastWaterDepth = double.Parse(waterDepthsArray[^1], CultureInfo.InvariantCulture);

                // Convert timestamp to readable date
                DateTimeOffset lastDate = DateTimeOffset.FromUnixTimeMilliseconds((long)lastTimestamp);
                string formattedDate = lastDate.ToString("yyyy-MM-dd HH:mm:ss");

                // Round water depth to two decimals
                lastWaterDepth = Math.Round(lastWaterDepth, 2);

                // Log the results
                Debug.Log($"Last Timestamp: {formattedDate} (Unix: {lastTimestamp})");
                Debug.Log($"Last Water Depth: {lastWaterDepth}");
            }
            else
            {
                Debug.LogWarning("Failed to find 'values' array in the JSON response.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error processing JSON: {ex.Message}");
        }
    }

    //void WaterLevel()
    //{
    //    // Check the water level and activate the corresponding GameObject
    //    if (lastWaterDepth < 0.1)
    //    {
    //        objectA.SetActive(true);
    //        objectB.SetActive(false);
    //    }
    //    else
    //    {
    //        objectA.SetActive(false);
    //        objectB.SetActive(true);

    //        // Adjust the Y position of Object B based on water depth
    //        float minY = 4f; // Minimum height (corresponds to 0 meters depth)
    //        float maxY = 20f; // Maximum height (corresponds to 2 meters depth)
    //        float depthToYFactor = (float)(lastWaterDepth); // Scale depth proportionally

    //        float newY = Mathf.Clamp(minY + depthToYFactor * (maxY - minY), minY, maxY);
    //        Vector3 newPosition = objectB.transform.position;
    //        newPosition.y = newY;
    //        objectB.transform.position = newPosition;

    //        Debug.Log($"Object B Y Position Adjusted: {newY}");
    //    }
    //}

    void WaterLevel()
    {
        // Check if objectA and objectB are assigned
        if (objectA == null && objectB == null)
        {
            Debug.LogWarning("No objects assigned for water level display. Skipping object activation.");
            return; // Exit the function if both objects are not assigned
        }

        if (lastWaterDepth < 0.1)
        {
            if (objectA != null) objectA.SetActive(true);
            if (objectB != null) objectB.SetActive(false);
        }
        else
        {
            if (objectA != null) objectA.SetActive(false);
            if (objectB != null)
            {
                objectB.SetActive(true);

                // Adjust the Y position of Object B based on water depth
                float minY = 4f; // Minimum height (corresponds to 0 meters depth)
                float maxY = 20f; // Maximum height (corresponds to 2 meters depth)
                float depthToYFactor = (float)(lastWaterDepth); // Scale depth proportionally

                float newY = Mathf.Clamp(minY + depthToYFactor * (maxY - minY), minY, maxY);
                Vector3 newPosition = objectB.transform.position;
                newPosition.y = newY;
                objectB.transform.position = newPosition;

                Debug.Log($"Object B Y Position Adjusted: {newY}");
            }
        }
    }


    void UpdateCanvasDisplay()
    {
        // Update the text on the canvas with the water depth
        if (waterDepthText != null)
        {
            waterDepthText.text = $"Water Depth: {lastWaterDepth:F2} meters";
        }

        if (waterDepthTMP != null)
        {
            waterDepthTMP.text = $"Water Depth: {lastWaterDepth:F2} meters";
        }

        Debug.Log("Canvas display updated with water depth.");
    }

    long GetCurrentUnixTimestampMs()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
