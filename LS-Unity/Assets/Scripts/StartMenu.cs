using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private GameObject map;

    [Header("Buttons")]
    [SerializeField] private Button loadOptimizedButton;
    [SerializeField] private Button loadUnoptimizedButton;
    [SerializeField] private Button loadNoneButton;

    private bool listenersAdded = false;

    private void Start()
    {
        Debug.Log("[StartMenu] Start");

        if (map != null)
        {
            map.SetActive(GameManager.Singleton.IsDemoMode);
        }

        if (GameManager.Singleton.IsDemoMode)
        {
            return;
        }

        AddButtonListeners();

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Debug.Log("[StartMenu] OnEnable");

        if (GameManager.Singleton != null && !GameManager.Singleton.IsDemoMode)
        {
            AddButtonListeners();
        }
    }

    private void AddButtonListeners()
    {
        if (listenersAdded) return;

        if (loadOptimizedButton != null)
            loadOptimizedButton.onClick.AddListener(OnLoadOptimizedClicked);
        else
            Debug.LogWarning("[StartMenu] loadOptimizedButton is not assigned");

        if (loadUnoptimizedButton != null)
            loadUnoptimizedButton.onClick.AddListener(OnLoadUnoptimizedClicked);
        else
            Debug.LogWarning("[StartMenu] loadUnoptimizedButton is not assigned");

        if (loadNoneButton != null)
            loadNoneButton.onClick.AddListener(OnLoadNoneClicked);
        else
            Debug.LogWarning("[StartMenu] loadNoneButton is not assigned");

        listenersAdded = true;
        Debug.Log("[StartMenu] Button listeners added");
    }

    public void OnLoadOptimizedClicked()
    {
        Debug.Log("[StartMenu] Optimized clicked");
        StartCoroutine(PlayAndLoadOptimizedCoroutine());
    }

    public void OnLoadUnoptimizedClicked()
    {
        Debug.Log("[StartMenu] Unoptimized clicked");
        StartCoroutine(PlayAndLoadUnoptimizedCoroutine());
    }

    public void OnLoadNoneClicked()
    {
        Debug.Log("[StartMenu] None clicked");

        if (map != null)
        {
            map.SetActive(true);
        }

        gameObject.SetActive(false);
    }

    private IEnumerator PlayAndLoadOptimizedCoroutine()
    {
        Debug.Log("[StartMenu] Play optimized");

        if (map != null)
        {
            map.SetActive(true);
        }
        else
        {
            Debug.LogError("[StartMenu] map is not assigned");
            yield break;
        }

        yield return null;
        yield return null;

        Debug.Log("[StartMenu] Calling GameManager.LoadOptimizedModels()");
        GameManager.Singleton.LoadOptimizedModels();

        gameObject.SetActive(false);
    }

    private IEnumerator PlayAndLoadUnoptimizedCoroutine()
    {
        Debug.Log("[StartMenu] Play unoptimized");

        if (map != null)
        {
            map.SetActive(true);
        }
        else
        {
            Debug.LogError("[StartMenu] map is not assigned");
            yield break;
        }

        yield return null;
        yield return null;

        Debug.Log("[StartMenu] Calling GameManager.LoadUnoptimizedModels()");
        GameManager.Singleton.LoadUnoptimizedModels();

        gameObject.SetActive(false);
    }
}