using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using TMPro;
using MyBox;
using UnityEngine.EventSystems;
using System.Linq;

public class DownloadSheet : MonoBehaviour
{
    private const string apiKey = "AIzaSyCl_GqHd1-WROqf7i2YddE3zH6vSv3sNTA";
    private const string baseUrl = "https://sheets.googleapis.com/v4/spreadsheets/";
    public static DownloadSheet instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    IEnumerator DL(string ID, string range)
    {
        string url = $"{baseUrl}{ID}/values/{range}?key={apiKey}";
        using UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(range);
            Debug.LogError($"Error: {www.error}");
        }
        else
        {
            string filePath = $"Assets/Resources/{range}.txt";
            File.WriteAllText($"{filePath}", www.downloadHandler.text);
            Debug.Log($"downloaded {range} from the internet");

            string[] allLines = File.ReadAllLines($"{filePath}");
            List<string> modifiedLines = allLines.ToList();
            modifiedLines.RemoveRange(1, 3);
            File.WriteAllLines($"{filePath}", modifiedLines.ToArray());
        }
    }

    public IEnumerator DownloadCardSheet(string range)
    {
        string sheetId = "1WOtiA9BUTAuzReClZ7Aw55ATTn3Oh0VWeV39qNKO938";
        yield return DL(sheetId, range);
    }

    public IEnumerator DownloadLevelSheet(string range)
    {
        string sheetId = "1tBB7LeXet8Y7saEnDK61u8DFO_jGxHhCBdm_57cYOGY";
        yield return DL(sheetId, range);
    }
}