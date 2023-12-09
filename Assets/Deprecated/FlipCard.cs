using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using System;
using System.Linq;

public class FlipCard : MonoBehaviour
{
    Image image;
    [SerializeField] CanvasGroup cGroup;
    [SerializeField] Sprite mainImage;
    [SerializeField] Sprite backImage;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.sprite = backImage;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(CardAnimation());
    }

    IEnumerator CardAnimation()
    {
        image.sprite = backImage;
        cGroup.alpha = 0;
        transform.localEulerAngles = new Vector3(0, 0, 0);

        float elapsedTime = 0f;
        float totalTime = 0.35f;

        Vector3 originalRot = this.transform.localEulerAngles;
        Vector3 newRot = new Vector3(0, 90, 0);

        while (elapsedTime < totalTime)
        {
            this.transform.localEulerAngles = Vector3.Lerp(originalRot, newRot, elapsedTime / totalTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.sprite = mainImage;
        cGroup.alpha = 1;
        elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            this.transform.localEulerAngles = Vector3.Lerp(newRot, originalRot, elapsedTime / totalTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.transform.localEulerAngles = originalRot;

    }
}
