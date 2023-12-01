using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAudioManager : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event hoverSound;
    [SerializeField] AK.Wwise.Event clickSound;

    private void Start()
    {
        Button[] sceneButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button button in sceneButtons)
        {
            button.onClick.AddListener(() => PlayButtonClick());
        }
    }
    public void PlayButtonHover()
    {
        hoverSound.Post(gameObject);
    }
    public void PlayButtonClick()
    {
        clickSound.Post(gameObject);
    }
}
