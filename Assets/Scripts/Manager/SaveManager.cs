using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.IO;
using MyBox;

[Serializable]
public class SaveData
{
    public List<List<string>> savedDecks = new List<List<string>>();

    public SaveData()
    {
        for (int i = 0; i < 3; i++)
            savedDecks.Add(new List<string>());
    }
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    Transform canvas;
    [ReadOnly] public SaveData currentSaveData;
    [ReadOnly] public string saveFileName;
    [Tooltip("Card prefab")][SerializeField] Card cardPrefab;

    [Tooltip("Put names of the TSVs in here")] public List<string> playerDecks;
    public List<List<Card>> characterCards;

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

    public void LoadFile(string fileName)
    {
        string path = $"{Application.persistentDataPath}/{fileName}.es3";
        currentSaveData = ES3.Load<SaveData>("saveData", path);
        saveFileName = fileName;
        Debug.Log($"file loaded: {fileName}.es3");
    }

    public void NewFile(string fileName)
    {
        currentSaveData = new SaveData();
        ES3.Save("saveData", currentSaveData, $"{Application.persistentDataPath}/{fileName}.es3");
        saveFileName = fileName;
        Debug.Log($"file loaded: {fileName}.es3");
    }

    public void SaveHand(List<List<Card>> deckToSave, string fileName)
    {
        List<List<string>> newCards = new List<List<string>>();
        for (int i = 0; i<deckToSave.Count; i++)
        {
            newCards.Add(new List<string>());
            foreach (Card card in deckToSave[i])
                newCards[i].Add(card.name);
        }

        currentSaveData.savedDecks = newCards;
        ES3.Save("saveData", currentSaveData, $"{Application.persistentDataPath}/{fileName}.es3");
        saveFileName = fileName;
    }

    public void DeleteData(string fileName)
    {
        ES3.DeleteFile($"{Application.persistentDataPath}/{fileName}.es3");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        canvas = GameObject.Find("Canvas").transform;
        RightClick.instance.transform.SetParent(canvas);
        RightClick.instance.transform.localPosition = new Vector3(0, 0);

        FPS.instance.transform.SetParent(canvas);
        FPS.instance.transform.localPosition = new Vector3(-850, -500);

        characterCards = new List<List<Card>>();
        for (int k = 0; k<playerDecks.Count; k++)
        {
            List<CardData> data = CardDataLoader.ReadCardData(playerDecks[k]);
            characterCards.Add(new List<Card>());

            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].maxInv; j++)
                {
                    Card nextCopy = Instantiate(cardPrefab, canvas);
                    nextCopy.name = $"{data[i].name} (P{k})";
                    nextCopy.transform.localPosition = new Vector3(10000, 10000);
                    nextCopy.CardSetup(data[i]);
                    characterCards[k].Add(nextCopy);
                }
            }
        }
    }

    public void UnloadObjects()
    {
        Preserve(RightClick.instance.gameObject);
        Preserve(FPS.instance.gameObject);
        characterCards.Clear();
    }

    void Preserve(GameObject next)
    {
        next.transform.SetParent(null);
        DontDestroyOnLoad(next);
    }
}