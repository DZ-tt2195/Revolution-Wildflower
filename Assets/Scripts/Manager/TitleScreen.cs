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

public class TitleScreen : MonoBehaviour
{
    Button deleteFile;
    Button createFile;
    Button loadFile;

    TMP_Text errorText;

    TMP_InputField newName;
    TMP_Dropdown fileChoose;

    TMP_Dropdown animationSpeed;
    Toggle confirmationToggle;

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

        animationSpeed = GameObject.Find("Card Animation Dropdown").GetComponent<TMP_Dropdown>();
        animationSpeed.onValueChanged.AddListener(delegate { SetAnimationSpeed(); });

        confirmationToggle = GameObject.Find("Confirmation Toggle").GetComponent<Toggle>();
        confirmationToggle.onValueChanged.AddListener(delegate { SetConfirmationStatus(); });
    }

    private void Start()
    {
        string[] currentFileNames = ES3.GetFiles(Application.persistentDataPath);
        foreach(string name in currentFileNames)
        {
            if (name != ".DS_Store")
            {
                fileChoose.options.Add(new TMP_Dropdown.OptionData(name[..^4]));
                fileChoose.RefreshShownValue();
            }
        }

        if (!PlayerPrefs.HasKey("Confirm Choices")) //0 doesn't ask for confirmation, 1 does
            PlayerPrefs.SetInt("Confirm Choices", 1);
        confirmationToggle.isOn = (PlayerPrefs.GetInt("Confirm Choices") == 1);

        if (!PlayerPrefs.HasKey("Animation Speed"))
            PlayerPrefs.SetFloat("Animation Speed", 0.4f);

        switch (PlayerPrefs.GetFloat("Animation Speed"))
        {
            case 0f:
                animationSpeed.value = 2;
                break;
            case 0.25f:
                animationSpeed.value = 1;
                break;
            case 0.4f:
                animationSpeed.value = 0;
                break;
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
            SaveManager.instance.UnloadObjects();
            SceneManager.LoadScene(1);
        }
    }

    void Creation()
    {
        if (newName.text == "")
        {
            StartCoroutine(CausedError("Your save file needs a name."));
        }
        else
        {
            SaveManager.instance.NewFile(newName.text);
            SaveManager.instance.UnloadObjects();
            SceneManager.LoadScene(1);
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

    void SetConfirmationStatus()
    {
        PlayerPrefs.SetInt("Confirm Choices", confirmationToggle.isOn ? 1 : 0);
    }

    void SetAnimationSpeed()
    {
        switch (animationSpeed.options[animationSpeed.value].text)
        {
            case "Slow":
                PlayerPrefs.SetFloat("Animation Speed", 0.4f);
                break;
            case "Fast":
                PlayerPrefs.SetFloat("Animation Speed", 0.25f);
                break;
            case "None":
                PlayerPrefs.SetFloat("Animation Speed", 0f);
                break;
        }
    }
}