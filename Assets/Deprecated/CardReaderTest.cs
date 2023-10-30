/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
using System.Reflection;

public class CardReaderTest : MonoBehaviour
{
    [SerializeField] Card cardPrefab;
    Transform canvas;

    private void Awake()
    {
        canvas = GameObject.Find("Canvas").transform;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        List<CardData> data = CardDataLoader.ReadCardData();

        for (int i = 0; i < data.Count; i++)
        {
            for (int j = 0; j < data[i].maxInv; j++)
            {
                Card nextCopy = Instantiate(cardPrefab, canvas);
                nextCopy.transform.localPosition = new Vector3(10000, 10000);
                nextCopy.CardSetup(data[i]);
            }
        }
    }
}
*/