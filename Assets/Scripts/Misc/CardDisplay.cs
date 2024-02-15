using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardDisplay : MonoBehaviour
{
    public static CardDisplay instance;
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
        bigImage.sprite = newCard.image.sprite;
        bigImage.color = newCard.image.color;

        this.cardName.text = newCard.textName.text;
        this.cardCost.text = newCard.textCost.text;
        this.cardDescr.text = newCard.textDescr.text;

        Material mat = new Material(bigImage.material);
        mat.SetColor("_GradientColorTop", newCard.ConvertToColor(newCard.typeOne));
        mat.SetColor("_GradientColorBottom", newCard.ConvertToColor(newCard.typeTwo));
        bigImage.material = mat;
    }
}
