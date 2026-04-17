using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SendMetrics : MonoBehaviour
{
    private static SendMetrics singleton = null!;
    public static SendMetrics Singleton => singleton;

    private float lastRtt = -1f;
    public string clientId = "unity-client-01";
    public string baseUrl = "http://195.251.58.122:8080";

    private void Awake()
    {
        if (singleton != null)
        {
            Destroy(this);
            Debug.LogWarning("A SendMetrics instance already exists");
            return;
        }

        singleton = this; 
    }

    public void Begin()
    {
        StartCoroutine(SendFPSLoop());
    }

    public void SendTTFR(float seconds)
    {
        StartCoroutine(PostEventTTFR(seconds));
    }

    IEnumerator PostEventTTFR(float value)
    {
        var json = $"{{\"client_id\":\"{clientId}\",\"event_name\":\"ttfr\",\"value\":{value}}}";
        using var req = new UnityWebRequest(baseUrl + "/api/v1/events", "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogError("[METRICS] TTFR send failed: " + req.error);
        else
            Debug.Log("[METRICS] TTFR sent: " + value + "s");
    }

    IEnumerator SendFPSLoop()
    {
        while (true)
        {
            float fps = 1f / Time.deltaTime;
            StartCoroutine(PostEventFPS(fps));
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator PostEventFPS(float fps)
    {
        var json = $"{{\"client_id\":\"{clientId}\",\"event_name\":\"heartbeat\",\"fps\":{fps}}}";

        string url = baseUrl + "/api/v1/events";


        Debug.Log("[METRICS] Posting FPS to: " + url);

        using var req = new UnityWebRequest(url, "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        float t0 = Time.realtimeSinceStartup;
        yield return req.SendWebRequest();
        float rtt = Time.realtimeSinceStartup - t0;

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[METRICS] FPS send failed: " + req.error);
            SendMetric("http_error_total", 1);
        }
        else
        {
            SendMetric("http_rtt_seconds", rtt);

            if (lastRtt > 0)
            {
                float jitter = Mathf.Abs(rtt - lastRtt);
                SendMetric("http_jitter_seconds", jitter);
            }

            lastRtt = rtt;
        }
    }

    public void SendMetric(string eventName, float value)
    {
        var json = $"{{\"client_id\":\"{clientId}\",\"event_name\":\"{eventName}\",\"value\":{value}}}";
        StartCoroutine(PostRaw(json));
    }

    IEnumerator PostRaw(string json)
    {
        using var req = new UnityWebRequest(baseUrl + "/api/v1/events", "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();
    }

}
