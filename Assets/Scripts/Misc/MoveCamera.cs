using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Windows;
using Input = UnityEngine.Input;

public class MoveCamera : MonoBehaviour
{
    [Header("Dragging")]
    [SerializeField]
    private float dragSpeed;

    Camera currentCamera;
    Vector2 mousePositionOnClick;

    private float leftLimit;
    private float rightLimit;
    private float upperLimit;
    private float lowerLimit;

    private float zoomSpeed = 20;
    private float zoomMin = 5;
    private float zoomMax = 50;

    private bool focused;
    private float focusDefualtZoom;

    private List<string> locks = new();

    private void Awake()
    {
        currentCamera = Camera.main;
    }

    private void Update()
    {
        if (locks.Count > 0)
        {
            return;
        }

        if (Input.GetMouseButton(0))
        {
            var input = new Vector3();
            input.x = Input.GetAxis("Mouse X") * dragSpeed * Time.deltaTime;
            input.z = Input.GetAxis("Mouse Y") * dragSpeed * Time.deltaTime;

            float horizontal = (input.x + input.z);
            float vertical = (input.x - input.z);
            

            transform.position = new Vector3(transform.position.x + vertical, transform.position.y, transform.position.z + horizontal);
        }

        CalculateScroll();

        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            NewManager.instance.FocusOnPlayer();
        }*/
        
        /*if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(Input.mousePosition);
        }*/

        /*if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
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
        }*/
    }

    private void CalculateScroll()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            Debug.Log(scrollInput * zoomSpeed);
            currentCamera.orthographicSize = Mathf.Clamp(currentCamera.orthographicSize + (scrollInput * zoomSpeed * Time.deltaTime), zoomMin, zoomMax);
        }
    }

    public void Focus(Entity entity)
    {

    }

    public void AddLock(string lockName)
    {
        if (locks.Contains(lockName))
        {
            Debug.LogWarning(GetType().Name + ": locklist already contains lock called " + lockName);
            return;
        }

        locks.Add(lockName);
    }

    public void RemoveLock(string lockName)
    {
        if (!locks.Contains(lockName))
        {
            Debug.LogWarning(GetType().Name + ": locklist does not contain lock called " + lockName);
            return;
        }

        locks.Remove(lockName);
    }

    public void ClearLocks()
    {   
        if (locks.Count == 0)
        {
            Debug.Log("Cleared lock list.");
            return; 
        }

        string log = "Cleared locks: ";
        for (var i = 0; i < locks.Count; i++)
        {
            log += locks[i];
            if (i != locks.Count - 1)
            {
                log += ", ";
            }
        }

        locks.Clear(); 
    }
}
