using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine.Networking;
using System.Collections;
using System;

public class ModelLoader : MonoBehaviour
{
    [SerializeField]
    private string modelName = "";
    [SerializeField]
    private Transform optimizedParent;
    [SerializeField]
    private Transform unoptimizedParent;

    public event Action? LoadingStarted;
    public event Action? LoadingFinished;

    private bool loading;
    public bool Loading
    {
        get => loading;
        private set
        {
            loading = value;
            if (value)
            {
                LoadingStarted?.Invoke();
            }
            else
            {
                LoadingFinished?.Invoke();
            }
        }
    }

    private void Start()
    {
        if (modelName.Length == 0)
        {
            Debug.LogWarning($"ModelLoader {name} not configured", gameObject);
            return;
        }

        GameManager.Singleton.StartLoadingAllOptimized += LoadOptimizedModel;
        GameManager.Singleton.StartLoadingAllUnoptimized += LoadUnoptimizedModel;
    }

    private void OnDestroy()
    {
        GameManager.Singleton.StartLoadingAllOptimized -= LoadOptimizedModel;
        GameManager.Singleton.StartLoadingAllUnoptimized -= LoadUnoptimizedModel;
    }

    [ContextMenu("LoadOptimized")]
    public void LoadOptimizedModel()
    {
        if (Loading) { return; }

        Clear();
        Loading = true;
        InstantiateModels($"{GameManager.Singleton.OptimizedManifestPath}/{modelName}/manifest.json", optimizedParent);
    }

    [ContextMenu("LoadUnoptimized")]
    public void LoadUnoptimizedModel()
    {
        if (Loading) { return; }

        Clear();
        Loading = true;
        InstantiateModels($"{GameManager.Singleton.UnoptimizedManifestPath}/{modelName}/manifest.json", unoptimizedParent);
    }

    private void InstantiateModels(string manifestUrl, Transform parent)
    {
        StartCoroutine(InstantiateModelsCoroutine(manifestUrl));
        IEnumerator InstantiateModelsCoroutine(string manifestUrl)
        {
            GlbModel[] packages = null;
            yield return GetModelsFromManifestCoroutine(manifestUrl, (p) => packages = p);

            if (packages == null || packages.Length == 0)
            {
                Debug.LogWarning($"No OBJ packages found in manifest: {manifestUrl}");
                Loading = false;
                yield break;
            }

            foreach (GlbModel pkg in packages)
            {
                yield return DownloadAndLoadGlbCoroutine(pkg, parent);
            }

            Loading = false;
        }
    }

    private IEnumerator GetModelsFromManifestCoroutine(string manifestUrl, System.Action<GlbModel[]> onDone)
    {
        using var req = UnityWebRequest.Get($"{GameManager.Singleton.BaseUrl}{manifestUrl}");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch manifest: " + req.error);
            onDone?.Invoke(null);
            yield break;
        }

        var manifest = JsonUtility.FromJson<Manifest>(req.downloadHandler.text);

        onDone?.Invoke(manifest?.packages);
    }

    private IEnumerator DownloadAndLoadGlbCoroutine(GlbModel pkg, Transform parent)
    {
        string root = Path.Combine(Application.persistentDataPath, "models", pkg.name);
        Directory.CreateDirectory(root);

        string glbPath = Path.Combine(root, Path.GetFileName(pkg.glb_url));

        yield return DownloadCoroutine(GameManager.Singleton.BaseUrl + pkg.glb_url, glbPath);

        GltfImport importer = new();
        byte[] modelBytes = File.ReadAllBytes(glbPath);
        if (modelBytes.Length == 0)
        {
            Debug.LogError($"Could not load model {modelName}", this);
            yield break;
        }

        Task<bool> importTask = importer.Load(modelBytes);

        yield return new WaitUntil(() => importTask.IsCompleted);
        if (!importTask.Result)
        {
            Debug.LogError($"Could not import glb model {pkg.name} from: {glbPath}");
            yield break;
        }

        Task<bool> instantiateTask = importer.InstantiateMainSceneAsync(parent);
        yield return new WaitUntil(() => importTask.IsCompleted);
        if (!instantiateTask.Result)
        {
            Debug.LogError($"Could not instantiate glb model {pkg.name}");
            yield break;
        }
        Debug.Log($"Finished loading model {modelName}");
    }

    private IEnumerator DownloadCoroutine(string url, string toPath)
    {
        Debug.Log($"Downloading: {url}");
        Directory.CreateDirectory(Path.GetDirectoryName(toPath));

        if (File.Exists(toPath)) { File.Delete(toPath); }

        using var req = UnityWebRequest.Get(url);

        req.downloadHandler = new DownloadHandlerFile(toPath, false);

        float startTime = Time.realtimeSinceStartup;
        yield return req.SendWebRequest();
        float rtt = Time.realtimeSinceStartup - startTime;
        Debug.Log($"Downloaded {req.downloadedBytes} bytes from {url} in {rtt}");

        SendMetrics.Singleton.SendMetric("http_rtt_seconds", rtt);

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Could not download from {url} to {toPath}");
            SendMetrics.Singleton.SendMetric("http_error_total", 1);
        }
    }

    private void Clear()
    {
        for (int i = 0; i < optimizedParent.childCount; i++)
        {
            Destroy(optimizedParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < unoptimizedParent.childCount; i++)
        {
            Destroy(unoptimizedParent.GetChild(i).gameObject);
        }
    }
}
