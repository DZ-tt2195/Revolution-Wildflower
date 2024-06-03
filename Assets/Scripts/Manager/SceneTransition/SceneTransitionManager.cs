using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

/*  HOW THIS WORKS:
 *  
 *  1) Create an empty child object inside the Canvas attached to this GameObject. Each child represents a specific scene transition animation. 
 *  2) Attach an Animator to this child, as well as a SceneTransitionAnimatorHelper class. 
 *  3) Make two animations: an "Out" animation for fading out, and an "In" animation for fading back into the game screen.
 *  4) On the final frame of both of these animations, add an Animation Event that calls SceneTransitionAnimatorHelper.OnTransitionOut and SceneTransitionAnimatorHelper.OnTransitionIn respectively.
 *  5) That's it! You can call your transition via the name of the object with the Animator. If you're confused on how the hierarchy should look, refer to the AlphaFade example. 
 * 
 */

public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager instance;


    /// <summary>
    /// This event runs when the transition animation reaches its "peak."
    /// </summary>
    public static event Action OnTransitionOutCompleted;

    /// <summary>
    /// This event runs when the transition animation completely finishes. If your class has things it wants to run when a transition finishes, subsrcribe to this event. 
    /// </summary>
    public static event Action OnTransitionInCompleted;

    [SerializeField] private Canvas canvas;

    private string newSceneName;
    private Transform targetTransitionTransform;
    private Animator targetAnimator;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        canvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Starts a transition animation to another scene of your choice. 
    /// </summary>
    /// <param name="transitionName"></param>
    /// <param name="newSceneName"></param>
    public static void Transition(string transitionName, string newSceneName)
    {
        Debug.Log($"Transitinoning... {transitionName}, {newSceneName}");
        instance.canvas.gameObject.SetActive(true);
        instance.newSceneName = newSceneName;

        instance.targetTransitionTransform = instance.canvas.transform.Find(transitionName);
        instance.targetAnimator = instance.targetTransitionTransform.GetComponent<Animator>();

        instance.targetAnimator.SetTrigger("Out");
        OnTransitionOutCompleted += instance.StartLoadingScene;
        OnTransitionInCompleted += instance.FinishLoadingScene;
    }

    /// <summary>
    /// This function should ONLY be called by SceneTransitionAnimatorHelper. 
    /// </summary>
    public static void AnimatorOnTransitionOut()
    {
        OnTransitionOutCompleted?.Invoke();
    }


    /// <summary>
    /// This function should ONLY be called by SceneTransitionAnimatorHelper. 
    /// </summary>
    public static void AnimatorOnTransitionIn()
    {
        OnTransitionInCompleted?.Invoke();
    }

    private void StartLoadingScene()
    {
        OnTransitionOutCompleted -= StartLoadingScene;
        StartCoroutine(LoadSceneAsync(newSceneName)); 
    }

    private void FinishLoadingScene()
    {
        OnTransitionInCompleted -= FinishLoadingScene;
        instance.canvas.gameObject.SetActive(false);

    }

    private IEnumerator LoadSceneAsync(string newSceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(newSceneName);

        while (!asyncLoad.isDone)
        {
            Debug.Log(asyncLoad.progress);
            yield return null; 
        }

        instance.targetAnimator.SetTrigger("In");
    }
}
