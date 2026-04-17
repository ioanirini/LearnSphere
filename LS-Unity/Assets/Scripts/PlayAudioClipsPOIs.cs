using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioClipsPOIs : MonoBehaviour
{
    public AudioSource audioSource; // Assign in Inspector
    private bool isPlaying = false; // Flag to track playing state

    public void ToggleAudio()
    {
        if (audioSource == null) return; // Safety check

        if (isPlaying)
        {
            // If playing, stop it
            audioSource.Stop();
            isPlaying = false;
        }
        else
        {
            // If not playing, play from the start
            audioSource.Play();
            isPlaying = true;
        }
    }

    private void Update()
    {
        // If the audio stops playing naturally, reset flag
        if (!audioSource.isPlaying)
        {
            isPlaying = false;
        }
    }
}
