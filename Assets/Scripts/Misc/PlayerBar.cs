using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using TMPro;
using UnityEngine.UI;

public class PlayerBar : MonoBehaviour
{
    [ReadOnly] public Button button;
    [ReadOnly] public TMP_Text playerName;
    [ReadOnly] public TMP_Text playerStats;

    private void Awake()
    {
        button = GetComponent<Button>();
        playerName = this.transform.GetChild(0).GetComponent<TMP_Text>();
        playerStats = this.transform.GetChild(1).GetComponent<TMP_Text>();
    }

    public void ChangeText(string stats)
    {
        playerStats.text = stats;
    }
}
