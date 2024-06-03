using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script should be attached to each scene transition GameObject (the one with the Animator).
 * Animation events can only run functions in scripts attached to the same object as the Animator, so we can't run the SceneTransitionManager stuff on its own.
 * This class just contains wrapper functions we can run instead. 
 */
public class SceneTransitionAnimatorHelper : MonoBehaviour
{
    public void OnTransitionOut()
    {
        SceneTransitionManager.AnimatorOnTransitionOut();
    }

    public void OnTransitionIn()
    {
        SceneTransitionManager.AnimatorOnTransitionIn();
    }
}
