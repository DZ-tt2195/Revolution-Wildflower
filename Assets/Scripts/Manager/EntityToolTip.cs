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
    [SerializeField] TMP_Text EntityName;
    [SerializeField] TMP_Text EntityInfo;
    [SerializeField] float margin = 25;
    bool isActive = false;

    private void Awake()
    {
        instance = this;
        CanvasTransform = GetComponent<RectTransform>();
    }

    public void SetInfo(string entityName, string entityInfo)
    {
        EntityName.text = entityName;
        EntityInfo.text = entityInfo;
        this.gameObject.SetActive(true);
        isActive = true;
    }

    private void Update()
    {
        CanvasTransform.SetHeight((margin * 3) + EntityNameTransform.rect.height + EntityToolTipTransform.rect.height);
        this.gameObject.SetActive(isActive);
        if (isActive)
            isActive = false;
        transform.position = Vector3.Lerp(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), 5*Time.deltaTime);
    }
}
