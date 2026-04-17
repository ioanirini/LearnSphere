using System.Collections;
using TMPro;
using UnityEngine;

public class HostConfigurator : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_InputField hostInput;
    [SerializeField]
    private TMP_Text hostLabel;

    public Manifest Config { get; private set; }

    private void Start()
    {
        if (GameManager.Singleton.IsDemoMode)
        {
            gameObject.SetActive(false);
            return;
        }

        if (hostInput != null)
        {    
            hostInput.text = GameManager.Singleton.BaseUrl;
        }
        if (hostLabel != null)
        {
            hostLabel.text = GameManager.Singleton.BaseUrl;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(OpenKeyboardAfterUiInitCoroutine());
#endif
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

    // ============================
    // OK BUTTON
    // ============================
    public void ConfirmHost()
    {
        string? input = hostInput?.text.Trim();

        if (!string.IsNullOrEmpty(input))
        {
            if (!input.StartsWith("http://") && !input.StartsWith("https://"))
            {
                input = "http://" + input;
            }

            GameManager.Singleton.SetBaseUrl(input);
        }

        if (hostLabel != null)
        {
            hostLabel.text = GameManager.Singleton.BaseUrl;
        }
    }
}