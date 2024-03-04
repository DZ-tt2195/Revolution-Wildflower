using OpenCover.Framework.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class InstructionsManager : MonoBehaviour
{
    private static InstructionsManager instance;

    [SerializeField] private GameObject arrow;
    private List<GameObject> activeArrows;
    private List<Instruction> instructions = new();
    private bool active = false;
    [SerializeField] private Color completedInstructionColor;

    [SerializeField] private GameObject instructionsObject;
    private TMP_Text instructionsText;

    private void Awake()
    {
        instance = this;
        Instruction.Manager = this;
        instructionsText = instructionsObject.GetComponentInChildren<TMP_Text>();
    }

    public static void PointTowards(Transform transform, Vector2 arrowDirection, float distanceFromOrigin)
    {
        GameObject newArrow = Instantiate(instance.arrow, transform.position, Quaternion.identity); 
        newArrow.transform.up = arrowDirection;
    }

    public static void UpdateInstructions(object obj, string[] events, string[] texts)
    {
        if (instance.active)
        {
            Debug.LogWarning($"Attempted to add instructions: {string.Concat(texts)} while instructions were already active.");
            return;
        }


        for (var i = 0; i < events.Length; i++)
        {
            Instruction instruction = new Instruction(testHandler, i, texts[i]);
            instruction.Setup(obj, events[i], texts[i]);
            instance.instructions.Add(instruction);
        }

        if (!instance.active)
        {
            instance.active = true;
            instance.instructionsObject.SetActive(true);
        }

        instance.UpdateText();
    }

    public static EventHandler testHandler;

    protected void UpdateText()
    {
        string textToPrint = "";
        for (var i = 0; i < instructions.Count; i++)
        {
            string instructionText = instructions[i].Text;
            if (instructions[i].Completed)
            {
                instructionText = $"<color=#{completedInstructionColor.ToHexString()}>{instructionText}</color>";
            }
            textToPrint += instructionText;
        }

        instructionsText.text = textToPrint;
    }

    public class Instruction
    {
        public EventHandler Event;
        public int Index;
        public string Text;
        public bool Completed;

        private object obj;
        private Delegate handler;
        private EventInfo eventInfo; 

        public static InstructionsManager Manager; 

        private void CompleteInstruction(object sender, EventArgs e)
        {
            Manager.instructions.Find(x => x == this).Completed = true;
            Manager.UpdateText();
            eventInfo.RemoveEventHandler(obj, handler);

            if (Manager.instructions.Count > 1)
            {
                Manager.UpdateText();
            }

            for (var i = 0; i < Manager.instructions.Count; i++)
            {
                if (Manager.instructions[i].Completed == false)
                {
                    return; 
                }
            }

            Manager.instructions.Clear();
            Manager.instructionsObject.SetActive(false);
            instance.active = false;

        }

        public void Setup(object obj, string thisEvent, string text)
        {
            MethodInfo method = GetType().GetMethod("CompleteInstruction", BindingFlags.NonPublic | BindingFlags.Instance);

            EventInfo eventInfo = obj.GetType().GetEvent(thisEvent);
            Type type = eventInfo.EventHandlerType;
            Delegate handler = Delegate.CreateDelegate(type, this, method);

            eventInfo.AddEventHandler(obj, handler);
            this.obj = obj;
            this.handler = handler;
            this.eventInfo = eventInfo;
        }

        public Instruction(EventHandler Event, int index, string text = "TEXT NOT SET")
        {
            this.Event = Event;
            this.Index = index;
            this.Text = text;

            this.Completed = false;
        }
    }


}
