using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MammalOS.API;
using MammalOS.API.Tools;
using System.Collections;
using UnityEngine;
using static TerminalApi.TerminalApi;
using TerminalApi.Events;
using static TerminalApi.Events.Events;
using SimpleCommand.API.Classes;
using static SimpleCommand.API.SimpleCommand;
using System.Collections.Generic;
using static Steamworks.InventoryItem;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using System.Net;
using UnityEngine.Windows;
using static TerminalGames.CardClasses;
using System.ComponentModel;
using System.Linq;
using static ConsoleGames.DiceClasses;
using UnityEngine.InputSystem.HID;
using Steamworks;
using System.Security.Principal;



namespace TerminalGames
{
    //Begin TerminalGames patch
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("atomic.terminalapi", MinimumDependencyVersion: "1.2.0")]
    [BepInDependency(SimpleCommand.API.MyPluginInfo.PLUGIN_GUID)]
    public class ConsoleGamesMain : BaseUnityPlugin
    {
        //Initialize vars for BepIn
        private const string modGUID = "Mammal.TerminalGames";
        private const string modName = "Terminal_Games";
        private const string modVersion = "1.1.0";
        //Initializing patch vars
        private static ConsoleGamesMain Instance;
        internal ManualLogSource logSource;
        private readonly Harmony harmony = new Harmony(modGUID);
        //Vars for gamestates
        public static bool bettingBJ = false;
        public static bool playingBJ = false;
        public static bool playingNumbers = false;
        public static bool playingGoFish = false;
        public static bool bettingDice = false;
        public static bool playingDice = false;
        //Vars for BJ
        public static List<string> bjWords = new List<string>() { "blackjack", "bj", "black", "bet", "confirm", "stand" };
        public static int currentBet = 0;
        public static List<BJCard> dealerBJCards = new List<BJCard>();
        public static List<BJCard> playerBJCards = new List<BJCard>();
        //Vars for Go Fish
        public static string finalDisplay;
        public static List<OneDeckCard> dealerODCards = new List<OneDeckCard>();
        public static List<OneDeckCard> playerODCards = new List<OneDeckCard>();
        public static int playerBooks = 0;
        public static int dealerBooks = 0;
        public static int lastCardAsked = -1;
        public static DeckOfCards deck;
        public static List<string> cardNames = new List<string>() { "ace", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "jack", "queen", "king" };
        //Vars for Numbers
        public static int guessableNumber;
        public static int tries = 0;
        //Vars for Dice
        public static List<Dice> playerDice = new List<Dice>();
        public static List<Dice> dealerDice = new List<Dice>();
        public static List<string> listString = new List<string>();
        public static int lastBidValue = 0;
        public static int lastBidCount = 0;

        //Stat vars
        public static int bjGames;
        public static int bjWins;
        public static int numbersGames;
        public static int numbersWins;
        public static int averageGuesses;
        public static int fishGames;
        public static int fishWins;
        public static int averageBooks;
        public static int diceGames;
        public static int diceWins;
        public static int creditEarnings;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            logSource = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            logSource.LogInfo($"Plugin {modName} is loaded!");
            TerminalParsedSentence += TextSubmitted;
            TerminalExited += OnTerminalExit;
            //Begin gambling modules
            SimpleCommandModule betConfirmed = new SimpleCommandModule()
            {
                DisplayName = "continue",
                Description = "Begin the game.",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = BetConfirmed,
            };

            SimpleCommandModule bet = new SimpleCommandModule()
            {
                DisplayName = "bet",
                Description = "Choose bet ammount. Type 'bet' followed by your bet.",
                HasDynamicInput = true,
                Arguments = new string[] { "'bet'", "number of credits" },
                HideFromCommandList = true,
                Method = ConfirmBet,
            };

            SimpleCommandModule betDenied = new SimpleCommandModule()
            {
                DisplayName = "back",
                Description = "Do not begin the game.",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = DenyBet,
            };
            //Begin Dice modules
            SimpleCommandModule diceBegin = new SimpleCommandModule()
            {
                DisplayName = "liarsdice",
                Description = "outwit a computer, win some moolah.",
                HasDynamicInput = false,
                Abbreviations = new string[] { "dice", "liars" },
                HideFromCommandList = true,
                Method = BeginDice,
            };
            SimpleCommandModule diceBid = new SimpleCommandModule()
            {
                DisplayName = "bid",
                Description = "outwit a computer, win some moolah.",
                HasDynamicInput = true,
                Arguments = new string[] {"'bid'", "word count", "number value" },
                HideFromCommandList = true,
                Method = BidEntered,
            };
            SimpleCommandModule diceAccept = new SimpleCommandModule()
            {
                DisplayName = "accept",
                Description = "you believe the bid",
                HasDynamicInput = false,
                HideFromCommandList = true,
                Method = DiceAccept,
            };
            SimpleCommandModule diceChallenge = new SimpleCommandModule()
            {
                DisplayName = "challenge",
                Description = "you don't believe the bid.",
                HasDynamicInput = false,
                HideFromCommandList = true,
                Method = DiceChallenge,
            };
            //Begin BlackJack modules
            SimpleCommandModule blackjackStand = new SimpleCommandModule()
            {
                DisplayName = "stand",
                Description = "get no more cards",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = StandBlackJack,
            };

            SimpleCommandModule blackjackHit = new SimpleCommandModule()
            {
                DisplayName = "hit",
                Description = "get another card",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = HitBlackJack,
            };

            SimpleCommandModule blackjack = new SimpleCommandModule()
            {
                DisplayName = "blackjack",
                Description = "Begins blackjack.",
                HasDynamicInput = false,
                Abbreviations = new string[] { "bj", "black" },
                HideFromCommandList = true,
                Method = BetBlackjack,
            };
            //End BlackJack modules

            //Begin Numbers modules
            SimpleCommandModule beginNumbers = new SimpleCommandModule()
            {
                DisplayName = "numbers",
                Description = "Can you guess the number?",
                HasDynamicInput = false,
                Abbreviations = null,
                HideFromCommandList = true,
                Method = BeginNumbers,
            };

            SimpleCommandModule guessNumbers = new SimpleCommandModule()
            {
                DisplayName = "guess",
                Description = "Can you guess the number?",
                HasDynamicInput = true,
                Arguments = new string[] { "'guess'", "number to guess" },
                Abbreviations = null,
                HideFromCommandList = true,
                Method = GuessNumbers,
            };
            //End Numbers modules

            //Begin misc modules
            SimpleCommandModule gameList = new SimpleCommandModule()
            {
                DisplayName = "list",
                Description = "Lists available games.\nType list for game list.",
                HasDynamicInput = false,
                Abbreviations = null,
                Method = ListGames,
            };
            SimpleCommandModule statsScreen = new SimpleCommandModule()
            {
                DisplayName = "stats",
                Description = "Stats from your games played.\nType stats for game stats.",
                HasDynamicInput = false,
                Abbreviations = new string[] { "stat" },
                HideFromCommandList = false,
                Method = ListStats,

            };

            SimpleCommandModule rulesScreen = new SimpleCommandModule()
            {
                DisplayName = "rules",
                Description = "displays the rules to a game",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Abbreviations = null,
                Method = RulesScreen,
            };
            //End misc modules

            //Begin Go Fish modules
            SimpleCommandModule goFish = new SimpleCommandModule()
            {
                DisplayName = "gofish",
                Description = "Play the classic family card game. Got any sevens?",
                Abbreviations = new string[] { "fish", "gofish" },
                HasDynamicInput = false,
                HideFromCommandList = true,
                Method = BeginGoFish,
            };

            SimpleCommandModule requestGoFish = new SimpleCommandModule()
            {
                DisplayName = "request",
                Description = "attempt to take a card",
                Arguments = new string[] { "request", "card" },
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = RequestGoFish,
            };



            AddSimpleCommand(gameList);
            AddSimpleCommand(blackjack);
            AddSimpleCommand(bet);
            AddSimpleCommand(betConfirmed);
            AddSimpleCommand(betDenied);
            AddSimpleCommand(blackjackHit);
            AddSimpleCommand(blackjackStand);
            AddSimpleCommand(beginNumbers);
            AddSimpleCommand(guessNumbers);
            AddSimpleCommand(goFish);
            AddSimpleCommand(requestGoFish);
            AddSimpleCommand(diceBegin);
            AddSimpleCommand(diceBid);
            AddSimpleCommand(diceAccept);
            AddSimpleCommand(diceChallenge);
            AddSimpleCommand(rulesScreen);
            AddSimpleCommand(statsScreen);
            harmony.PatchAll(typeof(ConsoleGamesMain));

        }
        //Begin misc methods
        public static TerminalNode ListGames(Terminal __terminal)
        {
            TerminalNode listNode = CreateTerminalNode(">BlackJack\nGamble your credits! Play responsibly.\n\n>Numbers\nCan you guess a number 1-1000 in 10 or less guesses?\n\n>GoFish\nAll of the fishing, none of the going. Got any sevens?\n\n>LiarsDice\nCan you bluff your way to victory?\n\nFor rules to any game, type 'rules [gamename]', ex. 'rules gofish'.\n\n");
            listNode.clearPreviousText = true;
            return listNode;
        }

        public static TerminalNode ListStats(Terminal __terminal)
        {
            TerminalNode returnNode;
            PlayerPrefs.Save(); //Save stats before referencing them. 
            bjGames = PlayerPrefs.GetInt("BJGames", 0);
            bjWins = PlayerPrefs.GetInt("BJWins", 0);
            numbersGames = PlayerPrefs.GetInt("NumGames", 0);
            numbersWins = PlayerPrefs.GetInt("NumWins", 0);
            averageGuesses = PlayerPrefs.GetInt("AvgNumGuess", 0);
            fishGames = PlayerPrefs.GetInt("FishGames", 0);
            fishWins = PlayerPrefs.GetInt("FishWins", 0);
            averageBooks = PlayerPrefs.GetInt("AvgBooks", 0);
            diceGames = PlayerPrefs.GetInt("DiceGames", 0);
            diceWins = PlayerPrefs.GetInt("DiceWins", 0);
            int allGames = bjGames + numbersGames + fishGames + diceGames;
            creditEarnings = PlayerPrefs.GetInt("Earnings", 0);

            if (allGames != 0)
            {
                
                returnNode = CreateTerminalNode("BlackJack Stats:\n      Games Played: " + bjGames + "\n      Games Won: " + bjWins +
                    "\n\nNumbers Stats:\n      Games Played: " + numbersGames + "\n      " +
                    "Games Won: " + numbersWins + "\n      Average Guesses: " + averageGuesses/numbersGames + "\n\nGo Fish Stats:\n      " +
                    "Games Played: " + fishGames + "\n      Games Won: " + fishWins + "\n      Average Books: " + averageBooks/fishGames +
                    "\n\nDice Stats:\n      Games Played: " + diceGames + "\n      Games Won:" + diceWins + "\n\nGeneral Stats:\n" +
                    "      Games Played: " + allGames + "\n      Credits Earned: " + creditEarnings + "\n\n");
            }
            else
            {
                returnNode = CreateTerminalNode("You have no stats! Play a game and create some by typing 'list'.\n\n");
            }
            returnNode.clearPreviousText = true;
            return returnNode;

        }

        public static TerminalNode RulesScreen(Terminal __terminal)
        {
            string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
            input = input.Substring(6); //Everything after 'rules '
            input = input.ToLower();
            TerminalNode returnNode;
            if (input.Equals("liarsdice"))
            {
                returnNode = CreateTerminalNode("The object of the game is to be the last player with dice remaining.\nBoth opponents roll their five dice, and hide the " +
                    "result from each other. Starting with the player, the opponents take turns 'bidding' at least how many of a certain number they think has been rolled. " +
                    "If their opponent believes them, they can 'accept' the bid, and make a bid of their own which must be higher in number or value than the previous bet (for example if " +
                    "the last bet was 'three 4s', the next bet must be 'four 4s' or greater or 'three 5s' or greater).\nIf their opponent thinks they are lying, they can 'challenge' " +
                    "the bid. After a challenge all dice are revealed. If the bidder was incorrect, they lose a die. If the challenger was incorrect, they lose a die.\nPlay continues " +
                    "until only one player has dice remaining.\nCan you keep your dice in your hands?\n\n");
            }
            else if (input.Equals("numbers"))
            {
                returnNode = CreateTerminalNode("The object of the game is to guess a random number from 1 to 1000.\nAfter each guess the computer will " +
                    "tell the player if their last guess was too high, or too low. Can you guess the correct number in ten tries or less?\n\n");
            }
            else if (input.Equals("blackjack"))
            {
                returnNode = CreateTerminalNode("The object of the game is to reach a card total close to 21 without exceeding it.\nPlayers decide whether to 'hit' for an additional " +
                    "card or 'stand' to avoid surpassing 21. The risk of going over 21 accompanies the decision to hit, while standing may mean not reaching a winning total. " +
                    "\nPlayers face off against the dealer, who draws until reaching at least 17, then stands. The player loses if the dealer surpasses their total but wins if " +
                    "the dealer goes over 21 or if the player's total exceeds the dealer's.\nDo you have what it takes to beat the house?\n\n");
            }
            else if (input.Equals("gofish")) //Who tf needs rules for go fish? I'll add it so that all the games have rules, but I hope it's never needed.
            {
                returnNode = CreateTerminalNode("The object of the game is to complete the most sets of the same four cards(four aces, four twos, four jack, etc), called books.\n " +
                    "On the player's turn they can request a card from their opponent. If their opponent has one or more of that card, they must give all of them to the player. " +
                    "On their turn, the opponent can also do this. If the requested does not have the card being asked for, the requester must draw a card.\nThe game ends once all " +
                    "cards have been turned in as books. Whoever has more books when all cards have been played wins.\nCan you fish up a win in this strategic card game?\n\n");
            }
            else
            {
                returnNode = CreateTerminalNode("There was an issue with your request.\n\n");
            }
            returnNode.clearPreviousText = true;
            return returnNode;
        }
        //End misc methods

        //Begin Liars Dice methods
        public static TerminalNode BeginDice(Terminal __terminal)
        {
            bettingDice = true;
            TerminalNode returnNode = CreateTerminalNode("How good's your bluffing skills?\nEnter how much you'd like to bet by typing 'bet [amount]'.\n\n");
            returnNode.clearPreviousText = true;
            return returnNode;
        }

        public static TerminalNode BidEntered(Terminal __terminal)
        {
            int guess;
            string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
            finalDisplay = "";
            input = input.ToLower();
            input = input.Substring(4);
            if (playingDice)
            {
                try
                {
                    if (input.Substring(0, 3).Equals("one") || input.Substring(0, 3).Equals("two") || input.Substring(0, 3).Equals("six") || input.Substring(0, 3).Equals("ten"))
                    {
                        guess = int.Parse(input.Substring(4, 1));
                        input = input.Substring(0, 3);
                    }
                    else if (input.Substring(0, 4).Equals("four") || input.Substring(0, 4).Equals("five") || input.Substring(0, 4).Equals("nine"))
                    {
                        guess = int.Parse(input.Substring(5, 1));
                        input = input.Substring(0, 4);
                    }
                    else if (input.Substring(0, 5).Equals("three") || input.Substring(0, 5).Equals("seven") || input.Substring(0, 5).Equals("eight"))
                    {
                        guess = int.Parse(input.Substring(6, 1));
                        input = input.Substring(0, 5);
                    }
                    else
                    {
                        finalDisplay = "Something went wrong. Please enter your bid (format 'bid [amount] [value]s'. Ex. {'bid two 1s'})\n\n";
                        TerminalNode rejectNode = CreateTerminalNode(finalDisplay);
                        rejectNode.clearPreviousText = true;
                        return rejectNode;
                    }
                    if (guess > lastBidValue || TranslateString(input) > lastBidCount)
                    {
                        finalDisplay = "You bid that there were at least " + input + " " + guess + "s.\n";
                        lastBidCount = TranslateString(input);
                        lastBidValue = guess;
                        DiceOpponentResponse(guess, input);
                    }
                    else 
                    {
                        finalDisplay = "Your bid must be higher than the previous bid. Please enter a new bid (format 'bid [amount] [value]s'. Ex. {'bid two 1s'})\n\n";
                        TerminalNode failNode = CreateTerminalNode(finalDisplay);
                        failNode.clearPreviousText = true;
                        return failNode;
                    }
                }
                catch (System.FormatException)
                {
                    finalDisplay = "Something went wrong. Please enter your bid (format 'bid [amount] [value]s'. Ex. {'bid two 1s'})\n\n";
                }
            }
            else
            {
                finalDisplay = "There was an issue with your request.\n\n";
            }
            TerminalNode returnNode = CreateTerminalNode(finalDisplay);
            returnNode.clearPreviousText = true;
            return returnNode;
        }

        public static TerminalNode DiceAccept(Terminal __terminal)
        {
            string text = "You accepted the bid. It's your turn, enter your bid (format 'bid [amount] [value]s'. Ex. {'bid two 1s'})\nYour roll: \n";
            List<int> delayDisplays = new List<int>();
            for (int i = 0; i < playerDice.Count; i++)
            {
                delayDisplays.Add(playerDice[i].faceValue);
            }
            text += DisplayDice(delayDisplays);
            text += "\n";
            TerminalNode returnNode = CreateTerminalNode(text);
            returnNode.clearPreviousText = true;
            return returnNode;
        }

        public static TerminalNode DiceChallenge(Terminal __terminal)
        {
            finalDisplay = "You challenged the bid! Time to show the dice.\n";
            HandleChallenge(false , lastBidValue, TranslateInt(lastBidCount));
            TerminalNode returnNode = CreateTerminalNode(finalDisplay);
            returnNode.clearPreviousText = true;
            return returnNode;
        }
        public static void DiceOpponentResponse(int value, string count) 
        {
            bool choice = ShouldChallenge(value, count);
            if (!choice)
            {
                DiceOpponentBid(false);
            }
            else
            {
                HandleChallenge(true, value, count);
            }
        }

        public static bool ShouldChallenge(int value, string count)
        {
            int expectedDice = 0;
            for (int i = 0; i < dealerDice.Count; i++)
            {
                if (dealerDice[i].faceValue == value) //Finds how many of that number the dealer has
                {
                    expectedDice++;
                }
            }
            if (playerDice.Count > 3) //Gives the benefit of the doubt to promote less challenges.
                expectedDice++;
            if (playerDice.Count == 5)
                expectedDice++;
            if (expectedDice < TranslateString(count)) //If the expected count of that number is less than what is bid, challenge.
                return true;
            return false;  //If not, accept.
        }

        public static void CalculateBid(out int value, out int count, bool firstTurn)
        {
            int choiceSeed = Random.Range(1, 11);      //Most of the time it is based on what the CPU has, but the CPU bluffs now and then, and is 
            int bluffThreshold;                        //more likely to bluff if the player has fewer dice. The CPU plays agressively.
            if (playerDice.Count == 1) 
                bluffThreshold = 5;
            else if (playerDice.Count > 1 && playerDice.Count <= 3)
                bluffThreshold = 7;
            else bluffThreshold = 10;
            if (firstTurn && choiceSeed > bluffThreshold) //When bluffing on the first turn the cpu chooses smaller counts.
            {
                value = Random.Range(1, 7);
                count = Random.Range(1, 4);
            }
            else if (choiceSeed > bluffThreshold) //CPU is bluffing(choosing randomly).
            {
                value = Random.Range(1, 7);
                if (lastBidCount + 1 < dealerDice.Count)
                {
                    if (value > lastBidValue)
                    {
                        count = Random.Range(lastBidCount, dealerDice.Count);
                    }
                    else
                    {
                        count = Random.Range(lastBidCount + 1, dealerDice.Count);
                    }
                }
                else
                {
                    if (value > lastBidValue)
                    {
                        count = Random.Range(lastBidCount, playerDice.Count + dealerDice.Count);
                    }
                    else
                    {
                        count = Random.Range(lastBidCount + 1, playerDice.Count + dealerDice.Count);
                    }
                }

            }
            else  //CPU is not bluffing.
            {
                List<int> values = new List<int>();
                for (int i = 0; i < dealerDice.Count; i++)
                {
                    values.Add(dealerDice[i].faceValue);
                }
                var filteredNumbers = values
                    .GroupBy(num => num)
                    .Where(group => group.Count() > lastBidCount)
                    .Select(group => group.Key);
                int bestBid = filteredNumbers
                    .OrderBy(num => values.Count(n => n == num))
                    .FirstOrDefault();
                if (bestBid > 0) //If the CPU is in a position where it can play without bluffing
                {
                    value = bestBid;
                    count = lastBidCount + 1;
                }
                else //If not, the CPU picks a random number that is valid 
                {
                    value = Random.Range(1, 7);
                    if (value > lastBidValue)
                        count = Random.Range(lastBidCount, dealerDice.Count);
                    else
                    {
                        if (dealerDice.Count > lastBidCount + 1)
                            count = Random.Range(lastBidCount + 1, dealerDice.Count);
                        else
                            count = Random.Range(lastBidCount + 1, dealerDice.Count + playerDice.Count);
                    }
                    if (count < 1)
                        count = lastBidCount + 1;
                }
            }
        }

        public static void DiceOpponentBid(bool firstTurn) 
        {
            string betCountRep;
            int betValue;
            int betCount;
            CalculateBid(out betValue, out betCount, firstTurn);
            lastBidCount = betCount;           
            lastBidValue = betValue;
            betCountRep = TranslateInt(betCount);
            if (firstTurn == false)
                finalDisplay += "Your opponent accepted it.\n";
            finalDisplay += "Your opponent bid that there are at least " + betCountRep + " " + betValue + "s.\nDo you accept the bid, or do you challenge it?\nYour dice:\n";
            List<int> delayDisplays = new List<int>();
            for (int i = 0; i < playerDice.Count; i++)
            {
                delayDisplays.Add(playerDice[i].faceValue);
            }
            finalDisplay += DisplayDice(delayDisplays);
            finalDisplay += "\n";
        }

        public static void HandleChallenge(bool isDealer, int value, string count)
        {
            List<Dice> allDice = new List<Dice>();
            List<int> delayPlayerDisplay = new List<int>();
            List<int> delayDealerDisplay = new List<int>();
            for (int i = 0; i < playerDice.Count; i++)
            {
                delayPlayerDisplay.Add(playerDice[i].faceValue);
            }
            for (int x = 0; x < dealerDice.Count; x++)
            {
                delayDealerDisplay.Add(dealerDice[x].faceValue);
            }
            int num = 0;
            int intCount;
            allDice.AddRange(playerDice);
            allDice.AddRange(dealerDice);
            intCount = TranslateString(count);
            for (int i = 0; i < allDice.Count; i++)
            {
                if (allDice[i].faceValue == value)
                    num++;
            }
            if (isDealer)
                finalDisplay = "Your opponent challenged it! Time to show the dice.\n";
            

            finalDisplay += DisplayDice(delayPlayerDisplay) + "\n" + DisplayDice(delayDealerDisplay) + "\n";
            if (num  < intCount)
            {
                if (isDealer)
                {
                    playerDice.RemoveAt(0);
                }
                else
                {
                    dealerDice.RemoveAt(0);
                }
            }
            else
            {
                if (isDealer)
                {
                    dealerDice.RemoveAt(0);
                }
                else
                {
                    playerDice.RemoveAt(0);
                }
            }
            lastBidCount = 0;
            lastBidValue = 0;
            delayPlayerDisplay.Clear();
            delayDealerDisplay.Clear();
            for (int i = 0; i < dealerDice.Count; i++)
            {
                dealerDice[i].Roll();
            }
            for (int i = 0; i < playerDice.Count; i++)
            {
                playerDice[i].Roll();
                delayPlayerDisplay.Add(playerDice[i].faceValue);
            }
            if (num < intCount)
            {
                finalDisplay += "The challenge succeeded! The score is now " + playerDice.Count + " dice to " + dealerDice.Count + " dice.\n";
            }
            else
            {
                finalDisplay += "The challenge failed!\nThe score is now " + playerDice.Count + " dice for the player and " + dealerDice.Count + " dice for the opponent.\n\n";
            }
            if (playerDice.Count == 0)
            {
                finalDisplay += "Game over! Better luck next time.\nYou lost " + currentBet + " credits.\n\n";
                creditEarnings -= currentBet;
                diceGames++;
                PlayerPrefs.SetInt("DiceGames", diceGames); PlayerPrefs.SetInt("Earnings", creditEarnings);
            }
            else if (dealerDice.Count == 0)
            {
                finalDisplay += "Game over! Well done. \nYou won " + currentBet + " credits.\n\n";
                Terminal tempTerminal = FindAnyObjectByType<Terminal>(); //Kind of bodge-ey, but it should work.
                Banker.MoneyManip(tempTerminal, currentBet * 2);
                creditEarnings += currentBet;
                diceWins++;
                diceGames++;
                PlayerPrefs.SetInt("DiceGames", diceGames); PlayerPrefs.SetInt("DiceWins", diceWins); PlayerPrefs.SetInt("Earnings", creditEarnings);
            }
            else if (isDealer)
            {
                finalDisplay += DisplayDice(delayPlayerDisplay) + "\nIt's your turn. Enter your bid (format 'bid [amount] [value]s'. Ex. {'bid two 1s'})\n\n";
            }
            else
            {
                finalDisplay += "It's your opponents turn.\n";
                DiceOpponentBid(true);
            }

        }
        //End Dice methods
        
        //Begin Go Fish methods
        public static TerminalNode BeginGoFish(Terminal __terminal)
        {
            string finalDisplay = "May the best fisher win!\n\nYour hand: ";
            playerODCards.Clear();
            dealerODCards.Clear();
            deck = new DeckOfCards();
            for (int x = 0; x < 7; x++)
            {
                playerODCards.Add(deck.Draw());
                dealerODCards.Add(deck.Draw());
            }
            playingGoFish = true;
            List<OneDeckCard> sortedPlayerHand = playerODCards.OrderBy(card => card.cardValue).ToList();
            for (int i = 0; i < playerODCards.Count - 1; i++)
            {
                finalDisplay += sortedPlayerHand[i].valueRep + " of " + sortedPlayerHand[i].suitRep + ", ";
            }
            finalDisplay += "and " + sortedPlayerHand[playerODCards.Count - 1].valueRep + " of " + sortedPlayerHand[playerODCards.Count - 1].suitRep;
            finalDisplay += ".\n\nIt's your turn, request a card from your opponent. (Format 'request [card]' in words)\n\n";
            TerminalNode returnNode = CreateTerminalNode(finalDisplay);
            returnNode.clearPreviousText = true;
            return returnNode;
        }

        public static TerminalNode RequestGoFish(Terminal __terminal)  
        {                                                            
            finalDisplay = "You asked for ";
            string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal); 
            input = input.ToLower(); 
            input = input.Substring(8);
            TerminalNode returnNode;
            if (playingGoFish)
            {
                int numCards = 0;
                if (PlayerTurn(input)) //PlayerTurn returns false if it cannot parse the input. If it does not return false it handles all of the player's turn.
                {
                    if (playingGoFish)  //If the game didn't end on the player's turn
                        OpponentTurn(); //Handles the opponents turn.
                }
                returnNode = CreateTerminalNode(finalDisplay);
                returnNode.clearPreviousText = true;
                return returnNode;
            }
            else
            {
                finalDisplay = "There was an issue with your request.\n\n";
            }
            returnNode = CreateTerminalNode(finalDisplay);
            returnNode.clearPreviousText = true;
            return returnNode;
        }

        private static bool PlayerTurn(string input)
        {
            TerminalNode returnNode;
            if (!cardNames.Contains(input))
            {
                finalDisplay = "There was an issue with what you requested. Please try again (format 'request [card]' in words)\n\n";
                return false;
            }
            int numCards = HandlePlayerTurn(input);
            HandlePostTurn(numCards);
            return true;
        }

        private static int HandlePlayerTurn(string input)
        {
            int numCards = 0;
            for (int i = dealerODCards.Count - 1; i >= 0; i--)
            {
                if (dealerODCards.Count != 0 && dealerODCards[i].valueRep.Substring(4).ToLower().Equals(input))
                {
                    numCards++;
                    playerODCards.Add(dealerODCards[i]);
                    dealerODCards.RemoveAt(i);
                }
            }

            finalDisplay += input + "s. Your opponent had " + numCards;
            return numCards;
        }

        private static void HandlePostTurn(int numCards)
        {
            if (numCards == 0 && CardRemaining(deck))
            {
                playerODCards.Add(deck.Draw());
                finalDisplay += "\n\nGo fish! You drew " + playerODCards[playerODCards.Count - 1].valueRep + " of " + playerODCards[playerODCards.Count - 1].suitRep;
            }
            else if (numCards == 0 && !CardRemaining(deck))
            {
                finalDisplay += "\n\nNo more cards in the deck!";
            }

            if (HasABook(false))
            {
                finalDisplay += "\nYou completed a book! That makes " + playerBooks + " for you, and " + dealerBooks + " for your opponent.\n\n";
            }

            if (!CardRemaining(deck) && (dealerODCards.Count + playerODCards.Count == 0))
            {
                finalDisplay += GetGameResultMessage();
                playingGoFish = false;
                
            }
        }

        private static string GetGameResultMessage()
        {
            string resultMessage = "That's game! With a final score of " + playerBooks + " books to " + dealerBooks + ", ";
            resultMessage += (playerBooks > dealerBooks) ? "you win!\n\n" : "your opponent wins! Better luck next time.\n\n";
            fishGames++;
            if (playerBooks > dealerBooks)
            {
                fishWins++;
                PlayerPrefs.SetInt("FishWins", fishGames);
            }
            averageBooks = averageBooks + playerBooks;
            PlayerPrefs.SetInt("FishGames", fishGames); PlayerPrefs.SetInt("AvgBooks", averageBooks);
            return resultMessage;
        }

        private static void OpponentTurn()
        {
            int seed = Random.Range(1, 11);
            int askFor = Random.Range(0, dealerODCards.Count);
            askFor = GoFishAI(askFor, seed);
            string askForRep = GetCardRepresentation(askFor);

            finalDisplay += "\nIt's your opponent's turn. They ask for " + askForRep;
            int secondNumCards = HandleOpponentTurn(askFor);
            HandlePostOpponentTurn(secondNumCards);
        }

        private static int GoFishAI(int original, int seed)
        {
            if (seed <= 8)
            {
                if (original >= 0 && original < dealerODCards.Count)
                {
                    original = dealerODCards[original].cardValue;
                }
                else if (dealerODCards.Count > 0)
                {
                    original = Random.Range(0, dealerODCards.Count);
                    original = dealerODCards[original].cardValue;
                }
                else
                {
                    original = Random.Range(1, 14);
                }
            }
            else
            {
                original = Random.Range(1, 14);
            }
            int tries = 0;
            while (lastCardAsked == original && dealerODCards.Count > 0) 
            {
                original = dealerODCards[Random.Range(0, dealerODCards.Count)].cardValue;
                tries++;
                if (tries == 25) //Ugly, ugly way to handle this.
                {
                    original = Random.Range(1, 14);
                    break;
                }
            }
            lastCardAsked = original;
            return original;
        }
        private static int HandleOpponentTurn(int askFor)
        {
            int secondNumCards = 0;

            for (int i = 0; i < playerODCards.Count; i++)
            {
                if (playerODCards[i].cardValue == askFor)
                {
                    secondNumCards++;
                    dealerODCards.Add(playerODCards[i]);
                    playerODCards.RemoveAt(i);
                    i--;
                }
            }

            finalDisplay += ". You had " + secondNumCards;
            return secondNumCards;
        }

        private static void HandlePostOpponentTurn(int secondNumCards)
        {
            if (secondNumCards == 0 && CardRemaining(deck))
            {
                dealerODCards.Add(deck.Draw());
                finalDisplay += "\nGo fish! Your opponent drew a card.";
            }
            else if (!CardRemaining(deck) && secondNumCards == 0)
            {
                finalDisplay += "\n\nNo more cards in the deck!";
            }

            if (HasABook(true))
            {
                finalDisplay += "\nYour opponent completed a book! That makes " + playerBooks + " for you, and " + dealerBooks + " for your opponent.\n\n";
            }

            if (!CardRemaining(deck) && (dealerODCards.Count + playerODCards.Count == 0))
            {
                finalDisplay += GetGameResultMessage();
                playingGoFish = false;
            }
            else
            {
                DisplayPlayerHand();
            }
        }

        private static void DisplayPlayerHand()
        {
            finalDisplay += "\n\nYour hand: ";

            List<OneDeckCard> sortedPlayerHand = playerODCards.OrderBy(card => card.cardValue).ToList();
            for (int i = 0; i < playerODCards.Count - 1; i++)
            {
                finalDisplay += sortedPlayerHand[i].valueRep + " of " + sortedPlayerHand[i].suitRep + ", ";
            }
            finalDisplay += "and " + sortedPlayerHand[playerODCards.Count - 1].valueRep + " of " + sortedPlayerHand[playerODCards.Count - 1].suitRep;
            finalDisplay += ".\n\nIt's your turn, request a card from your opponent. (Format 'request [card]' in words)\n\n";
        }

        private static string GetCardRepresentation(int cardValue)
        {
            string[] cardNames = { "aces", "twos", "threes", "fours", "fives", "sixes", "sevens", "eights", "nines", "tens", "jacks", "queens", "kings" };
            return cardNames[cardValue - 1];
        }
        //End Go Fish methods

        //Begin Numbers methods
        public static TerminalNode BeginNumbers(Terminal __terminal)
        {
             guessableNumber = Random.Range(1, 1001);
             playingNumbers = true;
             TerminalNode returnNode = CreateTerminalNode("I've chosen a random number between 1 and 1000.\nCan you guess it? (Format 'guess [number]')\n\n");
             returnNode.clearPreviousText = true;
             return returnNode;
        }

        public static TerminalNode GuessNumbers(Terminal __terminal)
        {
            TerminalNode returnNode;
            if (playingNumbers)
            {
                tries++;
                string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
                RemovePunctuation(input);
                input = input.Substring(6);
                if (int.TryParse(input, out int result))
                {
                    string finalDisplayText = "You guessed " + result + ". That is "; //Base text all guesses will retunr
                    if (result == guessableNumber) //If you're right
                    {
                        finalDisplayText += "correct! You got the number in " + tries + " tries.\n\n";
                        numbersGames++;
                        numbersWins++;
                        averageGuesses = averageGuesses + tries;
                        PlayerPrefs.SetInt("NumGames", numbersGames); PlayerPrefs.SetInt("NumWins", numbersWins); PlayerPrefs.SetInt("AvgNumGuess", averageGuesses);
                        tries = 0;
                        playingNumbers = false;
                    }
                    else if (result > guessableNumber) //If you're too high
                    {
                        if (tries == 10)
                        {
                            finalDisplayText += "too high. That was guess ten, and you've lost. The correct number was " + guessableNumber + ".\n\n";
                            numbersGames++;
                            averageGuesses = (averageGuesses + tries) / numbersGames;
                            PlayerPrefs.SetInt("NumGames", numbersGames); PlayerPrefs.SetInt("AvgNumGuess", averageGuesses);
                            tries = 0;
                            playingNumbers = false;
                        }
                        else
                        {
                            finalDisplayText += "too high. You are on guess number " + (tries + 1) + ".\n\n";
                        }
                    }
                    else //If you're too low
                    {
                        if (tries == 10)
                        {
                            finalDisplayText += "too low. That was guess ten, and you've lost. The correct number was " + guessableNumber + ".\n\n";
                            numbersGames++;
                            averageGuesses = (averageGuesses + tries) / numbersGames;
                            PlayerPrefs.SetInt("NumGames", numbersGames); PlayerPrefs.SetInt("AvgNumGuess", averageGuesses);
                            tries = 0;
                            playingNumbers = false;
                        }
                        else
                        {
                            finalDisplayText += "too low. You are on guess number " + (tries + 1) + ".\n\n";
                        }
                    }
                    returnNode = CreateTerminalNode(finalDisplayText);
                }
                else
                {
                    tries = 0;
                    returnNode = CreateTerminalNode("There was an issue with your guess. Please start the Numbers game again.\n\n");
                }
                returnNode.clearPreviousText = true;
                return returnNode;
            }
            else
            {
               returnNode = CreateTerminalNode("There was an issue with your request.\n\n");
               returnNode.clearPreviousText = true;
               return returnNode;

            }
        }
        //End Numbers methods

        //Begin BlackJack methods
        public static TerminalNode BetBlackjack(Terminal __terminal)
        {
            TerminalNode betNode = CreateTerminalNode("Welcome to BlackJack! Ready to gamble?\nEnter how many credits you would like to bet. (Format 'bet [ammount]')\n\n");
            betNode.clearPreviousText = true;
            ConsoleGamesMain.bettingBJ = true;
            return betNode;
        }

        public static TerminalNode ConfirmBet(Terminal __terminal)
        {
            if (bettingBJ || bettingDice)
            {
                string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
                RemovePunctuation(input);
                input = input.Substring(4);
                TerminalNode confirmNode;
                if (int.TryParse(input, out int result) && Banker.PocketWatch(__terminal, result))
                {
                    if (result < 0)
                    {
                        confirmNode = CreateTerminalNode("You can't bet a negative number.\n\n");
                        return confirmNode;
                    }
                    confirmNode = CreateTerminalNode("You have bet " + result + " credits. Is this okay?\nType continue or back.\n\n");
                    currentBet = result;

                }
                else
                {
                    confirmNode = CreateTerminalNode("There was an issue with your bet. Please try again.\n\n");
                }
                confirmNode.clearPreviousText = true;
                return confirmNode;
            }
            else
            {
                TerminalNode rejectNode = CreateTerminalNode("There was an issue with your request.\n\n");
                rejectNode.clearPreviousText = true;
                return rejectNode;
            }
        }
        public static TerminalNode DenyBet(Terminal __terminal)
        {
            if (bettingBJ || bettingDice)
            {
                TerminalNode rejectNode = CreateTerminalNode("Bet cancelled.\n\n");
                currentBet = 0;
                rejectNode.clearPreviousText = true;
                return rejectNode;
            }
            else
            {
                TerminalNode rejectNode = CreateTerminalNode("There was an issue with your request.\n\n");
                rejectNode.clearPreviousText = true;
                return rejectNode;
            }
        }

        public static TerminalNode BetConfirmed(Terminal __terminal)
        {
            dealerBJCards.Clear();
            playerBJCards.Clear();
            dealerDice.Clear();
            playerDice.Clear();                                                    
            if (bettingBJ)
            {
                playingBJ = true;
                bettingBJ = false;
                Banker.MoneyManip(__terminal, (currentBet * -1));
                dealerBJCards.Add(new BJCard()); dealerBJCards.Add(new BJCard());
                playerBJCards.Add(new BJCard()); playerBJCards.Add(new BJCard());
                if (dealerBJCards[0].cardValue + dealerBJCards[1].cardValue == 21)
                {
                    TerminalNode lostNode = CreateTerminalNode("Let the game begin!\nThe dealer has BlackJack! You have lost " + currentBet + " credits.\n\n");
                    bjGames++;
                    creditEarnings -= currentBet;
                    PlayerPrefs.SetInt("BJGames", bjGames); PlayerPrefs.SetInt("Earnings", creditEarnings);
                    playingBJ = false;
                    bettingBJ = true;
                    lostNode.clearPreviousText = true;
                    currentBet = 0;
                    return lostNode;
                }
                else if (playerBJCards[0].cardValue + playerBJCards[1].cardValue == 21)
                {
                    Banker.MoneyManip(__terminal, (currentBet * 2) + currentBet / 2); //BJ pays out 3:2
                    TerminalNode wonNode = CreateTerminalNode("Let the game begin!\nYou have " + playerBJCards[0].valueRep + " and " + playerBJCards[1].valueRep + ". BlackJack! You win " + (currentBet * 1.5) + " credits.\n\n");
                    bjGames++;
                    bjWins++;
                    creditEarnings += currentBet + (currentBet/ 2);
                    PlayerPrefs.SetInt("BJGames", bjGames); PlayerPrefs.SetInt("BJWins", bjWins); PlayerPrefs.SetInt("Earnings", creditEarnings);
                    currentBet = 0;
                    playingBJ = false;
                    bettingBJ = true;
                    wonNode.clearPreviousText = true;
                    return wonNode;
                }
                TerminalNode startNode = CreateTerminalNode("Let the game begin!\nThe dealer is showing " + dealerBJCards[1].valueRep + ".\n\nYou have " + playerBJCards[0].valueRep + " and " + playerBJCards[1].valueRep + ", for a total of " + (playerBJCards[0].cardValue + playerBJCards[1].cardValue) + ".\n\nHit, or Stand?\n\n");
                startNode.clearPreviousText = true;
                return startNode;
            } //For confirming blackjack
            else if (bettingDice)
            {
                string finalDisplay = "Let the game begin!\nYour first roll:\n";
                List<int> delayDisplays = new List<int>();
                playingDice = true;
                bettingDice = false;
                Banker.MoneyManip(__terminal, (currentBet * -1));
                for (int i = 0; i < 5; i++)
                {
                    dealerDice.Add(new Dice());
                    playerDice.Add(new Dice());
                    delayDisplays.Add(playerDice[i].faceValue);
                }
                finalDisplay += DisplayDice(delayDisplays);
                finalDisplay += "\n\nEnter your bid (format 'bid [amount] [value]s'. Ex. ('bid two 1s').\n\n";
                TerminalNode returnNode = CreateTerminalNode(finalDisplay);
                returnNode.clearPreviousText = true;
                return returnNode;
            }
            else
            {
                TerminalNode rejectNode = CreateTerminalNode("There was an issue with your request.\n\n");
                rejectNode.clearPreviousText = true;
                return rejectNode;
            }
        }

        public static TerminalNode HitBlackJack(Terminal __terminal)
        {
            int cardTotal = 0;
            TerminalNode resultNode;
            if (playingBJ)
            {
                playerBJCards.Add(new BJCard());
                for (int index = 0; index < playerBJCards.Count; index++)
                {
                    cardTotal += playerBJCards[index].cardValue;
                }

                if (cardTotal == 21) //Forces stand at 21 
                {
                    int dealersVal = dealerBJCards[0].cardValue + dealerBJCards[1].cardValue;
                    playingBJ = false;
                    bettingBJ = true;
                    if (dealersVal >= 17)
                    {
                        Banker.MoneyManip(__terminal, currentBet * 2);
                        bjGames++;
                        bjWins++;
                        creditEarnings += currentBet;
                        PlayerPrefs.SetInt("BJGames", bjGames); PlayerPrefs.SetInt("BJWins", bjWins); PlayerPrefs.SetInt("Earnings", creditEarnings);
                        resultNode = CreateTerminalNode("You hit and recieved " + playerBJCards[playerBJCards.Count - 1].valueRep + ". That's 21!\n\nDealer has " + dealersVal + ", so they stand. You win " + currentBet + " credits.\n\n");
                        currentBet = 0;
                    }
                    else
                    {
                        string finalDisplay = "You hit and recieved " + playerBJCards[playerBJCards.Count - 1].valueRep + ". That's 21! Dealer has " + dealersVal + ", so they must draw. They draw ";  //Base of the final terminal node's text
                        while (dealersVal < 17)
                        {
                            dealerBJCards.Add(new BJCard());
                            finalDisplay += dealerBJCards[dealerBJCards.Count - 1].valueRep;   //Add what the dealer drew to the string
                            dealersVal += dealerBJCards[dealerBJCards.Count - 1].cardValue;
                            if (dealersVal > 21)
                            {
                                int cardIndex = HasAnAce(true);
                                if (cardIndex != -1)
                                {
                                    dealerBJCards[cardIndex].cardValue = 1;
                                    dealersVal -= 10;
                                }
                            }
                            if (dealersVal < 17)
                            {
                                finalDisplay += " and ";    //If the dealer has to draw more than once add an "and" to the end
                            }
                        }
                        if (dealersVal == 21)  //Dealer ties, so it's a push
                        {
                            Banker.MoneyManip(__terminal, currentBet);
                            bjGames++;
                            bjWins++;
                            PlayerPrefs.SetInt("BJGames", bjGames); PlayerPrefs.SetInt("BJWins", bjWins);
                            finalDisplay += ". Dealer has 21! You tied, so your bet of " + currentBet + " was returned to your account.\n\n";
                            currentBet = 0;
                        }
                        else //Dealer loses, either bust or standing at less than 21
                        {
                            Banker.MoneyManip(__terminal, currentBet * 2);
                            bjGames++;
                            bjWins++;
                            creditEarnings += currentBet;
                            PlayerPrefs.SetInt("BJGames", bjGames); PlayerPrefs.SetInt("BJWins", bjWins); PlayerPrefs.SetInt("Earnings", creditEarnings);
                            if (dealersVal > 21) //Bust
                            {
                                finalDisplay += ". Dealer has " + dealersVal + ", that's a bust! You win " + currentBet + " credits.\n\n";
                                currentBet = 0;
                            }
                            else //Stand
                            {
                                finalDisplay += ". Dealer has " + dealersVal + ", so they must stand. You win " + currentBet + " credits.\n\n";
                                currentBet = 0;
                            }

                        }
                        resultNode = CreateTerminalNode(finalDisplay);

                    }
                    resultNode.clearPreviousText = true;
                }
                else if (cardTotal < 21) //Still playing
                {
                    resultNode = CreateTerminalNode("You hit and recieved " + playerBJCards[playerBJCards.Count - 1].valueRep + ", for a total of " + cardTotal + ". Hit, or stand?\n\n");
                }
                else //More than 21
                {
                    int cardIndex = HasAnAce(false);
                    if (cardIndex != -1 && (cardTotal - 10 < 21)) //Has an ace and it saves from busting
                    {
                        resultNode = CreateTerminalNode("You hit and recieved " + playerBJCards[playerBJCards.Count - 1].valueRep + ", for a total of " + cardTotal + ". You have an ace, so now your total is " + (cardTotal - 10) + ". Hit, or stand?\n\n");
                        playerBJCards[cardIndex].cardValue = 1;
                    }
                    else //Either has no ace or ace doesn't save them
                    {
                        playingBJ = false;
                        bettingBJ = true;
                        resultNode = CreateTerminalNode("You hit and recieved " + playerBJCards[playerBJCards.Count - 1].valueRep + ", for a total of " + cardTotal + ". That's a bust. You lost " + currentBet + " credits.\n\n");
                        creditEarnings -= currentBet;
                        bjGames++;
                        PlayerPrefs.SetInt("BJGames", bjGames); PlayerPrefs.SetInt("Earnings", creditEarnings);
                        currentBet = 0;
                    }
                }
                resultNode.clearPreviousText = true;
                return resultNode;
            }
            else //if not playingBJ
            {
                TerminalNode rejectNode = CreateTerminalNode("There was an issue with your request.\n\n");
                rejectNode.clearPreviousText = true;
                return rejectNode;
            }


        }

        public static TerminalNode StandBlackJack(Terminal __terminal)
        {
            int finalResult = 0; //1 is a player win, 2 is a dealer win, 3 is a tie.
            if (playingBJ)
            {
                TerminalNode finalNode;
                int cardTotal = 0;
                for (int index = 0; index < playerBJCards.Count; index++)
                {
                    cardTotal += playerBJCards[index].cardValue;
                }
                string finalDisplay = "You stood on " + cardTotal + ".";
                int dealersVal = dealerBJCards[0].cardValue + dealerBJCards[1].cardValue;
                if (dealersVal >= 17) //Dealer stands on all 17s.
                {
                    finalDisplay += " Dealer has " + dealersVal + ", so they must stand.";
                    if (cardTotal > dealersVal) //Player wins
                    {
                        Banker.MoneyManip(__terminal, currentBet * 2);
                        finalResult = 1;
                        finalDisplay += " You won " + currentBet + " credits.\n\n";
                        playingBJ = false;
                        bettingBJ = true;
                    }
                    else if (cardTotal == dealersVal) //Tie
                    {
                        Banker.MoneyManip(__terminal, currentBet);
                        finalDisplay += " Your bet of " + currentBet + " was returned to your account.\n\n";
                        finalResult = 3;
                    }
                    else    //Player loses
                    {
                        finalDisplay += " You lost " + currentBet + " credits.\n\n";
                        finalResult = 2;

                    }
                    playingBJ = false;
                    bettingBJ = true;
                }
                else //if dealer has less than 17
                {
                    finalDisplay += "Dealer has " + dealersVal + ", so they must draw. They draw ";
                    while (dealersVal < 17) //Dealer draws until they reach 17
                    {
                        dealerBJCards.Add(new BJCard());
                        finalDisplay += dealerBJCards[dealerBJCards.Count - 1].valueRep;   //Add what the dealer drew to the string
                        dealersVal += dealerBJCards[dealerBJCards.Count - 1].cardValue;
                        if (dealersVal > 21)
                        {
                            int cardIndex = HasAnAce(true);
                            if (cardIndex != -1)
                            {
                                dealerBJCards[cardIndex].cardValue = 1;
                                dealersVal -= 10;
                            }
                        }
                        if (dealersVal < 17)
                        {
                            finalDisplay += " and ";    //If the dealer has to draw more than once add an "and" to the end
                        }
                    }
                    if (dealersVal > 21) //Dealer busts
                    {
                        Banker.MoneyManip(__terminal, currentBet * 2);
                        finalResult = 1;
                        finalDisplay += ". That's " + dealersVal + ", bust! You win " + currentBet + " credits.\n\n";

                    }
                    else if (dealersVal == cardTotal) //Dealer ties 
                    {
                        finalResult = 3;
                        finalDisplay += ". That's " + dealersVal + ", tie! Your bet of " + currentBet + " was returned to your account.\n\n";
                    }
                    else if (dealersVal < cardTotal) //Dealer loses
                    {
                        finalResult = 1;
                        finalDisplay += ". That's " + dealersVal + ", so the dealer must stand. You win " + currentBet + " credits.\n\n";
                    }
                    else //Dealer wins
                    {
                        finalResult = 2;
                        finalDisplay += ". That's " + dealersVal + ", so the dealer must stand. You lose " + currentBet + " credits.\n\n";
                    }
                    playingBJ = false;
                    bettingBJ = true;
                }
                if (finalResult == 1) //Win
                {
                    bjGames++;
                    bjWins++;
                    creditEarnings += currentBet;
                    PlayerPrefs.SetInt("BJGames", bjGames); PlayerPrefs.SetInt("BJWins", bjWins); PlayerPrefs.SetInt("Earnings", creditEarnings);
                }
                else if (finalResult == 2) //Loss
                {
                    bjGames++;
                    creditEarnings -= currentBet;
                    PlayerPrefs.SetInt("BJGames", bjGames); PlayerPrefs.SetInt("Earnings", creditEarnings);
                }
                else //Tie (counts as a win in stats)
                {
                    bjGames++;
                    bjWins++;
                    PlayerPrefs.SetInt("BJGames", bjGames); PlayerPrefs.SetInt("BJWins", bjWins);
                }
                currentBet = 0;
                finalNode = CreateTerminalNode(finalDisplay);
                finalNode.clearPreviousText = true;
                return finalNode;
            }
            else
            {
                TerminalNode rejectNode = CreateTerminalNode("There was an issue with your request.\n\n");
                rejectNode.clearPreviousText = true;
                return rejectNode;
            }
        }
        //End BlackJack methods
        private void OnTerminalExit(object sender, TerminalEventArgs e)
        {
            playingBJ = false;
            bettingBJ = false;
            playingNumbers = false;
            playingGoFish = false;
            playingDice = false;
            //Saves stats about games played. 
            PlayerPrefs.Save();
        }

        private void TextSubmitted(object sender, TerminalParseSentenceEventArgs e)
        {
            if (!ConsoleGamesMain.bjWords.Contains(e.SubmittedText))
            {
                ConsoleGamesMain.playingBJ = false;
                ConsoleGamesMain.bettingBJ = false;
            }
        }

    }

    internal class Banker
    {
        public static void MoneyManip(Terminal __instance, int amount)
        {
            __instance.groupCredits += amount;
        }
        public static bool PocketWatch(Terminal __instance, int amount)
        {
            return (amount <= __instance.groupCredits);
        }
    }
}  //EOF