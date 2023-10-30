using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RightClick : MonoBehaviour
{
    public static RightClick instance;
    [SerializeField] Image bigImage;
    [SerializeField] TMP_Text cardName;
    [SerializeField] TMP_Text cardCost;
    [SerializeField] TMP_Text cardDescr;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            this.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            this.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void ChangeCard(Card newCard)
    {
        this.transform.GetChild(0).gameObject.SetActive(true);
        bigImage = newCard.image;

        this.cardName.text = newCard.TextName.text;
        this.cardCost.text = newCard.TextCost.text;
        this.cardDescr.text = newCard.TextDescr.text;
    }
}
