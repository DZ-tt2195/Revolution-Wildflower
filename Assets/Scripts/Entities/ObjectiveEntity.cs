using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using TMPro;

public class ObjectiveEntity : Entity
{
    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;
    [ReadOnly] [HideInInspector] public string textAssetFile;
    [ReadOnly] public string objective;
    [ReadOnly] public string instructionsWhenCompleted;


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
            Debug.Log("Dialogue/" + textAssetFile + ".json");
            inkJSON = Resources.Load<TextAsset>("Dialogue/" + textAssetFile);
            Debug.Log(inkJSON);
        }

        if (!DialogueManager.GetInstance().dialogueIsPlaying)
        {
            DialogueManager.GetInstance().StartStory(inkJSON);
            DialogueManager.dialogueVariables.globalVariablesStory.variablesState["current_player"] = player.name;
            DialogueManager.dialogueVariables.globalVariablesStory.variablesState["current_objective"] = objective;
            DialogueManager.GetInstance().EnterDialogueMode();
        }

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
        Destroy(this.gameObject);
    }
}