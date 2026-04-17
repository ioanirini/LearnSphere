using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class BackendPoster
{
    public static IEnumerator PostJson(string baseUrl, string path, string json, string apiToken = "")
    {
        using var req = new UnityWebRequest($"{baseUrl}{path}", "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(apiToken))
            req.SetRequestHeader("Authorization", $"Bearer {apiToken}");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[BackendPoster] POST {path} failed: {req.error}");
        }
        else
        {
            Debug.Log($"[BackendPoster] POST {path} OK {req.responseCode}");
        }
    }
}
