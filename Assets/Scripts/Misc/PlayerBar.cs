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
    public Animator animator;

    private void Awake()
    {
        button = GetComponent<Button>();
        playerName = this.transform.GetChild(0).GetComponent<TMP_Text>();
        playerStats = this.transform.GetChild(1).GetComponent<TMP_Text>();

        animator = this.GetComponent<Animator>();
    }

    public void ChangeText(string stats)
    {
        playerStats.text = stats;
    }

    private void OnMouseOver()
    {
        animator.SetBool("idle", false);
        animator.SetBool("selected", true);
    }

    private void OnMouseExit()
    {
        animator.SetBool("idle", false);
        animator.SetBool("selected", false);
    }

    public void AnimIdleTrigger()
    {
        animator.SetBool("idle", true);
    }

    private void Update()
    {
        Debug.Log("is selected: " + animator.GetBool("selected"));
        Debug.Log("is idle: " + animator.GetBool("idle"));
    }
}
