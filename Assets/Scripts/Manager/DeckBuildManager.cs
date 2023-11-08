using System.Collections;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class DeckBuildManager : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdownPrefab;
    [SerializeField] Transform deckPrefab;

    List<Transform> deckTransforms = new List<Transform>();
    List<TMP_Dropdown> characterDropdowns = new List<TMP_Dropdown>();

    Transform keepDecks;
    Button playGameButton;

    [SerializeField] AK.Wwise.Event cardMove;

    private void Awake()
    {
        Transform canvas = GameObject.Find("Canvas").transform;
        playGameButton = GameObject.Find("Play Game Button").GetComponent<Button>();

        keepDecks = GameObject.Find("Keep Decks").transform;
        keepDecks.transform.localPosition = new Vector3(10000, 10000, 0);

        for (int i = 0; i < 3; i++)
        {
            TMP_Dropdown newDropdown = Instantiate(dropdownPrefab, canvas);
            newDropdown.transform.localPosition = new Vector3(-500 + 500 * i, 0, 0);
            characterDropdowns.Add(newDropdown);
            newDropdown.onValueChanged.AddListener(delegate { SaveInformation(); });

            Button button = newDropdown.transform.Find("View Deck").GetComponent<Button>();
            int j = i;
            button.onClick.AddListener(() => ViewDeck(j));
        }

        for (int i = 0; i < SaveManager.instance.playerDecks.Count; i++)
        {
            string deckName = SaveManager.instance.playerDecks[i];
            Transform newDeck = Instantiate(deckPrefab, keepDecks);
            newDeck.transform.localPosition = new Vector3(-2000 * i, -200, 0);
            deckTransforms.Add(newDeck);

            foreach (TMP_Dropdown dropdown in characterDropdowns)
                dropdown.options.Add(new TMP_Dropdown.OptionData(deckName));

            List<Card> addToDeck = SaveManager.instance.GenerateCards(deckName);
            foreach (Card card in addToDeck)
                card.transform.SetParent(newDeck.GetChild(0).GetChild(0));
        }

        if (SaveManager.instance.currentSaveData.freshFile)
        {
            SaveInformation();
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                string findName = SaveManager.instance.currentSaveData.chosenDecks[i];
                for (int j = 0; j < SaveManager.instance.playerDecks.Count; j++)
                {
                    string deckName = SaveManager.instance.playerDecks[j];
                    if (deckName == findName)
                    {
                        characterDropdowns[i].value = j;
                        continue;
                    }
                }
            }
        }
    }

    public void SaveInformation()
    {
        List<string> decksToSave = new List<string>();
        foreach(TMP_Dropdown dropdown in characterDropdowns)
            decksToSave.Add(dropdown.options[dropdown.value].text);
        SaveManager.instance.SaveHand(decksToSave);
    }

    public void ViewDeck(int character)
    {
        int deckNumber = characterDropdowns[character].value;
        keepDecks.transform.localPosition = new Vector3(deckNumber * -2000, 0, 0);
    }
}
