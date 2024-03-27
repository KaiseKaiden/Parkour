using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    AudioSource audioSource;
    bool called;
    public bool pitch = true;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (pitch) audioSource.pitch = Random.Range(0.75f, 1.25f);
    }

    void Update()
    {
        if (audioSource.isPlaying && !called)
        {
            Destroy(gameObject, audioSource.clip.length);
            called = true;
        }
    }
}