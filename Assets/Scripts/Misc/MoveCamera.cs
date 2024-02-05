using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using Input = UnityEngine.Input;

public class MoveCamera : MonoBehaviour
{
    [Header("Dragging")]
    [SerializeField]
    private static float dragSpeed = 300;
    private static float minDragSpeed;
    private static float maxDragSpeed;

    private static List<Camera> cameras = new();
    Vector2 mousePositionOnClick;

    private float leftLimit;
    private float rightLimit;
    private float upperLimit;
    private float lowerLimit;

    [Header("Zooming")]
    [SerializeField]
    private static float zoomSpeed = 100;
    [SerializeField]
    private static float zoomMin = 15;
    [SerializeField]
    private static float zoomMax = 50;

    private bool focused = false;
    private float focusDefualtZoom;
    private Vector3 startingPosition;

    private Entity focusedEntity = null;
    private Vector3 focusedPosition = Vector3.zero;
    private float focusTime = 15f;
    private float focusZoom;

    private static List<string> locks = new();

    private static MoveCamera instance;

    private Camera camera;

    private void Start()
    {
        instance = this;
        camera = GetComponent<Camera>();
        cameras.Add(GetComponent<Camera>());
        
        for (var i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.TryGetComponent(out Camera camera);
            if (camera != null)
            {
                cameras.Add(camera);
                Debug.Log(camera.gameObject.name);
            }
        }

        minDragSpeed = dragSpeed / 8f;
        maxDragSpeed = dragSpeed / 4f;
    }

    private void Update()
    {
        if (!focused)
        {
            UpdateFreeCamera();
        }
    }

    private static void UpdateFreeCamera()
    {
        if (locks.Count > 0)
        {
            return; 
        }

        if (Input.GetMouseButton(0))
        {
                var input = new Vector3();

                float currentDragSpeed = Mathf.Clamp(dragSpeed * (zoomMin / instance.camera.orthographicSize), minDragSpeed, maxDragSpeed);

                input.x = Input.GetAxis("Mouse X") * currentDragSpeed * Time.deltaTime;
                input.z = Input.GetAxis("Mouse Y") * currentDragSpeed * Time.deltaTime;

                //  The camera's x/z axes are aligned with the isometric grid, not actual up/down, so some we need some conversion to make it work. 
                float vertical = (input.x - input.z);
                float horizontal = (input.x + input.z);

                instance.transform.position = new Vector3(instance.transform.position.x + vertical, instance.transform.position.y, instance.transform.position.z + horizontal);
        }

        CalculateScroll();
    }

    private static void CalculateScroll()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            foreach (Camera camera in cameras)
            {
                camera.orthographicSize = Mathf.Clamp(camera.orthographicSize + (scrollInput * zoomSpeed * Time.deltaTime), zoomMin, zoomMax);
            }
        }
    }

    private IEnumerator UpdateFocusedCamera()
    {
        float currentFrame = 0;
        if (focusedEntity != null)
        {
            focusedPosition = focusedEntity.transform.position;
        }

        while (currentFrame < focusTime)
        {
            Vector3 target = new Vector3(focusedPosition.x, transform.position.y, focusedPosition.z);
            transform.position = Vector3.Lerp(transform.position, target, (currentFrame / focusTime));
            currentFrame += Time.deltaTime;
            yield return null;
        }

        focused = false;
        focusedEntity = null;
        focusedPosition = Vector3.zero;
    }

    public void Focus(Entity entity, float targetZoom = -1)
    {
        focusedEntity = entity;
        startingPosition = transform.position;
        if (focused)
        {
            StopCoroutine(UpdateFocusedCamera());
        }

        focused = true; 
        StartCoroutine(UpdateFocusedCamera());
    }

    public void Focus(Vector3 position, float targetZoom = -1)
    {
        focusedPosition = position;
        startingPosition = transform.position;
        if (focused)
        {
            StopCoroutine(UpdateFocusedCamera());
        }

        focused = true;
        StartCoroutine(UpdateFocusedCamera());
    }

    public void Unfocus(bool returnToCenter = false)
    {
        if (returnToCenter)
        {
            Focus(Vector3.zero, zoomMax/2);
        }
    }

    public static void AddLock(string lockName)
    {
        if (locks.Contains(lockName))
        {
            Debug.LogWarning(instance.GetType().Name + ": locklist already contains lock called " + lockName);
            return;
        }

        locks.Add(lockName);
    }

    public static void RemoveLock(string lockName)
    {
        if (!locks.Contains(lockName))
        {
            Debug.LogWarning(instance.GetType().Name + ": locklist does not contain lock called " + lockName);
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
