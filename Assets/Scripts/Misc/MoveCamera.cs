using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] float scrollSpeed;
    [SerializeField] float movementSpeed;

    Camera currentCamera;

    private void Awake()
    {
        currentCamera = Camera.main;
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            NewManager.instance.FocusOnPlayer();
        }*/
        
        /*if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(Input.mousePosition);
        }*/

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            this.transform.position = new Vector3(this.transform.position.x - movementSpeed * Time.deltaTime, this.transform.position.y, this.transform.position.z - movementSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            this.transform.position = new Vector3(this.transform.position.x + movementSpeed * Time.deltaTime, this.transform.position.y, this.transform.position.z + movementSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            this.transform.position = new Vector3(this.transform.position.x + movementSpeed * Time.deltaTime, this.transform.position.y, this.transform.position.z - movementSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            this.transform.position = new Vector3(this.transform.position.x - movementSpeed * Time.deltaTime, this.transform.position.y, this.transform.position.z + movementSpeed * Time.deltaTime);
        }

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput > 0.0f && currentCamera.orthographicSize > 5) //scrolled up
        {
            currentCamera.orthographicSize -= scrollSpeed * Time.deltaTime; //zoom in
        }
        else if (scrollInput < 0.0f && currentCamera.orthographicSize < 50) // scrolled down
        {
            currentCamera.orthographicSize += scrollSpeed * Time.deltaTime; //zoom out
        }
    }
}
