using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using TMPro;
using System;

public class ObjectiveEntity : Entity, ITextTrigger
{
    public TextMeshProUGUI TextMeshPro
    {
        get => _gui;
    }
    public TextAsset InkJSON
    {
        get => _inkJSON;
    }

    public List<ITextFunction> TextCompleteOrders
    {
        get => _textFunctions;
    }

    private TextMeshProUGUI _gui;
    private TextAsset _inkJSON;
    private List<ITextFunction> _textFunctions;
    [ReadOnly] [HideInInspector] public string textAssetFile;
    [ReadOnly] public string objective;
    [ReadOnly] public string instructionsWhenCompleted;

    private Tutorial _tutorial;

    public void StartText()
    {
        TextAsset text = Resources.Load<TextAsset>($"Dialogue/{textAssetFile}");
        if (!text)
        {
            Debug.LogError($"Objective entity could not find text asset at Dialogue/{textAssetFile}");
        }
        _inkJSON = text;
        TutorialSceneData sceneData = TutorialManager.GetTutorialSceneData();
        _tutorial = new Tutorial(sceneData.GUI, _inkJSON, this as MonoBehaviour, sceneData.ContinueObject, sceneData.TutorialObject, sceneData.Animator);
        _tutorial.OnStoryEndAnimationFinished += OnTextFinished;
        StartCoroutine(_tutorial.StartStory());
    }

    private void OnTextFinished(object sender, EventArgs e)
    {
        Destroy(gameObject);
        //  Do objective complete animation stuff.
    }

    private void Update()
    {
        if (_tutorial != null)
        {
            _tutorial.Update();
        }
    }

    public virtual bool CanInteract()
    {
        return true;
    }

    public virtual IEnumerator ObjectiveComplete(PlayerEntity player)
    {
        LevelGenerator.instance.listOfObjectives.Remove(this);
        PhaseManager.instance.objectiveButton.gameObject.SetActive(false);

        currentTile.myEntity = null;
        player.adjacentObjective = null;

        if (textAssetFile != null)
        {
            _inkJSON = Resources.Load<TextAsset>("Dialogue/" + textAssetFile);
        }

        StartText();
       

        string[] pointList = instructionsWhenCompleted.Split('|');
        foreach (string nextInstruction in pointList)
        {
            switch (nextInstruction)
            {
                case "ALLDRAW":
                    foreach (PlayerEntity nextPlayer in LevelGenerator.instance.listOfPlayers)
                        nextPlayer.PlusCards(2);
                    break;
            }
        }

        LevelGenerator.instance.listOfObjectives.Remove(this);
        PhaseManager.instance.objectiveButton.gameObject.SetActive(false);
        currentTile.myEntity = null;
        player.adjacentObjective = null;

        yield return null;
    }
}