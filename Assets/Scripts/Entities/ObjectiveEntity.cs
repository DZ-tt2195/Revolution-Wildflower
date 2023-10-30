using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveEntity : Entity
{
    public virtual bool CanInteract()
    {
        return true;
    }

    public virtual IEnumerator ObjectiveComplete()
    {
        NewManager.instance.listOfObjectives.Remove(this);
        NewManager.instance.objectiveButton.gameObject.SetActive(false);
        Destroy(this.gameObject);
        yield return null;
    }
}
