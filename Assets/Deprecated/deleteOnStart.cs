using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deleteOnStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject);   
    }
}
