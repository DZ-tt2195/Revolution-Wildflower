using DG.Tweening;
using System;
using UnityEngine;

public class GridSelectionTracker : MonoBehaviour
{
    [SerializeField] 
    private Vector3 target;
    private float moveSpeed = 0.2f;
    private float scaleSpeed = 0.1f; 
    public void Move(Vector3 targetPosition)
    {
        transform.DOMoveX(targetPosition.x, moveSpeed).SetEase(Ease.OutQuart);
        transform.DOMoveZ(targetPosition.z, moveSpeed).SetEase(Ease.OutQuart);
    }

    public void OnSelect(object sender, EventArgs e)
    {
        if (sender is GridSelection)
        {
            transform.DOScaleX(transform.localScale.x + 0.03f, scaleSpeed).SetEase(Ease.OutQuart).SetLoops(2, LoopType.Yoyo);
            transform.DOScaleY(transform.localScale.y + 0.03f, scaleSpeed).SetEase(Ease.OutQuart).SetLoops(2, LoopType.Yoyo);
        }
    }

    private void Update()
    {

    }
}
