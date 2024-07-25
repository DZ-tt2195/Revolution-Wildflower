using MyBox;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;


[Serializable]
public sealed class TextFunctionMoveScene : ITextFunction
{
    [Scene, SerializeField] private string _newScene;
    public void OnTextComplete(ITextAdvancer advancer)
    {
        SceneTransitionManager.Transition("AlphaFade", _newScene);
    }
}

