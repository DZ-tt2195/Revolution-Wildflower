using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EntityToolTip : MonoBehaviour
{
    public static EntityToolTip instance;
    private RectTransform CanvasTransform;
    [SerializeField] RectTransform EntityNameTransform;
    [SerializeField] RectTransform EntityToolTipTransform;
    [SerializeField] public TMP_Text EntityName;
    [SerializeField] public TMP_Text EntityInfo;
    [SerializeField] float margin = 25;
    public bool isActive = false;

    private void Awake()
    {
        instance = this;
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
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
