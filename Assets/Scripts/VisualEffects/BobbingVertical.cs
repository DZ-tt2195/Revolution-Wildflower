using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobbingVertical : MonoBehaviour
{
    float originalY;

    public float floatStrength;
    public float randomizer;

    void Start()
    {
        randomizer = Random.Range(0.1f, 4);
        floatStrength = Random.Range(1f, 3f);
        this.originalY = this.transform.position.y;
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x, originalY + (Mathf.Cos(Time.time) * floatStrength * randomizer), transform.position.z);
    }
}
