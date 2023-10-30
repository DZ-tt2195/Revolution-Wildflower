//modified from code by Teemu Ikonen

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TSVReader
{
	static string SPLIT_RE = @"\t(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
	static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
	static char[] TRIM_CHARS = { '\"' };

	/// <summary>
	/// Reads a TSV from a file in the Resources folder and outputs a jagged array of strings
	/// </summary>
	/// <param name="file">The path of the file to load</param>
	/// <param name="headerLines">The number of lines at the top to skip over as headers</param>
	/// <returns></returns>
	public static string[][] ReadCards(string file, int headerLines)
	{
		TextAsset data = Resources.Load(file) as TextAsset;

		var lines = Regex.Split(data.text, LINE_SPLIT_RE);
		if (lines.Length - headerLines <= 0) return new string[0][];
		var list = new string[lines.Length - headerLines][];

		for (var i = headerLines; i < lines.Length; i++) {

			var values = Regex.Split(lines[i], SPLIT_RE);
			list[i - headerLines] = values;
		}
		return list;
	}

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