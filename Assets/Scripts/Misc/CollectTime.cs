using UnityEditor;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;

[InitializeOnLoad]
public class CollectTime : MonoBehaviour
{
    public static CollectTime instance;
    public Stopwatch stopwatch = new Stopwatch();

    static CollectTime()
    {
        UnityEngine.Debug.Log("added endtime");
        EditorApplication.quitting += EndTime;
    }

    private void Awake()
    {
        instance = this;
        stopwatch.Start();
    }

    public static void EndTime()
    {
        instance.stopwatch.Stop();
        string path = Application.persistentDataPath + "/test.txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine($"{DateTime.Now:MMM d, yyyy}, {ConvertTimeToString(instance.stopwatch.Elapsed)}, {10 - NewManager.instance.turnCount} turns");
        writer.Close();
        instance = null;
    }

    static string ConvertTimeToString(TimeSpan x)
    {
        string part1 = x.Seconds < 10 ? $"0{x.Seconds}" : $"{x.Seconds}";
        string part2 = "";

        if (x.Milliseconds < 10)
            part2 = $"00{x.Milliseconds}";
        else if (x.Milliseconds < 100)
            part2 = $"0{x.Milliseconds}";
        else
            part2 = x.Milliseconds.ToString();

        string timeString = $"{x.Minutes}:{part1}.{part2}";
        return timeString;
    }
}