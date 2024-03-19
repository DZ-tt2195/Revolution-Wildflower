using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MyBox;

public class LoadScene : MonoBehaviour
{
    [Scene]
    [SerializeField] string scene;

    Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(NextScene);
    }

    public void NextScene()
    {
        MoveCamera.ClearLocks();
        SaveManager.instance.UnloadObjects();
        SceneManager.LoadScene(scene);
    }
}
