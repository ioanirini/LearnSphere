using System.Collections;
using TMPro;
using UnityEngine;

public class HostConfigurator : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField hostInput;
    [SerializeField] private TMP_Text hostLabel;

    public Manifest Config { get; private set; }

    private void Start()
    {
        if (GameManager.Singleton.IsDemoMode)
        {
            gameObject.SetActive(false);
            return;
        }

        RefreshHostField();

#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(OpenKeyboardAfterUiInitCoroutine());
#endif
    }

    private void OnEnable()
    {
        if (GameManager.Singleton != null && !GameManager.Singleton.IsDemoMode)
        {
            RefreshHostField();
        }
    }

    private void RefreshHostField()
    {
        string savedHost = GameManager.Singleton.BaseUrl;

        if (hostInput != null)
        {
            hostInput.text = savedHost;
        }

        if (hostLabel != null)
        {
            hostLabel.text = savedHost;
        }
    }

    IEnumerator OpenKeyboardAfterUiInitCoroutine()
    {
        yield return null;
        yield return null;

        if (hostInput == null) yield break;

        hostInput.ActivateInputField();
        hostInput.Select();

        if (TouchScreenKeyboard.isSupported)
        {
            TouchScreenKeyboard.Open(
                hostInput.text,
                TouchScreenKeyboardType.URL
            );
        }
    }

    public void ConfirmHost()
    {
        string input = hostInput != null ? hostInput.text.Trim() : "";

        if (!string.IsNullOrEmpty(input))
        {
            if (!input.StartsWith("http://") && !input.StartsWith("https://"))
            {
                input = "http://" + input;
            }

            GameManager.Singleton.SetBaseUrl(input);
        }

        RefreshHostField();
    }
}