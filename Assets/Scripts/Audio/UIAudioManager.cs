using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public void PassUIEventToSoundManager(AudioClip sound)
    {
        SoundManager.instance.PlaySound(sound);
    }
}
