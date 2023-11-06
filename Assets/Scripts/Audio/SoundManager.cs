using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AK.Wwise.Event playing { get; private set; }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        playing = null;
    }

    public void PlaySound(AK.Wwise.Event sound, GameObject target)
    {
        if (playing == null || playing != sound)
        {
            sound.Post(target);
            playing = sound;
        }
    }
}
