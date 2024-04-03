using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MyBox;
using UnityEngine.UI;

[Serializable]
public class SaveData
{
    public bool freshFile;
    public int currentLevel = 0;

    public SaveData()
    {
    }
}

public class SaveManager : MonoBehaviour
{

#region Variables

    public static SaveManager instance;
    [ReadOnly] public Canvas canvas;
    public SaveData currentSaveData;
    [ReadOnly] public string saveFileName;
    [Tooltip("Card prefab")][SerializeField] Card cardPrefab;

    [SerializeField] Image transitionImage;
    [SerializeField] float transitionTime;

    [Tooltip("Put names of the card TSVs in here")] public List<string> playerDecks;
    [Tooltip("Put names of the level TSVs (in order)")] public List<string> levelSheets;
    [ReadOnly] public List<Card> allCards = new List<Card>();
    public float cardBaseHeight = -481;

#endregion

#region Setup

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            this.transform.GetChild(0).gameObject.SetActive(true);
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        #if UNITY_EDITOR
        foreach (string deck in playerDecks)
        {
            StartCoroutine(DownloadSheet.instance.DownloadCardSheet(deck));
        }

        foreach (string level in levelSheets)
        {
            StartCoroutine(DownloadSheet.instance.DownloadLevelSheet(level));
        }
        #endif
    }

    #endregion

#region Files

    public void LoadFile(string fileName)
    {
        string path = $"{Application.persistentDataPath}/{fileName}.es3";
        currentSaveData = ES3.Load<SaveData>("saveData", path);
        currentSaveData.freshFile = false;
        saveFileName = fileName;
        Debug.Log($"file loaded: {fileName}.es3");
    }

    public void NewFile(string fileName)
    {
        currentSaveData = new SaveData();
        ES3.Save("saveData", currentSaveData, $"{Application.persistentDataPath}/{fileName}.es3");
        currentSaveData.freshFile = true;
        saveFileName = fileName;
        Debug.Log($"file loaded: {fileName}.es3");
    }

    public void DeleteData(string fileName)
    {
        ES3.DeleteFile($"{Application.persistentDataPath}/{fileName}.es3");
    }

    #endregion

#region Scenes

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public List<Card> GenerateCards(string deck)
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        List<Card> characterCards = new();
        List<CardData> data = CardDataLoader.ReadCardData(deck);

        for (int i = 0; i < data.Count; i++)
        {
            for (int j = 0; j < data[i].maxInv; j++)
            {
                Card nextCopy = Instantiate(cardPrefab, canvas.transform);
                nextCopy.name = $"{data[i].name}";
                nextCopy.transform.localPosition = new Vector3(10000, cardBaseHeight);
                nextCopy.CardSetup(data[i]);
                characterCards.Add(nextCopy);
                allCards.Add(nextCopy);
            }
        }

        return characterCards;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        StartCoroutine(BringBackObjects());
    }

    IEnumerator BringBackObjects()
    {
        yield return SceneTransitionEffect(1);
        transitionImage.gameObject.SetActive(false);
    }

    public IEnumerator UnloadObjects(string nextScene)
    {
        yield return SceneTransitionEffect(0);
        allCards.Clear();
        SceneManager.LoadScene(nextScene);
    }

    IEnumerator SceneTransitionEffect(float begin)
    {
        transitionImage.gameObject.SetActive(true);
        transitionImage.SetAlpha(begin);

        float waitTime = 0f;
        while (waitTime < transitionTime)
        {
            transitionImage.SetAlpha(Mathf.Abs(begin - (waitTime / transitionTime)));
            waitTime += Time.deltaTime;
            yield return null;
        }

        transitionImage.SetAlpha(Mathf.Abs(begin - 1));
        transitionImage.gameObject.SetActive(true);
    }

    void Preserve(GameObject next)
    {
        next.transform.SetParent(null);
        DontDestroyOnLoad(next);
    }

#endregion

}