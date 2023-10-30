using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyExtensions
{
    public static void Shuffle(this Transform list)
    {
        for (int k = 0; k < 5; k++)
        {
            List<int> indexes = new List<int>();
            List<Transform> items = new List<Transform>();

            for (int i = 0; i < list.childCount; ++i)
            {
                indexes.Add(i);
                items.Add(list.GetChild(i));
            }

            foreach (var x in items)
            {
                x.SetSiblingIndex(indexes[Random.Range(0, indexes.Count)]);
            }
        }
    }
}