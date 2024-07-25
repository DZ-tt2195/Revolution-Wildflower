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

    private static LoadScene instance;

    Button button;

    private void Awake()
    {
        instance = this;
        button = GetComponent<Button>();
        button.onClick.AddListener(NextScene);
    }

    public static void NextScene()
    {
        Debug.Log("NEXTSCENE TRIGGERED, REMOVING ITSELF");
        MoveCamera.ClearLocks();
        SceneTransitionManager.Transition("AlphaFade", instance.scene);
        //DialogueManager.DialogueCompleted -= NextScene;
        //StartCoroutine(SaveManager.instance.UnloadObjects(scene));
    }
}
