using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeywordLinkHover : MonoBehaviour
{
    TMP_Text myText;
    RectTransform rectTrans;

    private void Awake()
    {
        myText = GetComponent<TMP_Text>();
        rectTrans = GetComponent<RectTransform>();
    }

    private void Update()
    {
        this.transform.SetAsLastSibling();
        Vector3 mousePosition = new(Input.mousePosition.x, Input.mousePosition.y, 0);

        if (TMP_TextUtilities.IsIntersectingRectTransform(rectTrans, mousePosition, null))
        {
            int intersectingLink = TMP_TextUtilities.FindIntersectingLink(myText, mousePosition, null);
            try{KeywordTooltip.instance.ActivateTextBox(myText.textInfo.linkInfo[intersectingLink].GetLinkID(), mousePosition);}
            catch (IndexOutOfRangeException){}
        }
    }
}
