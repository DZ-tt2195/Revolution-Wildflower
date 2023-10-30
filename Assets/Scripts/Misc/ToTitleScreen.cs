using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToTitleScreen : MonoBehaviour
{
    void Awake()
    {
        if (SaveManager.instance == null)
            SceneManager.LoadScene(0);
    }
}
