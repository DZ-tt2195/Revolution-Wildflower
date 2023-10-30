using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class SendChoice : MonoBehaviour
{
    [ReadOnly] public Button button;
    [ReadOnly] public Image image;
    [ReadOnly] public Image border;
    [ReadOnly] public Card myCard;
    bool enableBorder;

    void Awake()
    {
        myCard = this.GetComponent<Card>();
        image = this.GetComponent<Image>();
        button = this.GetComponent<Button>();
        border = this.transform.GetChild(0).GetComponent<Image>();

        if (button != null)
            button.onClick.AddListener(SendName);
    }

    private void FixedUpdate()
    {
        //if this button is able to be pressed, it flashes
        if (border != null && enableBorder)
        {
            border.color = new Color(1, 1, 1, ChoiceManager.instance.opacity);
        }
        else if (border != null && !enableBorder)
        {
            border.color = new Color(1, 1, 1, 0);
        }
    }

    public void SendName()
    {
        //give choice manager the card this has
        if (myCard != null)
            ChoiceManager.instance.ReceiveChoice(myCard);
    }

    public void EnableButton(bool border)
    {
        //this button can now be pressed
        this.gameObject.SetActive(true);
        enableBorder = border;

        if (button != null)
            button.interactable = true;
    }

    public void DisableButton()
    {
        //this button can't be pressed
        enableBorder = false;
        if (button != null)
            button.interactable = false;
    }
}
