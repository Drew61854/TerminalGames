using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Steamworks.InventoryItem;
using Random = UnityEngine.Random;

namespace ConsoleGames
{
    public class DiceClasses
    {
        public class Dice 
        {
            public int faceValue;
            public Dice()
            {
                faceValue = Roll();
            }
            public int Roll()
            {
                faceValue = Random.Range(1, 7);
                return faceValue;
            }
        }
        public static string DisplayDice(List<int> rolls) //ASCII representation of the dice rolls
        {
            string[] stringPieces =
                    {"", 
                     "", 
                     "", 
                     "",
                     "" };
            string returnString = "";
            for (int i = 0; i < rolls.Count; i++) 
            { 
                if (rolls[i] == 1)
                {
                    stringPieces[0] += "┌───────┐ ";
                    stringPieces[1] += "│       │ ";
                    stringPieces[2] += "│   ●   │ ";
                    stringPieces[3] += "│       │ ";
                    stringPieces[4] += "└───────┘ ";
                }
                else if (rolls[i] == 2)
                {
                    stringPieces[0] += "┌───────┐ ";
                    stringPieces[1] += "│ ●     │ ";
                    stringPieces[2] += "│       │ ";
                    stringPieces[3] += "│     ● │ ";
                    stringPieces[4] += "└───────┘ ";
                }
                else if (rolls[i] == 3)
                {
                    stringPieces[0] += "┌───────┐ ";
                    stringPieces[1] += "│ ●     │ ";
                    stringPieces[2] += "│   ●   │ ";
                    stringPieces[3] += "│     ● │ ";
                    stringPieces[4] += "└───────┘ ";
                }
                else if (rolls[i] == 4)
                {
                    stringPieces[0] += "┌───────┐ ";
                    stringPieces[1] += "│ ●   ● │ ";
                    stringPieces[2] += "│       │ ";
                    stringPieces[3] += "│ ●   ● │ ";
                    stringPieces[4] += "└───────┘ ";
                }
                else if (rolls[i] == 5)
                {
                    stringPieces[0] += "┌───────┐ ";
                    stringPieces[1] += "│ ●   ● │ ";
                    stringPieces[2] += "│   ●   │ ";
                    stringPieces[3] += "│ ●   ● │ ";
                    stringPieces[4] += "└───────┘ ";
                }
                else
                {
                    stringPieces[0] += "┌───────┐ ";
                    stringPieces[1] += "│ ●   ● │ ";
                    stringPieces[2] += "│ ●   ● │ ";
                    stringPieces[3] += "│ ●   ● │ ";
                    stringPieces[4] += "└───────┘ ";
                }
            }
            returnString = stringPieces[0] + "\n" + stringPieces[1] + "\n" + stringPieces[2] + "\n" + stringPieces[3] + "\n" + stringPieces[4];
            return returnString;
        }
        public static int TranslateString(string original)//Take a string one to ten and return it as an int. 
        {
            int newInt;
            switch (original)
            {
                case "one":
                    newInt = 1;
                    break;
                case "two":
                    newInt = 2;
                    break;
                case "three":
                    newInt = 3;
                    break;
                case "four":
                    newInt = 4;
                    break;
                case "five":
                    newInt = 5;
                    break;
                case "six":
                    newInt = 6;
                    break;
                case "seven":
                    newInt = 7;
                    break;
                case "eight":
                    newInt = 8;
                    break;
                case "nine":
                    newInt = 9;
                    break;
                default:
                    newInt = 10;
                    break;

            }
            return newInt;
        }

        public static string TranslateInt(int original)//Take an int 1 - 10 and return it spelled out.  
        {
            string newString;
            switch (original)
            {
                case 1:
                    newString = "one";
                    break;
                case 2:
                    newString = "two";
                    break;
                case 3:
                    newString = "three";
                    break;
                case 4:
                    newString = "four";
                    break;
                case 5:
                    newString = "five";
                    break;
                case 6:
                    newString = "six";
                    break;
                case 7:
                    newString = "seven";
                    break;
                case 8:   
                    newString = "eight";
                    break;
                case 9:
                    newString = "nine";
                    break;
                default:
                    newString = "ten";
                    break;

            }
            return newString;
        }  
    }
}
