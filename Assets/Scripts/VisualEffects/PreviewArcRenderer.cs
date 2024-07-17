using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PreviewArcRenderer : MonoBehaviour
{
    [SerializeField] private GameObject circles;
    [SerializeField] private GameObject endCircle;
    [SerializeField] private GameObject startCircle;
    private LineRenderer lineRenderer;
    private float height = 4;
    private float vertexCount = 20;

    private Vector3 startPosition;
    private TileData currentTile; 
    private Vector3 endPosition = new Vector3(-999, -999, -999);
    private Animator ringAnimator;
    [SerializeField] private Animator previewObjectAnimator;

    private bool tracking;

    private Camera mainCamera;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>(); 
        lineRenderer.enabled = false;
        Card.OnChoosingTile += StartTracking;
        mainCamera = Camera.main;
        circles.SetActive(false);
    }

    private void StartTracking(object sender, EventArgs e)
    {
        startPosition = PhaseManager.instance.lastSelectedPlayer.transform.position;
        startCircle.transform.position = startPosition;
        lineRenderer.enabled = true;
        tracking = true;

        if (endPosition != new Vector3(-999, -999, -999))
        {
            circles.SetActive(true);
        }

        if (PhaseManager.instance.chosenCard != null)
        {
            CardData data = PhaseManager.instance.chosenCard.data;
            RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>($"CardTileDrops/{data.name.Replace(".", "")}/{data.name} Controller");
            if (controller)
            {
                previewObjectAnimator.runtimeAnimatorController = controller;
            }

            else
            {
                Debug.LogError($"PreviewArcRenderer: Couldn't find an animator controller at CardTileDrops/{data.name.Replace(".", "")}/{data.name} Controller");
            }
        }
    }

    private void StopTracking(object sender, EventArgs e)
    {
        endCircle.SetActive(false);
    }

    private void SetPreviewObject(CardData card)
    {

    }

    private void Update()
    {
        if (tracking)
        {
            Vector3 currentEndPosition = endPosition;
            endPosition = GetClosestFloor(Physics.OverlapSphere(mainCamera.ScreenToWorldPoint(Input.mousePosition), 999f));
            if (endPosition == currentEndPosition && currentTile)
            {
                endPosition = currentTile.transform.position;
            }
            if (!currentTile)
            {
                return;
            }
            var pointList = new List<Vector3>();
            Vector3 modifiedStartPosition = startPosition;
            modifiedStartPosition.y = 0;
            Vector3 modifiedEndPosition = endPosition; 
            modifiedEndPosition.y = 0;

            
            Vector3 middlePosition = new Vector3((startPosition.x + endPosition.x) / 2, startPosition.y + height, (startPosition.z + endPosition.z) / 2);
            Debug.Log($"{startPosition}, {endPosition}, {middlePosition}");
            middlePosition.y += height;

            for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
            {
                var tangent1 = Vector3.Lerp(startPosition, middlePosition, ratio);
                var tangent2 = Vector3.Lerp(middlePosition, endPosition, ratio);
                var curve = Vector3.Lerp(tangent1, tangent2, ratio);

                pointList.Add(curve);
            }

            pointList.Add(endPosition);

            lineRenderer.positionCount = pointList.Count;
            lineRenderer.SetPositions(pointList.ToArray());
            endCircle.transform.position = endPosition;
        }
    }

    private Vector3 GetClosestFloor(Collider[] colliders)
    {
        Vector3 targetPosition = endPosition;
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, 0, mousePos.z);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("Floor"))
            {
                if (collider.gameObject.GetComponent<TileData>().moused)
                {
                    currentTile = collider.gameObject.GetComponent<TileData>();
                    Vector3 cubePosition = collider.transform.position;
                    Vector3 colliderPosition = new Vector3(cubePosition.x, cubePosition.y, cubePosition.z);
                    return colliderPosition;
                }
            }
        }

        return targetPosition;
    }
}
