using UnityEngine;

public class TalkToggle : MonoBehaviour
{
    public AudioSource audioSource;
    public Animator animator;

    private bool isTalking = true;

    public void ToggleTalk()
    {
        isTalking = !isTalking;

        if (isTalking)
        {
            audioSource.Play();
            animator.SetTrigger("Talk");
        }
        else
        {
            audioSource.Stop();
            animator.SetTrigger("Idle");
        }
    }
}