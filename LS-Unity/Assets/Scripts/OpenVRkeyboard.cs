using UnityEngine;
using TMPro;

public class OpenVRkeyboard : MonoBehaviour
{
    TMP_InputField input;

    void Awake()
    {
        input = GetComponent<TMP_InputField>();
        input.onSelect.AddListener(OpenKeyboard);
    }

    void OpenKeyboard(string _)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        TouchScreenKeyboard.Open(
            input.text,
            TouchScreenKeyboardType.Default,
            false,
            false,
            false,
            false
        );
#endif
    }
}