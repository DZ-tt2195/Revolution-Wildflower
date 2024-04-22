using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    [SerializeField] private AnimationCurve shakeCurve;
    [SerializeField] private float defaultShakeDuration = 1f;
    private bool restartShake = false;
    private bool shaking = false;

    private float currentValue = 0;
    private float maxValue = 1;
    private float valueChangeTime = 0.2f;
    private bool sliding;

    
    [SerializeField] private Image barImage;
    [SerializeField] private GameObject segments;
    [SerializeField] private Image previewObject;
    public Material segmentMaterial;

    [SerializeField] private Color previewGainColor;
    [SerializeField] private Color previewLossColor;

    private void Update()
    {
        if (previewObject.IsActive())
        {
            previewObject.SetAlpha(LevelUIManager.instance.opacity);
        }
    }

    public void SetMaximumValue(float maxValue)
    {
        this.maxValue = maxValue;
        SetSegments();
    }

    private void SetSegments()
    {
        //segmentMaterial = new Material(segmentMaterial);
        segmentMaterial.SetFloat("_Frequency", maxValue);
    }

    public void ChangeValue(float value)
    {
        StartCoroutine(SlideValue(Mathf.Clamp(currentValue + value, 0, maxValue)));
    }

    public void SetValue(float value)
    {
        StartCoroutine(SlideValue(Mathf.Clamp(value, 0, maxValue)));
    }

    private IEnumerator SlideValue(float newValue)
    {
        float startValue = currentValue;
        float elapsedTime = 0f;
        sliding = true;

        while (elapsedTime < valueChangeTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpValue = Mathf.Lerp(startValue, newValue, elapsedTime / valueChangeTime);
            barImage.fillAmount = lerpValue / maxValue;
            yield return null;
        }

        currentValue = newValue;
        sliding = false;
    }

    public void Preview(float change)
    {
        previewObject.gameObject.SetActive(true);
        float difference = currentValue + change;
        if (difference < currentValue)
        {
            previewObject.color = previewLossColor;
            float amount = barImage.rectTransform.rect.width * Mathf.Abs(change / maxValue);
            //float position = barImage.rectTransform.rect.width - (barImage.rectTransform.rect.width * ((difference - 1) / maxValue));
            float position = barImage.rectTransform.rect.width - (barImage.rectTransform.rect.width * Mathf.Abs((maxValue - currentValue) / maxValue)) - amount;

            previewObject.rectTransform.SetWidth(amount);
            previewObject.rectTransform.anchoredPosition = new Vector2(position, previewObject.rectTransform.anchoredPosition.y);
        }   
    }

    public void StopPreview()
    {
        previewObject.gameObject.SetActive(false);
        previewObject.color = Color.white;
    }


    public void Shake(float duration = -1f)
    {
        //  If you call the shake function while the shake is already happening, it just restarts the current shake from the beginning.
        //  Otherwise, the new shake's transform.position would be based off the offset position from the last one. 
        if (shaking)
        {
            restartShake = true;
        }

        if (duration != -1)
        {
            StartCoroutine(ShakeCoroutine(duration));
        }
        else
        {
            StartCoroutine(ShakeCoroutine(defaultShakeDuration));
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

            //elapsedTime += Time.deltaTime;
            //float strength = shakeCurve.Evaluate(elapsedTime / duration);
            //transform.position = startPosition + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.position = startPosition;
        shaking = false;
    }
}
