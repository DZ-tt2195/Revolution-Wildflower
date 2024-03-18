using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Input = UnityEngine.Input;

public class MoveCamera : MonoBehaviour
{
    [Header("Dragging")]
    [SerializeField]
    private static float dragSpeed = 500;
    private static float minDragSpeed;
    private static float maxDragSpeed;

    private static List<Camera> cameras = new();
    Vector2 mousePositionOnClick;

    private static float leftLimit;
    private static float rightLimit;
    private static float upperLimit;
    private static float lowerLimit;

    [Header("Zooming")]
    [SerializeField]
    private static float zoomSpeed = 500;
    [SerializeField]
    private static float zoomMin = 15;
    [SerializeField]
    private static float zoomMax = 50;

    private static bool focused = false;
    private static float focusDefualtZoom;
    private static Vector3 startingPosition;

    private static Entity focusedEntity = null;
    private static Vector3 focusedPosition = Vector3.zero;
    private static float focusTime = 0.5f;
    private static float focusZoom;

    private static List<string> locks = new();

    [Header("Shaking")]
    [SerializeField] private AnimationCurve shakeCurve;
    [SerializeField] private float defaultShakeDuration = 1f;
    private bool restartShake = false;
    private bool shaking = false;

    public static MoveCamera instance;

    private Camera camera;

    public static Action OnFocusComplete;

    private void Start()
    {
        cameras.Clear();
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
            Vector3 newPosition = new Vector3();
            if (instance.camera.orthographicSize > zoomMin && instance.camera.orthographicSize < zoomMax)
            {
                if (scrollInput > 0)
                {
                    newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }

                else
                {
                    newPosition = new Vector3(NewManager.instance.listOfTiles.GetLength(0) * -2, 0, NewManager.instance.listOfTiles.GetLength(1) * -2);
                }

                instance.transform.position = Vector3.Lerp(instance.transform.position, new Vector3(newPosition.x, 0, newPosition.z), Time.deltaTime * 3);
            }

            foreach (Camera camera in cameras)
            {
                camera.orthographicSize = Mathf.Clamp(camera.orthographicSize + (-scrollInput * zoomSpeed * Time.deltaTime), zoomMin, zoomMax);
            }
        }
    }

    private static IEnumerator UpdateFocusedCamera()
    {
        float currentFrame = 0;
        if (focusedEntity != null)
        {
            focusedPosition = focusedEntity.transform.position;
        }
        Vector3 startPosition = instance.transform.position;

        while (currentFrame < focusTime)
        {
            Vector3 target = new Vector3(focusedPosition.x, instance.transform.position.y, focusedPosition.z);
            instance.transform.position = Vector3.Lerp(startPosition, target, (currentFrame / focusTime));
            if (instance.transform.position == target)
            {
                yield return null;
            }
            currentFrame += Time.deltaTime;
            //Debug.Log(instance.transform.position + " " + target);
            yield return null;
        }
        focused = false;
        focusedEntity = null;
        focusedPosition = Vector3.zero;

        Unfocus(false);
        OnFocusComplete?.Invoke();
    }

    public static void Focus(Entity entity, float targetZoom = -1)
    {
        focusedEntity = entity;
        startingPosition = instance.transform.position;
        if (focused)
        {
            instance.StopCoroutine(UpdateFocusedCamera());
        }

        focused = true; 
        instance.StartCoroutine(UpdateFocusedCamera());
    }

    public static void Focus(Vector3 position, float targetZoom = -1)
    {
        focusedPosition = position;
        startingPosition = instance.transform.position;
        if (focused)
        {
            instance.StopCoroutine(UpdateFocusedCamera());
        }

        focused = true;
        instance.StartCoroutine(UpdateFocusedCamera());
    }

    public static void Unfocus(bool returnToCenter = false)
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

    public static void ClearLocks()
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

    //code shamelessly stolen from Noah and uh that youtube video
    public void Shake(float duration = -1f)
    {
        if (shaking)
        {
            restartShake = true;
        }

        if (PlayerPrefs.GetInt("Screen Shake") == 1)
        {
            if (duration != -1)
            {
                StartCoroutine(ShakeCoroutine(duration));
            }
            else
            {
                StartCoroutine(ShakeCoroutine(defaultShakeDuration));
            }
        }
    }
    private IEnumerator ShakeCoroutine(float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        shaking = true;

        while (elapsedTime < duration)
        {
            if (restartShake)
            {
                elapsedTime = 0;
                restartShake = false;
            }

            elapsedTime += Time.deltaTime;
            float strength = shakeCurve.Evaluate(elapsedTime / duration);
            transform.position = startPosition + UnityEngine.Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.position = startPosition;
        shaking = false;
    }
}
