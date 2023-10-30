using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPS : MonoBehaviour
{
    public static FPS instance;
    TMP_Text fpsText;
    int lastframe = 0;
    float lastupdate = 60;
    float[] framearray = new float[60];

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            fpsText = this.transform.GetChild(0).GetComponent<TMP_Text>();
            Application.targetFrameRate = 60;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        fpsText.text = $"FPS: {CalculateFrames():F0}";
    }

    float CalculateFrames()
    {
        framearray[lastframe] = Time.deltaTime;
        lastframe = (lastframe + 1);
        if (lastframe == 60)
        {
            lastframe = 0;
            float total = 0;
            for (int i = 0; i < framearray.Length; i++)
                total += framearray[i];
            lastupdate = (float)(framearray.Length / total);
            return lastupdate;
        }
        return lastupdate;
    }
}
