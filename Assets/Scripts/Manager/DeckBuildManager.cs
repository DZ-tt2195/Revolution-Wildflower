using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckBuildManager : MonoBehaviour
{
    [SerializeField] GameObject characterViewDeck;
    [SerializeField] Transform deckPrefab;

    Transform keepDecks;

    [SerializeField] AK.Wwise.Event viewDeckSound;

    private void Awake()
    {
        keepDecks = GameObject.Find("Keep Decks").transform;
        keepDecks.transform.localPosition = new Vector3(10000, 10000, 0);

        for (int i = 0; i < 3; i++)
        {
            string deckName = SaveManager.instance.playerDecks[i];
            Transform newDeck = Instantiate(deckPrefab, keepDecks);
            newDeck.transform.localPosition = new Vector3(3000 * i, 0, 0);

            List<Card> addToDeck = SaveManager.instance.GenerateCards(deckName);
            foreach (Card card in addToDeck)
                card.transform.SetParent(newDeck.GetChild(0).GetChild(0));
        }
    }

    public void ViewDeck(int character)
    {
        Debug.Log("viewing deck");
        viewDeckSound.Post(gameObject);
        keepDecks.transform.localPosition = new Vector3(character * -3000, 0, 0);
    }
}
