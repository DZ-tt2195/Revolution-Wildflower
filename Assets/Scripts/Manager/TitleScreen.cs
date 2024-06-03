using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using MyBox;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.Text.RegularExpressions;
//using UnityEngine.Windows;

public class TitleScreen : MonoBehaviour
{
    Button deleteFile;
    Button createFile;
    Button loadFile;

    TMP_Text errorText;
    TMP_InputField newName;
    TMP_Dropdown fileChoose;

    [Scene] [SerializeField] string scene;

    private void Awake()
    {
        deleteFile = GameObject.Find("Delete Deck").GetComponent<Button>();
        deleteFile.onClick.AddListener(Deletion);

        loadFile = GameObject.Find("Load Deck").GetComponent<Button>();
        loadFile.onClick.AddListener(Loading);

        createFile = GameObject.Find("Create Deck").GetComponent<Button>();
        createFile.onClick.AddListener(Creation);

        errorText = GameObject.Find("Error Text").GetComponent<TMP_Text>();
        errorText.gameObject.SetActive(false);
        newName = GameObject.Find("Save File Input").GetComponent<TMP_InputField>();

        fileChoose = GameObject.Find("Save File Dropdown").GetComponent<TMP_Dropdown>();
        fileChoose.onValueChanged.AddListener(delegate { LoadCheck(); });
    }

    private void Start()
    {
        string[] currentFileNames = ES3.GetFiles(Application.persistentDataPath + "/Saves");
        foreach(string name in currentFileNames)
        {
            fileChoose.options.Add(new TMP_Dropdown.OptionData(name[..^4]));
            fileChoose.RefreshShownValue();
        }
    }

    IEnumerator CausedError(string newText)
    {
        errorText.gameObject.SetActive(true);
        errorText.text = newText;
        yield return new WaitForSeconds(3f);
        errorText.gameObject.SetActive(false);
    }

    void LoadCheck()
    {
        bool outdated = false;
        foreach (string saveDataPath in SaveManager.instance.playerDecks)
        {
            if (File.GetCreationTime(Path.Combine(Application.dataPath, $"Resources/{fileChoose.options[fileChoose.value].text}.txt")) > File.GetCreationTime(saveDataPath))
                outdated = true;
        }

        if (outdated)
        {
            StartCoroutine(CausedError("Your save file may have outdated cards."));
        }
        else
        {
            SaveManager.instance.LoadFile(fileChoose.options[fileChoose.value].text);
        }
    }

    void Loading()
    {
        if (fileChoose.options.Count == 0)
        {
            StartCoroutine(CausedError("You don't have any save files."));
        }
        else
        { 
            SaveManager.instance.LoadFile(fileChoose.options[fileChoose.value].text);
            SceneTransitionManager.Transition("AlphaFade", scene);
        }
    }

    void Creation()
    {
        if (newName.text == "")
        {
            StartCoroutine(CausedError("Your save file needs a name."));
        }
        else if (!Regex.IsMatch(newName.text, @"^[a-zA-Z]+$"))
        {
            StartCoroutine(CausedError("Save file names can't have punctuation or numbers."));
        }
        else
        {
            SaveManager.instance.NewFile(newName.text);
            SceneTransitionManager.Transition("AlphaFade", scene);
        }
    }

    void Deletion()
    {
        if (fileChoose.options.Count > 0)
        {
            SaveManager.instance.DeleteData(fileChoose.options[fileChoose.value].text);
            fileChoose.options.RemoveAt(fileChoose.value);
            fileChoose.RefreshShownValue();
        }
    }
}