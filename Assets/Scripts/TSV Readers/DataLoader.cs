using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using MyBox;
using UnityEngine.SceneManagement;

//the data that a single card holds; this has the same fields and descriptions as in the spreadsheet
public class CardData
{
    public string name;    //name at top of card
    public string desc;    //card explanation displayed on the card itself
    public string cat1;    //atk (attack), draw, dist (distraction), eng (energy), mvmt (movement), misc (miscellaneous)
    public string cat2;    //same categories as above

    public int maxInv;     //number of cards in inventory
    public int epCost;     //EP cost to play card
    public bool isViolent; //violent or nonviolent

    public int chHP;       //HP change
    public int chMP;       //MP change
    public int chEP;       //EP change
    public int draw;       //number of cards to draw
    public int chooseHand;       //number of cards to draw

    public int stun;       //number of turns the action stuns
    public int range;      //range of action
    public int aoe;        //area of action effect
    public int delay;      //number of turns to delay
    public int wHP;        //change in wall HP
    public int volume;       //volume intensity
    public int vision;       //vision

    public string select;  //any extra condition needed to select a tile
    public string action;  //action to do
    public string enviroaction;  //environmental effect to do
    public string nextAct; //action to do next turn
}

//Uses TSVReader class to translate values from the CardData CSV.
public class CardDataLoader
{
    //Call this function at the beginning of your script to generate the cards and deck
    public static List<CardData> ReadCardData(string fileToLoad)
    {
        List<CardData> cardData = new List<CardData>();
        var data = TSVReader.ReadCards(fileToLoad);
        for (int i = 3; i < data.Length; i++)
        {
            string[] line = data[i];
            for (int j = 0; j < line.Length; j++)
            {
                line[j] = line[j].Trim().Replace("\"", "").Replace("\\", "");
                //Debug.Log(line[j]);
            }

            CardData newCard = new CardData();
            cardData.Add(newCard);

            newCard.name = line[0];
            Debug.Log(newCard.name);
            newCard.desc = line[1];
            newCard.cat1 = line[2];
            newCard.cat2 = line[3];

            newCard.maxInv = StringToInt(line[4]);
            newCard.epCost = StringToInt(line[5]);
            newCard.isViolent = (line[6] == "v");

            newCard.chHP = StringToInt(line[7]);
            newCard.chMP = StringToInt(line[8]);
            newCard.chEP = StringToInt(line[9]);
            newCard.draw = StringToInt(line[10]);
            newCard.chooseHand = StringToInt(line[11]);
            newCard.stun = StringToInt(line[12]);
            newCard.range = StringToInt(line[13]);

            newCard.aoe = StringToInt(line[14]);
            newCard.delay = StringToInt(line[15]);
            newCard.wHP = StringToInt(line[16]);
            newCard.volume = StringToInt(line[17]);
            newCard.vision = StringToInt(line[18]);

            newCard.select = line[19];
            newCard.action = line[20];
            try { newCard.enviroaction = line[21]; } catch (IndexOutOfRangeException) { newCard.nextAct = ""; }
            try { newCard.nextAct = line[22]; } catch (IndexOutOfRangeException) { newCard.nextAct = ""; }
        }
        return cardData;
    }

    //Convert the string to an integer (returning 0 if the line is empty)
    static int StringToInt(string line)
    {
        try
        {
            return (line == "") ? 0 : int.Parse(line);
        }
        catch (FormatException)
        {
            Debug.LogError($"{line} wasn't read correctly");
            return -1;
        }
    }
}

//A slightly redundant level loader using the TSVReader class
public class LevelLoader
{
    public static string[,] LoadLevelGrid(string levelToLoad)
    {
        string[,] grid = TSVReader.ReadLevel(levelToLoad);
        return grid;
    }
}