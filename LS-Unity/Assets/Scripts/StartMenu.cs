using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject map;

    [Header("Buttons")]
    [SerializeField]
    private Button loadOptimizedButton;
    [SerializeField]
    private Button loadUnoptimizedButton;
    [SerializeField]
    private Button loadNoneButton;

    private void Start()
    {
        gameObject.SetActive(false);
        map.SetActive(GameManager.Singleton.IsDemoMode);

        if (GameManager.Singleton.IsDemoMode)
        {
            return;
        }

        loadOptimizedButton.onClick.AddListener(() =>
        {
            Play();
            GameManager.Singleton.LoadOptimizedModels();
        });

        loadUnoptimizedButton.onClick.AddListener(() =>
        {
            Play();
            GameManager.Singleton.LoadUnoptimizedModels();
        });
        
        loadNoneButton.onClick.AddListener(() => Play());
    }

    private void Play()
    {
        gameObject.SetActive(false);
        map.SetActive(true);
    }
}
