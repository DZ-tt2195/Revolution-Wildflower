using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThoughtNotification : MonoBehaviour
{
    float destination;
    [SerializeField] float offset = 0.5f;
    [SerializeField] float lerpRatio = 0.2f;
    float nextPosition = 0;
    [SerializeField] float margin = 0.01f;

    void Start()
    {
        destination = transform.position.y + offset;
    }

    private void FixedUpdate()
    {
        nextPosition = Mathf.Lerp(transform.position.y, destination, lerpRatio);
        transform.position = new Vector3(transform.position.x, nextPosition, transform.position.z);
        if (offset - transform.position.y < margin)
        {
            Destroy(this.gameObject);
        }
    }
}
