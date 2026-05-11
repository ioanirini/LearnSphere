using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using GLTFast.Materials;
using UnityEngine.Networking;
using System.Collections;
using System;

public class ModelLoader : MonoBehaviour
{
    [SerializeField] private string modelName = "";

    [SerializeField] private Transform optimizedParent;
    [SerializeField] private Transform unoptimizedParent;

    public event Action LoadingStarted;
    public event Action LoadingFinished;

    private bool loading;

    public bool Loading
    {
        get => loading;
        private set
        {
            loading = value;

            if (value)
                LoadingStarted?.Invoke();
            else
                LoadingFinished?.Invoke();
        }
    }

    private void Start()
    {
        Debug.Log($"[ModelLoader] Start on {name}, modelName='{modelName}', active={gameObject.activeInHierarchy}");

        if (string.IsNullOrEmpty(modelName))
        {
            Debug.LogWarning($"[ModelLoader] {name} not configured. modelName is empty.", gameObject);
            return;
        }

        if (GameManager.Singleton == null)
        {
            Debug.LogError($"[ModelLoader] GameManager.Singleton is NULL on {name}");
            return;
        }

        GameManager.Singleton.StartLoadingAllOptimized += LoadOptimizedModel;
        GameManager.Singleton.StartLoadingAllUnoptimized += LoadUnoptimizedModel;

        Debug.Log($"[ModelLoader] Subscribed {name} to GameManager events");
    }

    private void OnDestroy()
    {
        if (GameManager.Singleton == null) return;

        GameManager.Singleton.StartLoadingAllOptimized -= LoadOptimizedModel;
        GameManager.Singleton.StartLoadingAllUnoptimized -= LoadUnoptimizedModel;
    }

    [ContextMenu("Load Optimized")]
    public void LoadOptimizedModel()
    {
        Debug.Log($"[ModelLoader] LoadOptimizedModel called on {name}, modelName='{modelName}', Loading={Loading}");

        if (Loading) return;

        Clear();
        Loading = true;

        string manifestUrl = $"{GameManager.Singleton.OptimizedManifestPath}/{modelName}/manifest.json";
        StartCoroutine(InstantiateModelsCoroutine(manifestUrl, optimizedParent));
    }

    [ContextMenu("Load Unoptimized")]
    public void LoadUnoptimizedModel()
    {
        Debug.Log($"[ModelLoader] LoadUnoptimizedModel called on {name}, modelName='{modelName}', Loading={Loading}");

        if (Loading) return;

        Clear();
        Loading = true;

        string manifestUrl = $"{GameManager.Singleton.UnoptimizedManifestPath}/{modelName}/manifest.json";
        StartCoroutine(InstantiateModelsCoroutine(manifestUrl, unoptimizedParent));
    }

    private IEnumerator InstantiateModelsCoroutine(string manifestUrl, Transform parent)
    {
        if (parent == null)
        {
            Debug.LogError($"[ModelLoader] Parent is null for {name}");
            Loading = false;
            yield break;
        }

        GlbModel[] packages = null;

        yield return GetModelsFromManifestCoroutine(manifestUrl, result => packages = result);

        if (packages == null || packages.Length == 0)
        {
            Debug.LogWarning($"[ModelLoader] No GLB packages found in manifest: {manifestUrl}");
            Loading = false;
            yield break;
        }

        foreach (GlbModel pkg in packages)
        {
            yield return DownloadAndLoadGlbCoroutine(pkg, parent);
        }

        Loading = false;
    }

    private IEnumerator GetModelsFromManifestCoroutine(string manifestUrl, Action<GlbModel[]> onDone)
    {
        string fullUrl = CombineUrl(GameManager.Singleton.BaseUrl, manifestUrl);

        Debug.Log($"[ModelLoader] Fetching manifest from: {fullUrl}");

        using UnityWebRequest req = UnityWebRequest.Get(fullUrl);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[ModelLoader] Failed to fetch manifest. Error: {req.error}. URL: {fullUrl}");
            onDone?.Invoke(null);
            yield break;
        }

        Debug.Log($"[ModelLoader] Manifest response for {modelName}: {req.downloadHandler.text}");

        Manifest manifest = JsonUtility.FromJson<Manifest>(req.downloadHandler.text);

        if (manifest == null || manifest.packages == null)
        {
            Debug.LogError($"[ModelLoader] Manifest parsed but contains no packages. URL: {fullUrl}");
            onDone?.Invoke(null);
            yield break;
        }

        onDone?.Invoke(manifest.packages);
    }

    private IEnumerator DownloadAndLoadGlbCoroutine(GlbModel pkg, Transform parent)
    {
        if (pkg == null)
        {
            Debug.LogError("[ModelLoader] Package is null.");
            yield break;
        }

        if (string.IsNullOrEmpty(pkg.glb_url))
        {
            Debug.LogError($"[ModelLoader] Package '{pkg.name}' has empty glb_url.");
            yield break;
        }

        string safeName = string.IsNullOrEmpty(pkg.name) ? modelName : pkg.name;
        string root = Path.Combine(Application.persistentDataPath, "models", safeName);
        Directory.CreateDirectory(root);

        string glbPath = Path.Combine(root, Path.GetFileName(pkg.glb_url));
        string glbUrl = CombineUrl(GameManager.Singleton.BaseUrl, pkg.glb_url);

        bool downloaded = false;
        yield return DownloadCoroutine(glbUrl, glbPath, success => downloaded = success);

        if (!downloaded)
        {
            Debug.LogError($"[ModelLoader] Download failed for model {safeName}");
            Loading = false;
            yield break;
        }

        if (!File.Exists(glbPath))
        {
            Debug.LogError($"[ModelLoader] Downloaded file does not exist: {glbPath}");
            Loading = false;
            yield break;
        }

        byte[] modelBytes = File.ReadAllBytes(glbPath);

        if (modelBytes == null || modelBytes.Length == 0)
        {
            Debug.LogError($"[ModelLoader] Could not load model bytes. Path: {glbPath}");
            Loading = false;
            yield break;
        }

        Debug.Log($"[ModelLoader] Loading GLB model '{safeName}' from {glbPath}. Size: {modelBytes.Length} bytes");

        GltfImport importer = CreateGltfImporter();

        Task<bool> importTask = importer.Load(modelBytes);

        yield return new WaitUntil(() => importTask.IsCompleted);

        if (importTask.IsFaulted)
        {
            Debug.LogError($"[ModelLoader] GLB import task failed for {safeName}: {importTask.Exception}");
            Loading = false;
            yield break;
        }

        if (!importTask.Result)
        {
            Debug.LogError($"[ModelLoader] Could not import GLB model {safeName} from: {glbPath}");
            Loading = false;
            yield break;
        }

        Task<bool> instantiateTask = importer.InstantiateMainSceneAsync(parent);

        yield return new WaitUntil(() => instantiateTask.IsCompleted);

        if (instantiateTask.IsFaulted)
        {
            Debug.LogError($"[ModelLoader] GLB instantiate task failed for {safeName}: {instantiateTask.Exception}");
            Loading = false;
            yield break;
        }

        if (!instantiateTask.Result)
        {
            Debug.LogError($"[ModelLoader] Could not instantiate GLB model {safeName}");
            Loading = false;
            yield break;
        }

        Debug.Log($"[ModelLoader] Finished loading model {safeName}");
    }

    private GltfImport CreateGltfImporter()
    {
        Debug.Log("[ModelLoader] Creating GltfImport with BuiltInMaterialGenerator");

        return new GltfImport(
            materialGenerator: new BuiltInMaterialGenerator()
        );
    }

    private IEnumerator DownloadCoroutine(string url, string toPath, Action<bool> onDone)
    {
        Debug.Log($"[ModelLoader] Downloading: {url}");

        string directory = Path.GetDirectoryName(toPath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        if (File.Exists(toPath))
            File.Delete(toPath);

        using UnityWebRequest req = UnityWebRequest.Get(url);
        req.downloadHandler = new DownloadHandlerFile(toPath, false);

        float startTime = Time.realtimeSinceStartup;

        yield return req.SendWebRequest();

        float rtt = Time.realtimeSinceStartup - startTime;

        Debug.Log($"[ModelLoader] Downloaded {req.downloadedBytes} bytes from {url} in {rtt} seconds");

        if (SendMetrics.Singleton != null)
            SendMetrics.Singleton.SendMetric("http_rtt_seconds", rtt);

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[ModelLoader] Could not download from {url} to {toPath}. Error: {req.error}");

            if (SendMetrics.Singleton != null)
                SendMetrics.Singleton.SendMetric("http_error_total", 1);

            onDone?.Invoke(false);
            yield break;
        }

        onDone?.Invoke(true);
    }

    private string CombineUrl(string baseUrl, string path)
    {
        if (string.IsNullOrEmpty(baseUrl))
            return path;

        if (string.IsNullOrEmpty(path))
            return baseUrl;

        return baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
    }

    private void Clear()
    {
        if (optimizedParent != null)
        {
            for (int i = optimizedParent.childCount - 1; i >= 0; i--)
                Destroy(optimizedParent.GetChild(i).gameObject);
        }

        if (unoptimizedParent != null)
        {
            for (int i = unoptimizedParent.childCount - 1; i >= 0; i--)
                Destroy(unoptimizedParent.GetChild(i).gameObject);
        }
    }
}