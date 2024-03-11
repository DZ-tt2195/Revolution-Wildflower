using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EntityToolTip : MonoBehaviour
{
    private RectTransform CanvasTransform;
    [SerializeField] RectTransform EntityNameTransform;
    [SerializeField] RectTransform EntityToolTipTransform;
    [SerializeField] public TMP_Text EntityName;
    [SerializeField] public TMP_Text EntityInfo;
    [SerializeField] float margin = 25;
    public bool isActive = false;

    private void Awake()
    {
        CanvasTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        CanvasTransform.SetHeight((margin * 3) + EntityNameTransform.rect.height + EntityToolTipTransform.rect.height);
        this.gameObject.SetActive(isActive);
        if (isActive)
        {
            isActive = false;
        }
        transform.position = Input.mousePosition + new Vector3(2, 2, 0);
    }
}
