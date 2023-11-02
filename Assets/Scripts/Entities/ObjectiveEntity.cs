using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveEntity : Entity
{
    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;

    public virtual bool CanInteract()
    {
        return true;
    }

    public virtual IEnumerator ObjectiveComplete()
    {
        NewManager.instance.listOfObjectives.Remove(this);
        NewManager.instance.objectiveButton.gameObject.SetActive(false);
        Destroy(this.gameObject);
        DialogueManager.GetInstance().EnterDialogueMode(inkJSON);
        yield return null;
    }
}
