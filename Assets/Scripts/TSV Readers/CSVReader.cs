using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TSVReader
{
	public static string[][] ReadCards(string file)
	{
		TextAsset data = Resources.Load(file) as TextAsset;

		string editData = data.text;
		editData = editData.Replace("],", "").Replace("{", "").Replace("}", "");

        string[] numCards = editData.Split("[");
        string[][] list = new string[numCards.Length][];

		for (int i = 0; i<numCards.Length; i++)
		{
			list[i] = numCards[i].Split("\",");
        }
		return list;
	}

	public static string[,] ReadLevel(string file)
    {
        TextAsset data = Resources.Load(file) as TextAsset;

		string editData = data.text;
		editData = editData.Replace("],", "").Replace("{", "").Replace("}", "");

        string[] numRows = editData.Split("[");
        string[][] list = new string[numRows.Length][];
		int maxCol = 0;

		for (int i = 0; i < numRows.Length; i++)
		{
			list[i] = numRows[i].Split("\",");
			if (list[i].Length > maxCol)
				maxCol = list[i].Length;
		}

		string[,] grid = new string[numRows.Length, maxCol];
		for (int x = 0; x<numRows.Length; x++)
		{
			for (int y = 0; y < maxCol; y++)
			{
				try
				{
					grid[x-1, y] = list[x][y];
				}
				catch (IndexOutOfRangeException)
				{
					continue;
				}
			}
		}

		return grid;
    }
}