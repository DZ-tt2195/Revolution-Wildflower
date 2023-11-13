//modified from code by Teemu Ikonen

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TSVReader
{
	static string SPLIT_RE = @"\t(?=(?:[^""]*""[^""]*"")*(?![^""]*""))"; //Regular expressions to split TSVs
	static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
	static char[] TRIM_CHARS = { '\"' };

	/// <summary>
	/// Reads a TSV from a file in the Resources folder for use in card data generation
	/// </summary>
	/// <param name="file">The path of the file to load</param>
	/// <param name="headerLines">The number of lines at the top to skip over as headers</param>
	/// <returns>A jagged array of strings to use in card scripts</returns>
	public static string[][] ReadCards(string file)
	{
		TextAsset data = Resources.Load(file) as TextAsset;

		string editData = data.text;
		editData = editData.Replace("],", "").Replace("]", "").Replace("{", "").Replace("}", "");

        string[] numCards = editData.Split("[");
        string[][] list = new string[numCards.Length][];

		for (int i = 0; i<numCards.Length; i++)
		{
			list[i] = numCards[i].Split("\",");
        }
		return list;
	}

	/// <summary>
	/// Reads a TSV from a file in the Resources folder for use in level generation
	/// </summary>
	/// <param name="file">The path of the file to load</param>
	/// <returns>A 2D array of strings to be translated into the level grid</returns>
	public static string[,] ReadLevel(string file)
    {
        TextAsset data = Resources.Load(file) as TextAsset;
        var rows = Regex.Split(data.text, LINE_SPLIT_RE);
        int gridHeight = rows.Length;
        if (gridHeight < 1) return new string[0, 0];
        var gridWidth = Regex.Split(rows[0], SPLIT_RE).Length;
        var grid = new string[gridWidth, gridHeight];

        for (int y = 0; y < gridHeight; y++)
        {
            var cells = Regex.Split(rows[y], SPLIT_RE);
            for (int x = 0; x < gridWidth; x++)
            {
                grid[x, y] = cells[x];
            }
        }

        return grid;
    }
}