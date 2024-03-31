using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioService : MonoBehaviour
{
    public static AudioService instance;
    public static AudioSource AudioSource => instance?.Source;
    public AudioSource Source;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        Source = GetComponent<AudioSource>();
    }
}
