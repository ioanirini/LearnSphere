using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager singleton = null!;
    public static GameManager Singleton => singleton;

    // TODO: Grab from /config?
    [Header("Configuration")]
    [field: SerializeField]
    public string OptimizedManifestPath { get; private set; } = "/assets/models/optimized";
    [field: SerializeField]
    public string UnoptimizedManifestPath { get; private set; } = "/assets/models/unoptimized";

    [field: SerializeField]
    public bool IsDemoMode { get; private set; } = false;

    [Header("Defaults")]
    [SerializeField]
    private string defaultBaseUrl = "http://localhost:8080";


    public string BaseUrl { get; private set; }

    private const string PLAYER_PREFS_BASE_URL_KEY = "backend_baseUrl";

    public event Action? StartLoadingAllOptimized;
    public event Action? StartLoadingAllUnoptimized;

    private void Awake()
    {
        if (singleton != null)
        {
            Destroy(this);
            Debug.LogWarning("A SendMetrics instance already exists");
            return;
        }

        singleton = this;

        BaseUrl = PlayerPrefs.GetString(PLAYER_PREFS_BASE_URL_KEY, defaultBaseUrl);
    }

    private void Start()
    {
        if (IsDemoMode)
        {
            LoadOptimizedModels();
            return;
        }
    }

    public void SetBaseUrl(string baseUrl)
    {
        BaseUrl = baseUrl;

        PlayerPrefs.SetString(PLAYER_PREFS_BASE_URL_KEY, baseUrl);
        PlayerPrefs.Save();

        Debug.Log($"[GameManager] Host set to: {baseUrl}");
    }

    public void LoadOptimizedModels()
    {
        Debug.Log("Loading optimized models");
        StartLoadingAllOptimized?.Invoke();
    }

    public void LoadUnoptimizedModels()
    {
        Debug.Log("Loading unoptimized models");
        StartLoadingAllUnoptimized?.Invoke();
    }
}