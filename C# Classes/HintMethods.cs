using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalGames;
using UnityEngine.UIElements;
using UnityEngine;
using static TerminalApi.TerminalApi;
using Random = UnityEngine.Random;
using System.Net.Http.Headers;


namespace ConsoleGames
{
    internal class HintMethods
    {
        public static string character = "Null";
        public static int turns = 0;
        public static bool[] displayMarks = new bool[21];
        public static List<string> player2Knowns = new List<string>();
        public static List<string> player3Knowns = new List<string>();
        public static List<string> playerMarks = new List<string>();    
        public static List<string> player2Marks = new List<string>();
        public static List<string> player3Marks = new List<string>();
        public static string playerRoom = "";
        public static List<string> allChoices = new List<string> {"Baron Bracken", "Lord Lootbug", "Tribune Thumper", "General Giant", "Sheriff Slime", "Foreman Flea", "Pool", "Bar", "Galley", "Cellar", "Lobby", "Canteen", "Gameroom", "Balcony", "Office", "Flail", "Dagger", "Bat", "Shotgun", "Shovel", "Landmine"};
        public static List<string> referenceChoices = new List<string>(); 
        public static string[] realHints = new string[3];
        public static int player2Strat = -1;
        public static int player3Strat = -1;
        public static string player2WeaponGuess = "";
        public static string player2CharacterGuess = "";
        public static string player2Room = "";
        public static string player3WeaponGuess = "";
        public static string player3CharacterGuess = "";
        public static string player3Room = "";
        public static string accuseChara = "";
        public static string accuseRoom = "";
        public static string accuseWeapon = "";
        public static string[] addedText = new string[27];
        public static bool playerShowing = false;
        public static bool player2Guessing = false;
        public static bool player3Guessing = false;
        public static bool player2Elim = false;
        public static bool player3Elim = false;
        public static TerminalNode PlayerTurn(Terminal __terminal)
        {
            string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
            if (ConsoleGamesMain.playingHint)
            {
                if (!playerShowing)
                {
                    if (input.Length > 8) //Because no room is only one letter long "select [room]", but "gameroom" is more than seven.
                    {
                        input = input.Substring(7);
                    }
                    if (input.Equals("bar") || input.Equals("pool") || input.Equals("galley") || input.Equals("cellar") || input.Equals("lobby") || input.Equals("canteen")
                        || input.Equals("gameroom") || input.Equals("balcony") || input.Equals("office"))
                    {
                        playerRoom = input.Substring(0, 1).ToUpper() + input.Substring(1); //Make it uppercase
                    }
                    else
                    {
                        string failedRet = "Sorry, that room doesn't exist, or you spelled it wrong. Try again.\n" + AssembleHUD();
                        TerminalNode failedNode = CreateTerminalNode(failedRet);
                        failedNode.clearPreviousText = true;
                        return failedNode;
                    }
                    addedText[0] = "You have entered the ";
                    addedText[1] = playerRoom + ". Enter your";
                    addedText[2] = "guess about who did it and";
                    addedText[3] = "with what weapon.";
                    addedText[4] = "(Guess 'person', 'weapon')";
                    for (int i = 5; i < 18; i++)
                    {
                        addedText[i] = "";
                    }
                    string ret = AssembleHUD(addedText);
                    TerminalNode returnNode = CreateTerminalNode(ret);
                    returnNode.clearPreviousText = true;
                    return returnNode;
                }
                else
                {
                    TerminalNode retNode = CreateTerminalNode(MustShow());
                    retNode.clearPreviousText = true;
                    retNode.maxCharactersToType = 32;
                    return retNode;
                }
            }
            else
            {
                TerminalNode retNode = CreateTerminalNode("There was an issue with your request.\n\n");
                retNode.clearPreviousText = true;
                retNode.maxCharactersToType = 32;
                return retNode;
            }
        }
        public static TerminalNode ChooseChar(Terminal __terminal)
        {
            TerminalNode returnNode = CreateTerminalNode("");
            returnNode.clearPreviousText = true;
            string ret = "";
            realHints = new string[] {"", "", ""};
            playerMarks.Clear();
            player2Marks.Clear();
            player3Marks.Clear();
            if (ConsoleGamesMain.playingHint)
            {
                string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
                if (input.Length > 7)
                { 
                    input = input.ToLower().Substring(7); //Get rid of choose
                }
                else
                {
                    ret = "You must enter a character to choose. Choose your character out of the following list by typing 'Choose (name)'. You can omit the first word of the name.\n\nBaron Bracken\nLord Lootbug\nTribune Thumper\nGeneral Giant\nSheriff Slime\nForeman Flea\n\n\n";
                    returnNode.displayText = ret;
                    return returnNode;
                }
                if (input.Length > 0)
                {
                    string oldChar = input;
                    character = FormatCharacter(input);
                    if (oldChar == character) // Invalid input
                    {
                        ret = "There was an issue with your request, or you entered this when you weren't supposed to. If you were in a game, your progress is still saved, and you can return by typing a command like \"mark\".\n\n";
                        returnNode.displayText = ret;
                        return returnNode;
                    }
                    realHints[0] = Choose(allChoices, "character");
                    realHints[1] = Choose(allChoices, "weapon");
                    realHints[2] = Choose(allChoices, "room");
                    for (int i = 0; i < 3; i++)
                    {    
                        allChoices.Remove(realHints[i]);
                    }
                    for (int i = 0; i < 6; i++) //6 cards for each of the three players after the three hint cards.
                    {
                        string chosen = Choose(allChoices);
                        int index = referenceChoices.IndexOf(chosen);
                        playerMarks.Add(chosen);
                        displayMarks[index] = true;
                        allChoices.Remove(chosen);

                        chosen = Choose(allChoices);
                        player2Marks.Add(chosen);
                        allChoices.Remove(chosen);

                        chosen = Choose(allChoices);
                        player3Marks.Add(chosen);
                        allChoices.Remove(chosen);

                    }
                    player2Strat = AIPersonality();
                    player3Strat = AIPersonality();
                    player2Knowns.AddRange(player2Marks);
                    player3Knowns.AddRange(player3Marks);
                    addedText[0] = "It's your turn. Select";
                    addedText[1] = "a room to start your guess";
                    addedText[2] = "in. Or, if you're ready,";
                    addedText[3] = "sue someone.";
                    addedText[4] = "";
                    addedText[5] = "Your cards have been marked";
                    addedText[6] = "with an X on your notepad.";
                    for (int i = 7; i < 18; i++)
                    {
                        addedText[i] = "";
                    }
                    ret = "You're playing as " + character + ". There are two other players.\n" + AssembleHUD(addedText);
                }
                else
                {
                    ret = "You must enter a character to choose. Choose your character out of the following list by typing 'Choose (name)'. You can omit the first word of the name.\n\nBaron Bracken\nLord Lootbug\nTribune Thumper\nGeneral Giant\nSheriff Slime\nForeman Flea\n\n\n";
                }
            }
            else
            {
                ret = "There was an issue with your request.\n\n";  
            }
            returnNode.displayText = ret;
            return returnNode;
        }
        public static TerminalNode BeginHint(Terminal __terminal)
        {
            ConsoleGamesMain.playingHint = true;
            for (int i = 0; i < 21; i++)
            {
                displayMarks[i] = false;
            }
            allChoices = new List<string> {"Baron Bracken", "Lord Lootbug", "Tribune Thumper", "General Giant", "Sheriff Slime", "Foreman Flea", "Pool", "Bar", "Galley", "Cellar", "Lobby", "Canteen", "Gameroom", "Balcony", "Office", "Flail", "Dagger", "Bat", "Shotgun", "Shovel", "Landmine"};
            referenceChoices.Clear();
            referenceChoices.AddRange(allChoices);
            player2Knowns.Clear();
            player3Knowns.Clear();
            player2Marks.Clear();
            player3Marks.Clear();
            string ret = "Choose your character out of the following list by typing 'Choose (name)'. You can omit the first word of the name.\n\nBaron Bracken\nLord Lootbug\nTribune Thumper\nGeneral Giant\nSheriff Slime\nForeman Flea\n\n\n";
            TerminalNode returnNode = CreateTerminalNode(ret);
            returnNode.clearPreviousText = true;
            return returnNode;
        }

        public static TerminalNode Mark(Terminal __terminal)
        {
            if (ConsoleGamesMain.playingHint)
            {
                for (int i = 0; i < 18; i++)
                    addedText[i] = "";
                string ret = "";
                string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
                if (input.Length > 5)
                {
                    addedText[0] = "It's been marked.";
                    addedText[1] = "";
                    if (playerShowing)
                    {
                        string guessedWep;
                        string guessedChara;
                        string guessedRoom;
                        if (player2Guessing)
                        {
                            guessedChara = player2CharacterGuess;
                            guessedWep = player2WeaponGuess;
                            guessedRoom = player2Room;
                        }
                        else
                        {
                            guessedChara = player3CharacterGuess;
                            guessedWep = player3WeaponGuess;
                            guessedRoom = player3Room;
                        }
                        addedText[2] = "Enter something to show.";
                        addedText[3] = "They guessed it was ";
                        addedText[4] = guessedChara + " in the ";
                        addedText[5] = guessedRoom + " with the";
                        addedText[6] = guessedWep;
                    }
                    else
                    {
                        addedText[2] = "Select a room to enter or";
                        addedText[3] = "enter a guess.";
                    }
                    input = input.Substring(5).ToLower(); //Removes the mark part
                    string isItChar = FormatCharacter(input);
                    input = input.Substring(0, 1).ToUpper() + input.Substring(1);
                    if (referenceChoices.Contains(input))
                    {
                        displayMarks[referenceChoices.IndexOf(input)] = !displayMarks[referenceChoices.IndexOf(input)];
                    }
                    else if (referenceChoices.Contains(isItChar))
                    {
                        displayMarks[referenceChoices.IndexOf(isItChar)] = !displayMarks[referenceChoices.IndexOf(isItChar)];
                    }
                    else
                    {
                        
                        addedText[0] = "Something went wrong.";
                        addedText[1] = "Please try again.";
                        addedText[2] = "(Ex. Mark 'thumper')";
                        addedText[3] = "";
                        for (int i = 4; i < 18; i++)
                            addedText[i] = "";
                        ret = AssembleHUD(addedText);
                    }
                }
                else
                {
                    addedText[0] = "Something went wrong.";
                    addedText[1] = "Please try again.";
                    addedText[2] = "(Ex. Mark 'thumper')";
                    addedText[3] = "";
                    
                }
                ret = AssembleHUD(addedText);
                TerminalNode returnNode = CreateTerminalNode(ret);
                returnNode.clearPreviousText= true;
                returnNode.maxCharactersToType = 32;
                return returnNode;
            }
            else
            {
                TerminalNode retNode = CreateTerminalNode("There was an issue with your request.");
                retNode.clearPreviousText = true;
                return retNode;
            }
        }
        public static TerminalNode Show(Terminal __terminal)
        {
            if (ConsoleGamesMain.playingHint)
            {
                string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
                string ret = "";
                TerminalNode returnNode;

                if (playerShowing)
                {
                    for (int i = 0; i < 18; i++)
                    {
                        addedText[i] = "";
                    }
                    if (input.Length > 4) //Otherwise they didn't put anything.
                    {
                        input = input.Substring(5);
                        input = FormatCharacter(input);
                        input = input.Substring(0, 1).ToUpper() + input.Substring(1); //Capitalize the first letter, because that's how it is.
                    }
                    if (referenceChoices.Contains(input) && playerMarks.Contains(input) == true) //Valid choice and player has it
                    {
                        if (player2Guessing)
                        {
                            player2Knowns.Add(input);
                            player3Guessing = true;
                            player2Guessing = false;
                            addedText[0] = "It's player three's turn.";
                            if (AIAccuse(3))
                            {
                                addedText[1] = "Player three is sueing!";
                                addedText[2] = "Their guess was..." + EvaluateAccuse();
                                addedText[3] = "Game Over!";
                                ConsoleGamesMain.playingHint = false;
                                for (int i = 8; i < 18; i++)
                                    addedText[i] = "";
                                if (player3Elim == true && player2Elim == false && ConsoleGamesMain.playingHint == true)
                                {
                                    player3Guessing = false;
                                    player2Guessing = false;
                                    addedText[4] = "Player three is eliminated.";
                                    addedText[5] = "";
                                    addedText[6] = "It's your turn. Select a";
                                    addedText[7] = "room to start your guess.";
                                    ConsoleGamesMain.playingHint = true;
                                }
                                else if (player3Elim == true && player2Elim == true)
                                {
                                    addedText[4] = "Player three is eliminated.";
                                    addedText[5] = "";
                                    addedText[6] = "You win by default!";
                                    ConsoleGamesMain.playingHint = false;
                                }
                                ret = AssembleHUD(addedText);
                                returnNode = CreateTerminalNode(ret);
                                returnNode.clearPreviousText = true;
                                returnNode.maxCharactersToType = 32;
                                return returnNode;
                            }
                            addedText[1] = NPCGuess() + ".";
                            addedText[2] = "Player three guesses it was";
                            addedText[3] = player3CharacterGuess + " with the";
                            addedText[4] = player3WeaponGuess;
                            if (PlayerHasIt(player3CharacterGuess, player3WeaponGuess, player3Room) == true)
                                addedText[5] = "Enter something to show.";
                            else
                            {
                                playerShowing = false;
                                addedText[5] = NPCResult();
                                addedText[6] = "It's your turn. Select";
                                addedText[7] = "a room to start your guess";
                                addedText[8] = "in. Or, if you're ready,";
                                addedText[9] = "sue someone.";
                            }
                        }
                        else if (player3Guessing)
                        {
                            player3Knowns.Add(input);
                            player2Guessing = false;
                            player3Guessing = false;
                            playerShowing = false;
                            addedText[0] = "It's your turn. Select";
                            addedText[1] = "a room to start your guess";
                            addedText[2] = "in. Or, if you're ready,";
                            addedText[3] = "sue someone.";
                        }
                    }
                    else
                    {
                        ret = "There was an issue with what you entered.\n";
                        if (player2Guessing)
                        {
                            addedText[0] = "Player two guesses it was";
                            addedText[1] = player2CharacterGuess + " with the";
                            addedText[2] = player2WeaponGuess + " in the " + player2Room;
                            addedText[3] = "Enter something to show.";
                        }
                        else if (player3Guessing)
                        {
                            addedText[0] = "Player three guesses it was";
                            addedText[1] = player3CharacterGuess + " with the";
                            addedText[2] = player3WeaponGuess + " in the " + player3Room;
                            addedText[3] = "Enter something to show.";
                        }
                        
                        for (int i = 4; i < 18; i++)
                        {
                            addedText[i] = "";
                        }

                    }
                }
                else
                {
                    ret = "It's not your turn to show something.\n";
                    addedText[0] = "It's your turn. Select";
                    addedText[1] = "a room to start your guess";
                    addedText[2] = "in. Or, if you're ready,";
                    addedText[3] = "sue someone.";
                }
                ret += AssembleHUD(addedText);
                returnNode = CreateTerminalNode(ret);
                returnNode.clearPreviousText = true;
                returnNode.maxCharactersToType = 32;
                return returnNode;
            }
            else
            {
                TerminalNode retNode = CreateTerminalNode("There was an issue with your request.\n\n");
                retNode.clearPreviousText = true;
                retNode.maxCharactersToType = 32;
                return retNode;
            }
        } 

        public static TerminalNode Sue(Terminal __terminal)
        {
            string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
            input.TrimStart();
            string ret = "";
            string part1 = "";
            string part2 = "";
            string part3 = "";
            TerminalNode finalNode;

            if (ConsoleGamesMain.playingHint && input.Length > 4)
            {
                if (!playerShowing)
                {
                    ConsoleGamesMain.playingHint = false;
                    input = input.Substring(4); //Remove 'sue '
                    int lastIndex = -1;
                    for (int i = 0; i < input.Length; i++)
                    {
                        if (input[i] == ' ')
                        {
                            if (part1 == "")
                            {
                                part1 = input.Substring(0, i);
                                lastIndex = i + 1;
                                string tryFormat = FormatCharacter(part1);
                                if (part1 != tryFormat)
                                {
                                    part1 = tryFormat;
                                    break;
                                }
                                part1 = part1.Substring(0, 1).ToUpper() + part1.Substring(1);
                            }
                            else if (part2 == "")
                            {
                                part2 = input.Substring(lastIndex, i - lastIndex);
                                lastIndex = i + 1;
                                string tryFormat = FormatCharacter(part2);
                                if (part2 != tryFormat)
                                {
                                    part2 = tryFormat;
                                    break;
                                }
                                part2 = part2.Substring(0, 1).ToUpper() + part2.Substring(1);
                            }
                        }
                        else if (i == input.Length - 1) //Last char, so it's the end of part3
                        {
                            part3 = input.Substring(lastIndex).TrimEnd();
                            string tryFormat = FormatCharacter(part3);
                            if (part3 != tryFormat)
                            {
                                part3 = tryFormat;
                                break;
                            }
                            part3 = part3.Substring(0, 1).ToUpper() + part3.Substring(1);
                            break; //Done after all three parts
                        }
                    }
                    if (realHints.Contains(part1) && realHints.Contains(part2) && realHints.Contains(part3))
                    {
                        ret = "You are absolutely correct! Congratulations, you have won this game of Hint.\n\n";
                        ConsoleGamesMain.playingHint = false;
                    }
                    else if (part3 != "")
                    {
                        ret = "Sorry, but that's not quite right.\n\nIt was " + realHints[0] + " with the " + realHints[1] + " in the " + realHints[2] + ".\nBetter luck next time!\n\n";
                        ConsoleGamesMain.playingHint = false;
                    }
                    else
                    {
                        ConsoleGamesMain.playingHint = true;  
                        for (int i = 0; i < 18; i++)
                            addedText[i] = "";
                        addedText[0] = "It's your turn. Select";
                        addedText[1] = "a room to start your guess";
                        addedText[2] = "in. Or, if you're ready,";
                        addedText[3] = "sue someone.";
                        ret = "You must enter your entire guess.\n" + AssembleHUD(addedText);
                    }
                }
                else
                {
                    ret = MustShow();
                }
            }
            else
            {
                ret = "There was an issue with your request.\n\n";
            }
            finalNode = CreateTerminalNode(ret);
            finalNode.clearPreviousText = true;
            return finalNode;
        }

        public static string NPCGuess()
        {
            string charGuess = "";
            string roomGuess = "";
            string weaponGuess = "";
            List<string> marks = player2Marks;
            List<string> knowns = player2Knowns;
            int NPCStrat = player2Strat;
            if (player3Guessing)
            {
                NPCStrat = player3Strat;
                marks = player3Marks;
                knowns = player3Knowns;
            }
            if (NPCStrat == 1) //Completely random, with the exception of not choosing all three things that they have.
            {
                charGuess = Choose(referenceChoices, "character");
                roomGuess = Choose(referenceChoices, "room");
                weaponGuess = Choose(referenceChoices, "weapon");
                while (knowns.Contains(charGuess) && knowns.Contains(weaponGuess) && knowns.Contains(roomGuess))
                {
                    charGuess = Choose(referenceChoices, "character");
                    roomGuess = Choose(referenceChoices, "room");
                    weaponGuess = Choose(referenceChoices, "weapon");
                }
            }
            else if (NPCStrat == 2) //Looks for whatever they have the most of first, then whatever they have the second most of, then the final.
            {
                string choice = FindMost();
                List<string> availableChoices = new List<string>();
                List<string> secondChoices = new List<string>();
                List<string> thirdChoices = new List<string>();
                int start = 0;
                int end = 0;
                if (choice == "character")
                {
                    end = 6;
                }
                else if (choice == "room")
                {
                    start = 6;
                    end = 15;
                }
                else
                {
                    start = 15;
                    end = referenceChoices.Count;
                }
                for (int i = start; i < end; i++) //Get card the AI wants to know about
                {
                    if (!knowns.Contains(referenceChoices[i])) //Add things not known to the list
                        availableChoices.Add(referenceChoices[i]);
                }
                //Select the three guesses
                if (choice == "character")
                {
                    charGuess = Choose(availableChoices);
                    for (int i = 6; i < referenceChoices.Count; i++)
                    {
                        if (marks.Contains(referenceChoices[i])) //The AI only wants what it's looking for, so we add things it has.
                        {
                            if (i < 15)
                                secondChoices.Add(referenceChoices[i]);
                            else
                                thirdChoices.Add(referenceChoices[i]);
                        }
                    }
                    if (secondChoices.Count == 0) //It's possible to get none of a certain type or, in especially rare cases, to get none of two types. In this case choose randomly from all.
                    {
                        for (int x = 6; x < 15; x++)
                            secondChoices.Add(referenceChoices[x]); //Choose randomly from all choices
                    }
                    if (thirdChoices.Count == 0)
                    {
                        for (int x = 15; x < referenceChoices.Count; x++)
                            thirdChoices.Add(referenceChoices[x]); 
                    }
                    roomGuess = Choose(secondChoices);
                    weaponGuess = Choose(thirdChoices);
                }
                else if (choice == "room")
                {
                    roomGuess = Choose(availableChoices);
                    for (int i = 0; i < referenceChoices.Count; i++)
                    {
                        if (marks.Contains(referenceChoices[i])) //The AI only wants what it's looking for, so we add things it has.
                        {
                            if (i < 6)
                                secondChoices.Add(referenceChoices[i]);
                            else if (i > 15)
                                thirdChoices.Add(referenceChoices[i]);
                        }
                    }
                    if (secondChoices.Count == 0) //It's possible to get none of a certain type or, in especially rare cases, to get none of two types. In this case choose randomly from all.
                    {
                        for (int x = 0; x < 6; x++)
                            secondChoices.Add(referenceChoices[x]); //Choose randomly from all choices
                    }
                    if (thirdChoices.Count == 0)
                    {
                        for (int x = 15; x < referenceChoices.Count; x++)
                            thirdChoices.Add(referenceChoices[x]);
                    }
                    charGuess = Choose(secondChoices);
                    weaponGuess = Choose(thirdChoices);
                }
                else
                {
                    weaponGuess = Choose(availableChoices);
                    for (int i = 0; i < 15; i++)
                    {
                        if (marks.Contains(referenceChoices[i])) //The AI only wants what it's looking for, so we add things it has.
                        {
                            if (i < 6)
                                secondChoices.Add(referenceChoices[i]);
                            else
                                thirdChoices.Add(referenceChoices[i]);
                        }
                    }
                    if (secondChoices.Count == 0) //It's possible to get none of a certain type or, in especially rare cases, to get none of two types. In this case choose randomly from all.
                    {
                        for (int x = 6; x < 15; x++)
                            secondChoices.Add(referenceChoices[x]); //Choose randomly from all choices
                    }
                    if (thirdChoices.Count == 0)
                    {
                        for (int x = 15; x < referenceChoices.Count; x++)
                            thirdChoices.Add(referenceChoices[x]);
                    }
                    charGuess = Choose(secondChoices);
                    roomGuess = Choose(thirdChoices);
                    
                }


            }
            if (player2Guessing)
            {
                player2CharacterGuess = charGuess;
                player2WeaponGuess = weaponGuess;
                player2Room = roomGuess;
            }
            else
            {
                player3CharacterGuess = charGuess;
                player3WeaponGuess = weaponGuess;
                player3Room = roomGuess;
            }
            return "They chose the " + roomGuess;
        }


        public static string NPCResult()
        {
            string charGuess = player2CharacterGuess;
            string roomGuess = player2Room;
            string weaponGuess = player2WeaponGuess;
            List<string> knowns = player2Knowns;
            List<string> checkMarks = player3Marks;
            string playerNum = "three"; //It's the opposite because it says who is showing something
            int NPCStrat = player2Strat;
            if (player3Guessing)
            {
                charGuess = player3CharacterGuess;
                roomGuess = player3Room;
                weaponGuess = player3WeaponGuess;
                knowns = player3Knowns;
                checkMarks = player2Marks;
                playerNum = "two";
                NPCStrat = player3Strat;
            }
            string message = "Player " + playerNum + " had something.";
            if (knowns.Contains(roomGuess) && checkMarks.Contains(roomGuess)) //When asked for something already shown, the AI always shows it, meaning nothing is added to the marks.
            {
            }
            else if (knowns.Contains(charGuess) && checkMarks.Contains(charGuess)) //Ditto
            {            
            }
            else if (knowns.Contains(weaponGuess) && checkMarks.Contains(weaponGuess)) //Ditto
            {
            }
            else if (checkMarks.Contains(roomGuess) || checkMarks.Contains(weaponGuess) || checkMarks.Contains(charGuess)) //Asked for something and had it
            {
                if (checkMarks.Contains(roomGuess))
                    knowns.Add(roomGuess);
                else if (checkMarks.Contains(weaponGuess))
                    knowns.Add(weaponGuess);
                else
                    knowns.Add(charGuess);
            } 
            else //Asked for something and didn't have it
            {
                if (player2Guessing && PlayerHasIt(charGuess, weaponGuess, roomGuess) == true) //Player3 had nothing but player does
                {
                    message = "Player three had nothing.";
                    playerShowing = true;
                }
                else if (player2Guessing) //Player must not have it
                {
                    message = "Nobody had anything!";
                    AINothing(2);
                    playerShowing = false;
                }
                else if (player3Guessing) //If it's player three it's already passed the player and two had nothing
                {
                    message = "Nobody had anything!";
                    AINothing(3);
                    playerShowing = false;
                }
            }
            return message;
        }
        public static int AIPersonality()
        {
            return Random.Range(1, 3); //Only two for now. More possible in a future update. Both players could have the same personality.
        }
        public static string Guess(string guessedCharacter, string guessedWeapon) //For the player's guess
        {
            int index1 = -1;
            int index2 = -1;
            int index3 = -1;
            string finalReturn;
            string ret = "";
            List<string> currentMarks;
            List<string> checkMarks;
            string playerNum = "two";
            if (!playerShowing)
            {
                for (int AIShowing = 2; AIShowing < 4; AIShowing++)
                {
                    currentMarks = player2Marks;
                    if (AIShowing == 3)
                    {
                        currentMarks = player3Marks;
                        playerNum = "three";
                    }
                    if (currentMarks.Contains(guessedCharacter) || currentMarks.Contains(guessedWeapon) || currentMarks.Contains(playerRoom))
                    {
                        if (currentMarks.Contains(guessedCharacter))
                        {
                            index1 = currentMarks.IndexOf(guessedCharacter);
                            ret = guessedCharacter;
                        }
                        if (currentMarks.Contains(guessedWeapon))
                        {
                            index2 = currentMarks.IndexOf(guessedWeapon);
                            ret = guessedWeapon;
                        }
                        if (currentMarks.Contains(playerRoom))
                        {
                            index3 = currentMarks.IndexOf(playerRoom);
                            ret = playerRoom;
                        }
                        break;
                    }
                }
                if (index1 != -1 && index2 != -1 && index3 != -1)
                {
                    if (playerMarks.Contains(guessedCharacter)) //If the AI has already shown the card to the player
                    {
                        ret = character;
                    }
                    else if (playerMarks.Contains(guessedWeapon)) 
                    {
                        ret = guessedWeapon;
                    }
                    else if (playerMarks.Contains(playerRoom))
                    {
                        ret = playerRoom;
                    }
                    else //Randomly selects one of the three
                    {
                        ret = Choose(new List<string>() { character, guessedWeapon, playerRoom });
                    }
                }
                if (!ret.Equals(""))
                {
                    addedText[0] = "Player " + playerNum + " had something.";
                    string[] characterList = new string[6] {"Baron Bracken", "Tribune Thumper", "General Giant", "Sheriff Slime", "Foreman Flea", "Lord Lootbug" };
                    if (characterList.Contains(ret)) //Proper grammar is important.
                    {
                        addedText[1] = "You saw " + ret + ".";
                    }
                    else
                    {
                        addedText[1] = "You saw the " + ret + ".";
                    }
                    addedText[2] = "It's been put in your notes.";
                    displayMarks[referenceChoices.IndexOf(ret)] = true;

                }
                else
                {
                    addedText[0] = "Nobody had anything!";
                    addedText[1] = "";
                    addedText[2] = "";
                }
                addedText[3] = "";
                if (player2Elim == false)
                {
                    addedText[4] = "It's player two's turn.";
                    if (AIAccuse(2))
                    {
                        addedText[5] = "Player two is sueing!";
                        addedText[6] = "Their guess was..." + EvaluateAccuse();
                        addedText[7] = "Game Over!";
                        ConsoleGamesMain.playingHint = false;
                        for (int i = 8; i < 18; i++)
                            addedText[i] = "";
                        if (player2Elim == true && player3Elim == false && ConsoleGamesMain.playingHint == true)
                        {
                            player3Guessing = true;
                            player2Guessing = false;
                            ConsoleGamesMain.playingHint = true;
                            addedText[7] = "Player two is eliminated.";
                            addedText[8] = "";
                            addedText[9] = "It's player three's turn.";
                            addedText[10] = NPCGuess() + ".";
                            addedText[11] = "They guess it was";
                            addedText[12] = player3CharacterGuess + " with the";
                            addedText[13] = player3WeaponGuess + ".";
                            if (PlayerHasIt(player3CharacterGuess, player3WeaponGuess, player3Room))
                            {
                                playerShowing = true;
                                addedText[14] = "Show a card.";
                            }
                            else
                            {
                                addedText[14] = "You don't have anything.";
                                addedText[15] = "It's your turn. Select a";
                                addedText[16] = "room to start your guess.";
                            }

                        }
                        else if (player2Elim == true && player3Elim == true)
                        {
                            addedText[7] = "Player two is eliminated.";
                            addedText[8] = "";
                            addedText[9] = "You win by default!";
                            ConsoleGamesMain.playingHint = false;
                        }
                        finalReturn = AssembleHUD(addedText);
                        return finalReturn;
                    }
                    else
                    {
                        player2Guessing = true;
                        player3Guessing = false;
                        addedText[5] = NPCGuess() + "."; //Selects p2's guess
                        addedText[6] = "Player two guesses it was";
                        addedText[7] = player2CharacterGuess + " with the";
                        addedText[8] = player2WeaponGuess + ".";
                        addedText[9] = NPCResult(); //Checks if p3 had it
                    }
                    if (!playerShowing)
                    {
                        player3Guessing = true;
                        player2Guessing = false;
                        addedText[10] = "";
                        addedText[11] = "It's player three's turn.";
                        if (AIAccuse(3))
                        {
                            addedText[12] = "Player three is sueing!";
                            addedText[13] = "Their guess was..." + EvaluateAccuse();
                            addedText[14] = "Game Over!";
                            ConsoleGamesMain.playingHint = false;
                            for (int i = 15; i < 18; i++)
                                addedText[i] = "";
                            if (player3Elim == true && player2Elim == false && ConsoleGamesMain.playingHint == true)
                            {
                                player3Guessing = false;
                                player2Guessing = false;
                                ConsoleGamesMain.playingHint = true;
                                addedText[14] = "Player three is eliminated.";
                                addedText[15] = "";
                                addedText[16] = "It's your turn. Select a";
                                addedText[17] = "room to start your guess.";
                            }
                            else if (player3Elim == true && player2Elim == true)
                            {
                                addedText[14] = "Player three is eliminated.";
                                addedText[15] = "";
                                addedText[16] = "You win by default!";
                            }
                            finalReturn = AssembleHUD(addedText);
                            return finalReturn;
                        }
                        addedText[12] = NPCGuess() + ".";
                        addedText[13] = "They guess that it was";
                        addedText[14] = player3CharacterGuess + " with the";
                        addedText[15] = player3WeaponGuess + ".";
                        addedText[16] = "";
                        if (PlayerHasIt(player3CharacterGuess, player3WeaponGuess, player3Room))
                        {
                            playerShowing = true;
                            addedText[17] = "Show a card.";
                        }
                        else
                        {
                            addedText[16] = NPCResult();
                            addedText[17] = "Select a room for a guess.";
                        }
                    }
                    else
                    {
                        for (int i = 10; i < 18; i++)
                        {
                            addedText[i] = "";
                        }
                        if (playerShowing)
                        {
                            addedText[10] = "Enter something to show.";
                        }
                    }
                }
                else
                {
                    if (!playerShowing)
                    {
                        player3Guessing = true;
                        player2Guessing = false;
                        addedText[10] = "";
                        addedText[11] = "It's player three's turn.";
                        if (AIAccuse(3))
                        {
                            addedText[5] = "Player three is sueing!";
                            addedText[6] = "Their guess was..." + EvaluateAccuse();
                            addedText[7] = "Game Over!";
                            ConsoleGamesMain.playingHint = false;
                            for (int i = 8; i < 18; i++)
                                addedText[i] = "";
                            if (player3Elim == true && player2Elim == false && ConsoleGamesMain.playingHint == true)
                            {
                                player3Guessing = false;
                                player2Guessing = false;
                                addedText[7] = "Player three is eliminated.";
                                addedText[8] = "";
                                addedText[9] = "It's your turn. Select a";
                                addedText[10] = "room to start your guess.";
                                ConsoleGamesMain.playingHint = true;
                            }
                            else if (player3Elim == true && player2Elim == true)
                            {
                                addedText[7] = "Player three is eliminated.";
                                addedText[8] = "";
                                addedText[9] = "You win by default!";
                            }
                            finalReturn = AssembleHUD(addedText);
                            return finalReturn;
                        }
                        addedText[12] = NPCGuess() + ".";
                        addedText[13] = "They guess that it was";
                        addedText[14] = player3CharacterGuess + " with the";
                        addedText[15] = player3WeaponGuess + ".";
                        addedText[16] = "";
                        if (PlayerHasIt(player3CharacterGuess, player3WeaponGuess, player3Room))
                        {
                            playerShowing = true;
                            addedText[17] = "Show a card.";
                        }
                        else
                        {
                            addedText[16] = NPCResult();
                            addedText[17] = "Select a room for a guess.";
                        }
                    }
                    else
                    {
                        for (int i = 10; i < 18; i++)
                        {
                            addedText[i] = "";
                        }
                        if (playerShowing)
                        {
                            addedText[10] = "Enter something to show.";
                        }
                    }
                }
                finalReturn = AssembleHUD(addedText);
                return finalReturn;
            }
            else
            {
                return MustShow();
            }
        }
        public static string NoGuess()
        {
            addedText[0] = "There was an error with your";
            addedText[1] = "guess. Please try again.";
            addedText[2] = "(Guess 'person', 'weapon')";
            addedText[3] = "";
            for (int i = 4; i < 18; i++)
            {
                addedText[i] = "";
            }
            return AssembleHUD(addedText);
        }

        public static void AINothing(int whoIsGuessing) //For when nobody has anything after an AI's guess. Eliminates all other options.
        {
            List<string> knowns; //Using knowns vs marks is irrelevant because it has to be both; nobody had anything.
            string charaGuess;
            string roomGuess;
            string weaponGuess;
            if (whoIsGuessing == 2)
            {
                knowns = player2Knowns;
                charaGuess = player2CharacterGuess;
                roomGuess = player2Room;
                weaponGuess = player2WeaponGuess;
            }
            else
            {
                knowns = player3Knowns;
                charaGuess = player3CharacterGuess;
                roomGuess = player3Room;
                weaponGuess = player3WeaponGuess;
            }
            bool hasChar = false;
            bool hasRoom = false;
            bool hasWeapon = false;
            if (knowns.Contains(charaGuess))
                hasChar = true;
            if (knowns.Contains(roomGuess))
                hasRoom = true;
            if (knowns.Contains(weaponGuess))
                hasWeapon = true;
            List<bool> tempList = new List<bool>() { hasChar, hasRoom, hasWeapon};
            for (int i = 0; i < tempList.Count; i++)
            {
                if (tempList[i] == false)
                {
                    if (i == 0)
                    {
                        for (int x = 0; x < 6; x++)
                        {
                            if (charaGuess != referenceChoices[x])
                                knowns.Add(referenceChoices[x]);
                        }
                    }
                    else if (i == 1)
                    {
                        for (int x = 6; x < 15; x++)
                        {
                            if (roomGuess != referenceChoices[x])
                                knowns.Add(referenceChoices[x]);
                        }
                    }
                    else
                    {
                        for (int x = 15; x < referenceChoices.Count; x++)
                        {
                            if (weaponGuess != referenceChoices[x])
                            {
                                knowns.Add(referenceChoices[x]);
                            }
                        }
                    }
                }
            }      
        }

        internal static string MustShow()
        {
            string finalReturn = "You have to show a card!\n";
            if (player2Guessing)
            {
                addedText[0] = "Player two guessed it was ";
                addedText[1] = player2CharacterGuess + " with the";
                addedText[2] = player2WeaponGuess;
                addedText[3] = "in the " + player2Room;
            }
            else
            {
                addedText[0] = "Player three guessed it was ";
                addedText[1] = player3CharacterGuess + "with the";
                addedText[2] = player3WeaponGuess;
                addedText[3] = "in the " + player3Room;
            }
            addedText[4] = "Enter something to show.";
            for (int i = 5; i < 18; i++)
            {
                addedText[i] = "";
            }
            finalReturn += AssembleHUD(addedText);
            return finalReturn;
        }
        internal static string[] DisplayHUD(bool[] marks) //Draw board and notebook.
        {
            string[] temp = {
                             "╔═══════════════════════╗      ╔════════════════╗", //Template
                             "║ [Pool]          [Bar] ║      ║  Notes         ║",
                             "║        [Galley]       ║      ║────────────────║",
                             "║                       ║      ║ B. Bracken │ │ ║",
                             "║ [Cellar]      [Lobby] ║      ║ L. Lootbug │ │ ║",
                             "║                       ║      ║ T. Thumper │ │ ║",
                             "║ [Canteen]             ║      ║ G. Giant   │ │ ║",
                             "║                       ║      ║ S. Slime   │ │ ║",
                             "║ [Gameroom]  [Balcony] ║      ║ F. Flea    │ │ ║",
                             "║                       ║      ║────────────────║",
                             "║        [Office]       ║      ║ Pool       │ │ ║",
                             "╚═══════════════════════╝      ║ Bar        │ │ ║",
                             "                               ║ Galley     │ │ ║",
                             "                               ║ Cellar     │ │ ║",
                             "                               ║ Lobby      │ │ ║",
                             "                               ║ Canteen    │ │ ║",
                             "                               ║ Gameroom   │ │ ║",
                             "                               ║ Balcony    │ │ ║",
                             "                               ║ Office     │ │ ║",
                             "                               ║────────────────║",
                             "                               ║ Flail      │ │ ║",
                             "                               ║ Dagger     │ │ ║",
                             "                               ║ Bat        │ │ ║",
                             "                               ║ Shotgun    │ │ ║",
                             "                               ║ Shovel     │ │ ║",
                             "                               ║ Landmine   │ │ ║",
                             "                               ╚════════════════╝",
                             "                                                 ",
                             "                                                 ",
                             "                                                 "

                            };
            int index = 0;
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i].Substring(44, 3).Equals("│ │"))
                {
                    if (marks[index])
                    {
                        if (playerMarks.Contains(referenceChoices[index]))
                            temp[i] = temp[i].Substring(0, 45) + "X" + temp[i].Substring(46); //Places an X in the spot where the space used to be, player's card
                        else
                            temp[i] = temp[i].Substring(0, 45) + "T" + temp[i].Substring(46); //Places a T in the spot where the space used to be, AI's card
                    }
                    index++;
                }
            }
            return temp;
        }

        public static string AssembleHUD(string[] addedText) //Adds the text under the board to the HUD.
        {
            string[] temp = DisplayHUD(displayMarks);
            string ret = "";
            if (temp != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    ret += temp[i] + "\n";
                }

                for (int i = 12; i < 30; i++)
                {
                    if (addedText.Length > i - 12)
                    {
                        ret += addedText[i - 12];
                        for (int x = addedText[i - 12].Length; x < 29; x++)
                        {
                            ret += " ";
                        }
                    }

                    if (temp[i] != null && temp[i].Length > 29)
                    {
                        ret += temp[i].Substring(29) + "\n";
                    }
                }
                return ret;
            }
            else
            {
                Debug.Log("Temp is null!");
                return "You done messed up, fool!";
            }
        } 
        public static string AssembleHUD() //Used to put the rows of the array together to form the image.
        {
            string[] temp = DisplayHUD(displayMarks);
            string ret = "";
            for (int i = 0; i < 28; i++)
            {
                ret += temp[i] + "\n";
            }
            return ret;
        }

        public static string EvaluateAccuse()
        {
            string ret = "incorrect!";
            if (realHints.Contains(accuseChara) && realHints.Contains(accuseWeapon) && realHints.Contains(accuseRoom))
            {
                ret = "correct!";
                ConsoleGamesMain.playingHint = false;
            }
            if (player2Guessing)
            {
                player2Elim = true;
                player2Guessing = false;
            }
            else
            {
                player3Elim = true;
                player3Guessing = false;
            }
            return ret;
        }

        public static bool PlayerHasIt(string guessedCharacter, string guessedWeapon, string guessedRoom)
        {
            if (playerMarks.Contains(guessedCharacter))
            {
                return true;
            }
            else if (playerMarks.Contains(guessedWeapon))
            {
                return true;
            }
            else if (playerMarks.Contains(guessedRoom))
            {
                return true;
            }
            return false;
        }

        public static bool AIAccuse(int playerNumber) //Only valid playerNumbers are 2 and 3
        {
            int personality;
            int knownsAtCharacter = 0;
            int knownsAtWeapon = 0;
            int knownsAtRoom = 0;
            string guessCharacter = "";
            string guessWeapon = "";
            string guessRoom = "";
            List<string> knowns = new List<string>();
            if (playerNumber == 2)
            {
                personality = player2Strat;
                knowns.AddRange(player2Knowns);
                player2Guessing = true;
            }
            else
            {
                personality = player3Strat;
                knowns.AddRange(player3Knowns);
                player3Guessing = true;
            }
            //Information gathering used by all personalities
            for (int i = 0; i < 6; i++)
            {
                if (knowns.Contains(referenceChoices[i]))
                {
                    knownsAtCharacter++;
                }
                else
                {
                    guessCharacter = referenceChoices[i];
                }
            }
            for (int i = 6; i < 15; i++)
            {
                if (knowns.Contains(referenceChoices[i]))
                {
                    knownsAtRoom++;
                }
                else
                {
                    guessRoom = referenceChoices[i];
                }
            }
            for (int i = 15; i < referenceChoices.Count; i++)
            {
                if (knowns.Contains(referenceChoices[i]))
                {
                    knownsAtWeapon++;
                }
                else
                {
                    guessWeapon = referenceChoices[i];
                }
            }

            int numKnowns = 3;
            if (knownsAtCharacter != 5) //Tracks which ones are 100% known by keeping their correct guess.
            {
                guessCharacter = "";
                numKnowns--;
            }
            if (knownsAtRoom != 8)
            { 
                guessRoom = "";
                numKnowns--;
            }
            if (knownsAtWeapon != 5)
            {
                guessWeapon = "";
                numKnowns--;
            }
            int numUnknown = 0;
            if (knownsAtCharacter == 4) //Tracks how many 50/50s there are.
                numUnknown += 1;
            if (knownsAtRoom == 7)
                numUnknown += 1;
            if (knownsAtWeapon == 4)
                numUnknown += 1;
            if (personality == 1) //Random personality. Will only accuse once they have it down to at least 2 for sure and one 50/50. Element of randomness in if they accuse even under those circumstances
            {
                if (numKnowns == 3) //Has all three, automatically accuses.
                {
                    accuseChara = guessCharacter;
                    accuseWeapon = guessWeapon;
                    accuseRoom = guessRoom;
                    return true;
                }
                int seed = Random.RandomRange(0, 100);
                if (numUnknown == 1 && numKnowns == 2 && seed > 66) //When it's a 50/50 one-third of the time it will guess
                {
                    string option1 = "";
                    string option2 = "";
                    int start = 0;
                    int end = 0;
                    if (knownsAtCharacter != 6)
                    {
                        start = 0;
                        end = 6;
                    }
                    else if (knownsAtRoom != 9)
                    {
                        start = 6;
                        end = 15;
                    }
                    else
                    {
                        start = 15;
                        end = referenceChoices.Count;
                    }
                    for (int i = start; i < end; i++)
                    {
                        if (!knowns.Contains(referenceChoices[i]))
                        {
                            if (option1 == "")
                                option1 = referenceChoices[i];
                            else
                            {
                                option2 = referenceChoices[i];
                                break;
                            }
                        }
                    }
                    guessCharacter = Choose(new List<string>() { option1, option2 });
                    accuseChara = guessCharacter;
                    accuseWeapon = guessWeapon;
                    accuseRoom = guessRoom;
                    return true;
                }
            }
            else if (personality == 2) //Focused personality. Will guess only when it has all three, or if it has been more than 15 turns it has a chance to guess on one 50/50.
            {
                if (numKnowns == 3) //Has all three, automatically accuses.
                {
                    accuseChara = guessCharacter;
                    accuseWeapon = guessWeapon;
                    accuseRoom = guessRoom;
                    return true;
                }
                int seed = Random.RandomRange(0, 100);
                if (numUnknown == 1 && numKnowns == 2 && seed > 50 && turns >= 12) //When it's a 50/50 and it has been 12 turns it will take a guess half the time.
                {
                    string option1 = "";
                    string option2 = "";
                    int start = 0;
                    int end = 0;
                    if (knownsAtCharacter != 6)
                    {
                        start = 0;
                        end = 6;
                    }
                    else if (knownsAtRoom != 9)
                    {
                        start = 6;
                        end = 15;
                    }
                    else
                    {
                        start = 15;
                        end = referenceChoices.Count;
                    }
                    for (int i = start; i < end; i++)
                    {
                        if (!knowns.Contains(referenceChoices[i]))
                        {
                            if (option1 == "")
                                option1 = referenceChoices[i];
                            else
                            {
                                option2 = referenceChoices[i];
                                break;
                            }
                        }
                    }
                    guessCharacter = Choose(new List<string>() { option1, option2 });
                    accuseChara = guessCharacter;
                    accuseWeapon = guessWeapon;
                    accuseRoom = guessRoom;
                    return true;
                }
            }
            return false;
        }

        public static string FindMost() //Method to find what category the AI has the most of. By percentage, not count.
        {
            string retString = "";
            List<string> marks = player2Knowns;
            if (player3Guessing)
                marks = player3Knowns;
            int numChara = 0;
            int numWep = 0;
            int numRoom = 0;
            for (int i = 0; i < referenceChoices.Count; i++)
            {
                if (marks.Contains(referenceChoices[i]))
                {
                    if (i < 6)
                        numChara++;
                    else if (i < 15)
                        numRoom++;
                    else
                        numWep++;
                }
            }
            float charaPercentage = (float)numChara / 5;
            float roomPercentage = (float)numRoom / 8;  //All are minus one because the AI is left with one unmarked, that being the correct one.
            float wepPercentage = (float)numWep / 5;

            if (charaPercentage == 1)
                charaPercentage = 0;
            if (roomPercentage == 1) 
                roomPercentage = 0; //We don't want to target categories that are solved, so we set it to 0.
            if (wepPercentage == 1)
                wepPercentage = 0;

            if (charaPercentage > roomPercentage && charaPercentage > wepPercentage)
            {
                retString = "character";
            }
            else if (roomPercentage > wepPercentage && roomPercentage > charaPercentage)
            {
                retString = "room";
            }
            else
            {
                retString = "weapon";
            }

            return retString;
        }

        internal static string FormatCharacter(string input)
        {
            string returnValue = "";
            switch (input)
            {
                case "baron bracken":
                case "bracken":
                    returnValue = "Baron Bracken";
                    break;
                case "lord lootbug":
                case "lootbug":
                    returnValue = "Lord Lootbug";
                    break;
                case "tribune thumper":
                case "thumper":
                    returnValue = "Tribune Thumper";
                    break;
                case "general giant":
                case "giant":
                    returnValue = "General Giant";
                    break;
                case "sheriff slime":
                case "slime":
                    returnValue = "Sheriff Slime";
                    break;
                case "foreman flea":
                case "flea":
                    returnValue = "Foreman Flea";
                    break;
                default: 
                    returnValue = input;
                    break;
            }
            return returnValue;
        }

        internal static string Choose(List<string> choices) //Thanks for the idea, GML.
        {
            int index = Random.Range(0, choices.Count);
            return choices[index];
        } 

        internal static string Choose(List<string> choices, string type)
        {
            int index = -1;
            if (type.Equals("character"))
            {
                index = Random.Range(0, 6);
            }
            else if (type.Equals("room"))
            {
                index = Random.Range(6, 15);
            }
            else
            {
                index = Random.Range(15, choices.Count);
            }
            return choices[index];
        }
    }
}
