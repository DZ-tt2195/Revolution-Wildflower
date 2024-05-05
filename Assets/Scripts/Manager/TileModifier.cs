using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class TileModifier : MonoBehaviour
{
    [Tooltip("Store this modifier's instructions")][ReadOnly] public Card card;
    [Tooltip("Animator component")] public Animator animator;
    [Tooltip("Sprite renderer component")] public SpriteRenderer spriteRenderer;

    public IEnumerator ResolveList(Entity entity)
    {
        string divide = card.data.enviroaction.Replace(" ", "");
        divide = divide.ToUpper().Trim();
        string[] methodsInStrings = divide.Split('/');

            foreach (string nextMethod in methodsInStrings)
            {
                LevelGenerator.instance.DisableAllTiles();
                LevelGenerator.instance.DisableAllCards();

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
                entity.GetComponent<MovingEntity>().movementLeft = -1;
                break;
            default:
                Debug.LogError($"{methodName} isn't a method");
                yield return null;
                break;
        }
    }

}
