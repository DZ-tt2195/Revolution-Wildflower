using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeywordLinkHover : MonoBehaviour
{
    TMP_Text myText;
    RectTransform rectTrans;
    Camera _cameraToUse;

    private void Awake()
    {
        myText = GetComponent<TMP_Text>();
        rectTrans = GetComponent<RectTransform>();
        Canvas _canvasToCheck = GetComponentInParent<Canvas>();
        if (_canvasToCheck.renderMode == RenderMode.ScreenSpaceOverlay)
            _cameraToUse = null;
        else
            _cameraToUse = _canvasToCheck.worldCamera;
    }

    private void Update()
    {
        this.transform.SetAsLastSibling();
        Vector3 mousePosition = new(Input.mousePosition.x, Input.mousePosition.y, 0);

        if (TMP_TextUtilities.IsIntersectingRectTransform(rectTrans, mousePosition, _cameraToUse))
        {
            int intersectingLink = TMP_TextUtilities.FindIntersectingLink(myText, mousePosition, _cameraToUse);
            try { KeywordTooltip.instance.ActivateTextBox(myText.textInfo.linkInfo[intersectingLink].GetLinkID(), mousePosition, _cameraToUse==null); }
            catch (IndexOutOfRangeException) { }
        }
    }
}
