using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class ObjectiveEntity : Entity
{
    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;
    [ReadOnly] public string objective;

    public virtual bool CanInteract()
    {
        return true;
    }

    public virtual IEnumerator ObjectiveComplete(PlayerEntity player)
    {
        NewManager.instance.listOfObjectives.Remove(this);
        NewManager.instance.objectiveButton.gameObject.SetActive(false);

        if (!DialogueManager.GetInstance().dialogueIsPlaying)
        {
             DialogueManager.GetInstance().dialogueVariables.globalVariablesStory.variablesState["current_player"] = player.name;
             DialogueManager.GetInstance().dialogueVariables.globalVariablesStory.variablesState["current_objective"] = objective;
             DialogueManager.GetInstance().EnterDialogueMode(inkJSON);
        }
        
        NewManager.instance.listOfObjectives.Remove(this);
        NewManager.instance.objectiveButton.gameObject.SetActive(false);
        currentTile.myEntity = null;
        player.adjacentObjective = null;

        Destroy(this.gameObject);
        yield return null;
    }
}

