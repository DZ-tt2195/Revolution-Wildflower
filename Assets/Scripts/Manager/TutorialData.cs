using System;
using System.Reflection;
using UnityEngine;

[Obsolete]
public class TutorialData
{
    public EventHandler eventHandler; 
    public TextAsset InkJSON;
    public string eventClass;
    public string eventName;
    public LevelStartDialogueVariable[] dialogueVariables;

    private object obj;
    private EventInfo eventInfo;
    private Delegate handler; 


    public void Load(object sender, EventArgs e)
    {
        /*Debug.Log("TUTORIAL WORKS");
        if (dialogueAsset)
        {
            DialogueManager.GetInstance().StartStory(dialogueAsset);
            foreach (LevelStartDialogueVariable dialogueVariable in dialogueVariables)
            {
                //  Though we store the values as strings in the ScriptableObject, we can convert them to ints as necessary. 
                float numValue;
                if (float.TryParse(dialogueVariable.value, out numValue))
                {
                    DialogueManager.dialogueVariables.globalVariablesStory.variablesState[dialogueVariable.name] = numValue;
                }

                else
                {
                    DialogueManager.dialogueVariables.globalVariablesStory.variablesState[dialogueVariable.name] = dialogueVariable.value;
                }
            }
            DialogueManager.GetInstance().EnterDialogueMode();
            Debug.Log("Entering dialogue mode");
        }

        eventInfo.RemoveEventHandler(obj, handler);*/
    }

    public void Setup(string objName, string thisEvent)
    {
        Type T = Type.GetType(objName);
        MethodInfo method = GetType().GetMethod("Load", BindingFlags.Public | BindingFlags.Instance);
        EventInfo eventInfo = T.GetEvent(thisEvent, BindingFlags.Public | BindingFlags.Static);
        Type eventHandlerType = eventInfo.EventHandlerType;
        Delegate handler = Delegate.CreateDelegate(eventHandlerType, this, method);
        eventInfo.AddEventHandler(obj, handler);

        //this.obj = obj;
        this.handler = handler;
        this.eventInfo = eventInfo;
    }
}
