using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    AudioSource source;

    public AudioClip playing { get; private set; }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            source = GetComponent<AudioSource>();
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

    public void PlaySound(AudioClip sound)
    {
        if (!playing || playing != sound)
        {
            source.PlayOneShot(sound);
            playing = sound;
        }
    }

    public void PlaySound(AudioClip sound, Vector3 position)
    {
        transform.position = position;
        print("playing" + sound.name);
        source.PlayOneShot(sound);
    }

    public void PlaySoundDelayed(AudioClip sound, float delay)
    {
        StartCoroutine(SoundDelayCoroutine(sound, delay));
    }

    IEnumerator SoundDelayCoroutine(AudioClip sound, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.PlayOneShot(sound);
    }
}
