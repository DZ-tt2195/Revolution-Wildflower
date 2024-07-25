using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

/// <summary>
/// The <c>TutorialTrigger</c> class creates an anonymous instance of the <c>Tutorial</c> class that activates when a given event is triggered.
/// </summary>
public class TutorialTrigger : ITextTrigger
{
    private TextMeshProUGUI _gui;
    private TextAsset _textAsset;
    private List<ITextFunction> _textFunctions;
    private Tutorial _tutorial;
    private GameObject _continueObject;
    private MonoBehaviour _coroutineMono;
    private GameObject _tutorialObject;
    private Animator _animator;

    private object obj;
    private EventInfo eventInfo;
    private Delegate handler;

    public TextMeshProUGUI TextMeshPro { get => _gui; }
    public TextAsset InkJSON { get => _textAsset; }
    public List<ITextFunction> TextCompleteOrders { get => _textFunctions; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="gui">The <c>TextMeshProUGUI</c> to print text to.</param>
    /// <param name="textAsset">The <c>TextAsset</c> to read text from.</param>
    /// <param name="eventName">The name of the event you want to cause the trigger.</param>
    /// <param name="eventClass">The name of the class that holds the <c>eventName</c>.</param>
    /// <param name="sceneData">A struct containing the data for UI objects.</param>
    public TutorialTrigger(TextMeshProUGUI gui, TextAsset textAsset, string eventName, string eventClass, TutorialSceneData sceneData)
    {
        Debug.Log(eventName + " " + eventClass);
        Type T = Type.GetType(eventClass);
        MethodInfo method = GetType().GetMethod("Start", BindingFlags.Public | BindingFlags.Instance);
        EventInfo eventInfo = T.GetEvent(eventName, BindingFlags.Public | BindingFlags.Static);
        Type eventHandlerType = eventInfo.EventHandlerType;
        Delegate handler = Delegate.CreateDelegate(eventHandlerType, this, method);
        eventInfo.AddEventHandler(obj, handler);

        //this.obj = obj;
        this.handler = handler;
        this.eventInfo = eventInfo;

        _gui = gui;
        Debug.Log("Tutorial Trigger Init " + _gui);

        _textAsset = textAsset;
        _coroutineMono = sceneData.CoroutineMono;
        _continueObject =sceneData.ContinueObject;
        _tutorialObject =sceneData.TutorialObject;
        _animator = sceneData.Animator;
    }

    /// <summary> 
    /// Starts text, then removes itself from whichever event triggered it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void Start(object sender, EventArgs e)
    {
        StartText();
        eventInfo.RemoveEventHandler(obj, handler);
    }

    /// <summary>
    /// Starts the text printing process.
    /// </summary>
    public void StartText()
    {
        Debug.Log("Tutorial Trigger start text " + _gui);
        _tutorial = new Tutorial(_gui, _textAsset, _coroutineMono, _continueObject, _tutorialObject, _animator);
        _coroutineMono.StartCoroutine(_tutorial.StartStory());
    }
}