using UnityEngine;

public class GridSelectionTracker : MonoBehaviour
{
    private Vector3 target;
    private float speed = 0.1f;
    public void Move(Vector3 targetPosition)
    {
        target = targetPosition;
    }

    private void Update()
    {
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed);
        }
    }
}
