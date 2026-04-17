using UnityEngine;

public class PaintingAudioControl : MonoBehaviour
{
    public AudioSource audioSource;

    // Static reference to the currently playing PaintingAudioControl
    private static PaintingAudioControl currentPlaying;

    private bool isPlaying = false;

    public void ToggleAudio()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource is not assigned!");
            return;
        }

        // If this painting is already playing  just pause it
        if (isPlaying)
        {
            audioSource.Pause();
            isPlaying = false;

            // Clear the static reference
            if (currentPlaying == this)
                currentPlaying = null;

            return;
        }

        // If another painting is playing, stop that one
        if (currentPlaying != null && currentPlaying != this)
        {
            currentPlaying.StopAudio();
        }

        // Start this audio
        audioSource.Stop();  // ensures it starts from the beginning
        audioSource.Play();

        isPlaying = true;
        currentPlaying = this;  // set as the active one
    }

    private void StopAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        isPlaying = false;
    }
}
