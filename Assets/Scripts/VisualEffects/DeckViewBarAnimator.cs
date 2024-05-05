using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckViewBarAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
     
    private Animator animator;
 
    private void Start()
    {
        animator = GetComponent<Animator>();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetTrigger("OnMouseEnter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetTrigger("OnMouseExit");
    }
}
