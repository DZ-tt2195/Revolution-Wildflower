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
    List<List<Card>> cardsInCollection = new List<List<Card>>();
    List<List<Card>> cardsInDeck = new List<List<Card>>();

    Transform handContainer;
    List<Transform> collectionTransforms = new List<Transform>();
    List<Transform> deckTransforms = new List<Transform>();

    TMP_Dropdown cardDropdown;
    TMP_Dropdown characterDropdown;
    TMP_Text deckSizeText;
    Button playGameButton;

    [SerializeField] int deckSize;
    [SerializeField] AudioClip cardMove;

    private void Awake()
    {
        for (int i = 0; i<3; i++)
        {
            cardsInCollection.Add(new List<Card>());
            cardsInDeck.Add(new List<Card>());
        }

        handContainer = GameObject.Find("All Cards").transform;
        for (int i = 0; i<3; i++)
        {
            collectionTransforms.Add(handContainer.GetChild(0).GetChild(i).GetChild(0).GetChild(0));
            deckTransforms.Add(handContainer.GetChild(1).GetChild(i).GetChild(0).GetChild(0));
        }

        cardDropdown = GameObject.Find("Card Dropdown").GetComponent<TMP_Dropdown>();
        cardDropdown.onValueChanged.AddListener(delegate { NewSort(); });
        characterDropdown = GameObject.Find("Character Dropdown").GetComponent<TMP_Dropdown>();
        characterDropdown.onValueChanged.AddListener(delegate { SwitchDecks(); });

        deckSizeText = GameObject.Find("Deck Text").GetComponent<TMP_Text>();
        playGameButton = GameObject.Find("Play Game Button").GetComponent<Button>();
    }

    private void Start()
    {
        for (int i = 0; i<SaveManager.instance.characterCards.Count; i++)
        {
            characterDropdown.value = i;
            foreach(Card card in SaveManager.instance.characterCards[i])
            {
                card.transform.localScale = new Vector3(0.8f, 0.8f, 1);
                RemoveFromDeck(card, false);
            }
        }

        for (int i = 0; i < SaveManager.instance.currentSaveData.savedDecks.Count; i++)
        {
            characterDropdown.value = i;
            foreach (string card in SaveManager.instance.currentSaveData.savedDecks[i])
            {
                AddToDeck(collectionTransforms[characterDropdown.value].Find(card).GetComponent<Card>(), false);
            }
        }

        characterDropdown.value = 0;
        StartCoroutine(SwapCards());
    }

    public void SwitchDecks()
    {
        handContainer.transform.localPosition = new Vector3(characterDropdown.value * -2000, 0, 0);
    }

    public void NewSort()
    {
        foreach (List<Card> cardList in cardsInCollection)
        {
            foreach (Card card in cardList)
            {
                ApplySorting(card);
            }
        }
    }

    void ApplySorting(Card card)
    {
        switch (cardDropdown.options[cardDropdown.value].text)
        {
            case "":
                card.gameObject.SetActive(true);
                break;
            case "All cards":
                card.gameObject.SetActive(true);
                break;
            case "Costs 0":
                card.gameObject.SetActive(card.energyCost == 0);
                break;
            case "Costs 1":
                card.gameObject.SetActive(card.energyCost == 1);
                break;
            case "Costs 2":
                card.gameObject.SetActive(card.energyCost == 2);
                break;
            case "Costs 3":
                card.gameObject.SetActive(card.energyCost == 3);
                break;
            case "Attack":
                card.gameObject.SetActive(card.typeOne == Card.CardType.Attack || card.typeTwo == Card.CardType.Attack);
                break;
            case "Draw":
                card.gameObject.SetActive(card.typeOne == Card.CardType.Draw || card.typeTwo == Card.CardType.Draw);
                break;
            case "Energy":
                card.gameObject.SetActive(card.typeOne == Card.CardType.Energy || card.typeTwo == Card.CardType.Energy);
                break;
            case "Movement":
                card.gameObject.SetActive(card.typeOne == Card.CardType.Movement || card.typeTwo == Card.CardType.Movement);
                break;
            case "Misc effect":
                card.gameObject.SetActive(card.typeOne == Card.CardType.Misc || card.typeTwo == Card.CardType.Misc);
                break;
            default:
                Debug.LogError("filter not implemented");
                break;
        }
    }

    public void AddToDeck(Card newCard, bool save)
    {
        if (cardsInDeck[characterDropdown.value].Count < deckSize)
        {
            cardsInCollection[characterDropdown.value].Remove(newCard);
            cardsInDeck[characterDropdown.value].Add(newCard);
            newCard.transform.SetParent(deckTransforms[characterDropdown.value]);
            ApplySorting(newCard);

            SoundManager.instance.PlaySound(cardMove);

            if (save)
                SaveManager.instance.SaveHand(cardsInDeck, SaveManager.instance.saveFileName);
        }
    }

    public void RemoveFromDeck(Card newCard, bool save)
    {
        playGameButton.gameObject.SetActive(false);
        cardsInDeck[characterDropdown.value].Remove(newCard);
        cardsInCollection[characterDropdown.value].Add(newCard);
        newCard.transform.SetParent(collectionTransforms[characterDropdown.value]);

        SoundManager.instance.PlaySound(cardMove);
        if (save)
            SaveManager.instance.SaveHand(cardsInDeck, SaveManager.instance.saveFileName);
    }

    bool CheckDecks()
    {
        foreach(List<Card> deck in cardsInDeck)
        {
            if (deck.Count != deckSize)
                return false;
        }
        return true;
    }

    IEnumerator SwapCards()
    {
        playGameButton.gameObject.SetActive(CheckDecks());
        deckSizeText.text = $"Deck ({cardsInDeck[characterDropdown.value].Count}/{deckSize})";
        foreach(List<Card> cardList in SaveManager.instance.characterCards)
            ChoiceManager.instance.ChooseCard(cardList);

        while (ChoiceManager.instance.chosenCard == null)
            yield return null;

        //swap cards between your deck and collection
        if (cardsInCollection[characterDropdown.value].Contains(ChoiceManager.instance.chosenCard))
            AddToDeck(ChoiceManager.instance.chosenCard, true);
        else
            RemoveFromDeck(ChoiceManager.instance.chosenCard, true);

        StartCoroutine(SwapCards());
    }
}
