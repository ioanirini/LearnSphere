using System.Collections;
using UnityEngine;

public class FadeOut : MonoBehaviour
{
    public GameObject blackPanel;

    public FadeOut()
    {
        StartCoroutine(BlackFadeOut());
    }

    private IEnumerator BlackFadeOut()
    {
        blackPanel.SetActive(true);
        yield return new WaitForEndOfFrame();
        blackPanel.SetActive(false);
    }
}
