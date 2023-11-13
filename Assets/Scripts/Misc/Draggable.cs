using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour
{
    Canvas canvas;

    private void Awake()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
    }

    public void DragHangler(BaseEventData data)
    {
        PointerEventData pointer = (PointerEventData)data;
        RectTransformUtility.ScreenPointToLocalPointInRectangle
        ((RectTransform)canvas.transform, pointer.position, canvas.worldCamera, out Vector2 position);

        transform.position = canvas.transform.TransformPoint(position);
    }

    private void Update()
    {
        if (this.transform.localPosition.x < -700)
            this.transform.localPosition = new Vector3(-700, transform.localPosition.y, 0);
        else if (this.transform.localPosition.x > 700)
            this.transform.localPosition = new Vector3(700, transform.localPosition.y, 0);

        if (this.transform.localPosition.y < -400)
            this.transform.localPosition = new Vector3(transform.localPosition.x, -400, 0);
        else if (this.transform.localPosition.y > 400)
            this.transform.localPosition = new Vector3(transform.localPosition.x, 400, 0);
    }
}
