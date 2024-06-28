using Services;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioService : MonoBehaviour, IService
{
    public static AudioService instance;
    public static AudioSource AudioSource => instance?.Source;
    public AudioSource Source;

    public AudioClip sfx_victory;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        ServiceLocator.RegisterAsService(this);
        Source = GetComponent<AudioSource>();
    }
}
