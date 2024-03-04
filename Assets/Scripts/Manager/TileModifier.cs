using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class TileModifier : MonoBehaviour
{
    [Tooltip("Store this modifier's instructions")][ReadOnly] public Card card;

    public IEnumerator ResolveList(Entity entity)
    {
        string divide = card.enviroEffect.Replace(" ", "");
        divide = divide.ToUpper().Trim();
        string[] methodsInStrings = divide.Split('/');

            foreach (string nextMethod in methodsInStrings)
            {
                NewManager.instance.DisableAllTiles();
                NewManager.instance.DisableAllCards();

                if (nextMethod == "" || nextMethod == "NONE")
                {
                    continue;
                }
                else
                {
                    yield return ResolveMethod(entity, nextMethod);
                }
            }
    }

    IEnumerator ResolveMethod(Entity entity, string methodName)
    {
        methodName = methodName.Replace("]", "").Trim();

        switch (methodName)
        {
            case "SELFDESTRUCT":
                Destroy(this);
                break;
            case "ZEROMOVEMENT":
                yield return ZeroMovement(entity);
                break;
            default:
                Debug.LogError($"{methodName} isn't a method");
                yield return null;
                break;
        }
    }

    IEnumerator ZeroMovement(Entity entity)
    {
        entity.GetComponent<MovingEntity>().movementLeft = -1;
        Debug.Log($"hit {entity.name}");
        yield return null;
    }
}
