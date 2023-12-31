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
using static UnityEngine.GraphicsBuffer;



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
        private const string modVersion = "1.2.0";
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
        public static bool playingAdventure = false;
        //Vars for BJ
        public static List<string> bjWords = new List<string>() { "blackjack", "bj", "black", "bet", "confirm", "stand" };
        public static int currentBet = 0;
        public static List<BJCard> dealerBJCards = new List<BJCard>();
        public static List<BJCard> playerBJCards = new List<BJCard>();
        public static List<int> displayPlayerCardVals = new List<int>();
        public static List<int> displayPlayerCardSuits = new List<int>();
        public static List<int> displayDealerCardVals = new List<int>();
        public static List<int> displayDealerCardSuits = new List<int>();
        //Vars for Go Fish
        public static string finalDisplay;
        public static List<OneDeckCard> dealerODCards = new List<OneDeckCard>();
        public static List<OneDeckCard> playerODCards = new List<OneDeckCard>();
        public static int playerBooks = 0;
        public static int dealerBooks = 0;
        public static int lastCardAsked = -1;
        public static List<int> bookedCards;
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
        //Vars for Salvager
        public static int gameState;
            //Int vars because PlayerPrefs don't save bools. 0 is false 1 is true.
        public static int door0;    
        public static int door3;
        public static int door5;
        public static int door12;
        public static int door12Locked;
        public static int flashlight;
        public static int ladder;
        public static int remote;
        public static int landmine;
        public static int turret;
        public static int key;
        public static int keyGiven;
        public static int arm;
        public static int fuse1;
        public static int fuse2;
        public static int fuse3;
        public static int fuse4;
        public static int bodyMoved;
        public static int[] insertedFuses = {0, 0, 0, 0};

        public static int area;
        public static string[] inventory = {"Empty", "Empty", "Empty", "Empty"};
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
                Method = BlackjackMethods.BetConfirmed,
            };

            SimpleCommandModule bet = new SimpleCommandModule()
            {
                DisplayName = "bet",
                Description = "Choose bet ammount. Type 'bet' followed by your bet.",
                HasDynamicInput = true,
                Arguments = new string[] { "'bet'", "number of credits" },
                HideFromCommandList = true,
                Method = BlackjackMethods.ConfirmBet,
            };

            SimpleCommandModule betDenied = new SimpleCommandModule()
            {
                DisplayName = "back",
                Description = "Do not begin the game.",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = BlackjackMethods.DenyBet,
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
                Method = BlackjackMethods.StandBlackJack,
            };

            SimpleCommandModule blackjackHit = new SimpleCommandModule()
            {
                DisplayName = "hit",
                Description = "get another card",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = BlackjackMethods.HitBlackJack,
            };

            SimpleCommandModule blackjackDouble = new SimpleCommandModule()
            {
                DisplayName = "double",
                Description = "get another card and double",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = BlackjackMethods.DoubleBlackJack,
            };

            SimpleCommandModule blackjack = new SimpleCommandModule()
            {
                DisplayName = "blackjack",
                Description = "Begins blackjack.",
                HasDynamicInput = false,
                Abbreviations = new string[] { "bj", "black" },
                HideFromCommandList = true,
                Method = BlackjackMethods.BetBlackjack,
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

            //Begin Adventure modules
            SimpleCommandModule adventureBegin = new SimpleCommandModule()
            {
                DisplayName = "salvager",
                Description = "Begin the epic.",
                HasDynamicInput = false,
                HideFromCommandList = true,
                Method = AdventureMethods.LoadGame,
            };
            SimpleCommandModule adventureActions = new SimpleCommandModule()
            {
                DisplayName = "actions",
                Description = "lists actions",
                HasDynamicInput = false,
                HideFromCommandList = true,
                Method = AdventureMethods.Actions,
            };
            SimpleCommandModule adventureWalk = new SimpleCommandModule()
            {
                DisplayName = "walk",
                Description = "travels to a location",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = AdventureMethods.Walk,
            };
            SimpleCommandModule adventureGrab = new SimpleCommandModule()
            {
                DisplayName = "grab",
                Description = "takes an item",
                HasDynamicInput = true,
                Abbreviations = new string[] {"take"}, 
                HideFromCommandList = true,
                Method = AdventureMethods.GrabItem,
            };
            SimpleCommandModule adventureUse = new SimpleCommandModule()
            {
                DisplayName = "use",
                Description = "uses an item",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = AdventureMethods.UseItem,
            };
            SimpleCommandModule adventureInventory = new SimpleCommandModule()
            {
                DisplayName = "inventory",
                Description = "lists items in inventory",
                HasDynamicInput = false,
                HideFromCommandList = true,
                Method = AdventureMethods.Inventory,
            };
            SimpleCommandModule adventureInspect = new SimpleCommandModule()
            {
                DisplayName = "inspect",
                Description = "inspects area or item",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = AdventureMethods.Inspect,
            };
            SimpleCommandModule adventurePush = new SimpleCommandModule()
            {
                DisplayName = "push",
                Description = "pushes the body",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = AdventureMethods.Push,
            };
            SimpleCommandModule adventureRestart = new SimpleCommandModule()
            {
                DisplayName = "restart",
                Description = "resets the game",
                HasDynamicInput = false,
                HideFromCommandList = true,
                Method = AdventureMethods.Restart,
            };
            //End Adventure modules

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
                Description = "Displays a game's rules.\nType 'rules [game name]' for information.",
                HasDynamicInput = true,
                HideFromCommandList = false,
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
                Method = GoFishMethods.BeginGoFish,
            };

            SimpleCommandModule requestGoFish = new SimpleCommandModule()
            {
                DisplayName = "request",
                Description = "attempt to take a card",
                Arguments = new string[] { "request", "card" },
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = GoFishMethods.RequestGoFish,
            };



            AddSimpleCommand(gameList);
            AddSimpleCommand(blackjack);
            AddSimpleCommand(bet);
            AddSimpleCommand(betConfirmed);
            AddSimpleCommand(betDenied);
            AddSimpleCommand(blackjackHit);
            AddSimpleCommand(blackjackStand);
            AddSimpleCommand(blackjackDouble);
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
            AddSimpleCommand(adventureBegin);
            AddSimpleCommand(adventureActions);
            AddSimpleCommand(adventureInventory);
            AddSimpleCommand(adventureGrab);
            AddSimpleCommand(adventureUse);
            AddSimpleCommand(adventurePush);
            AddSimpleCommand(adventureWalk);
            AddSimpleCommand(adventureInspect);
            AddSimpleCommand(adventureRestart);
            harmony.PatchAll(typeof(ConsoleGamesMain));

        }
        //Begin misc methods
        public static TerminalNode ListGames(Terminal __terminal)
        {
            TerminalNode listNode = CreateTerminalNode(">BlackJack\nGamble your credits! Play responsibly.\n\n>Numbers\nCan you guess a number 1-1000 in 10 or less guesses?\n\n>GoFish\nAll of the fishing, none of the going. Got any sevens?\n\n>LiarsDice\nCan you bluff your way to victory?\n\n>Salvager\nCan you find your way through the labyrinth?\n\n");
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
            bettingBJ = false;
            playingAdventure = false;
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

        public static int CheatLevel(int AIDiff, int value)
        {
            int expectedDice = 0;
            if (AIDiff < 25) //No cheating, just speculation
            {
                Debug.Log("No cheating");
                if (playerDice.Count > 3) //Gives the benefit of the doubt to promote less challenges.
                    expectedDice++;
                if (playerDice.Count == 5)
                    expectedDice++;
            }
            else if (AIDiff < 50) //Looks at one of the player's dice.
            {
                if (playerDice[Random.Range(0, playerDice.Count)].faceValue == value)
                    expectedDice++;
                Debug.Log("Cheating a little.");
            }
            else if (AIDiff < 65 && playerDice.Count > 1) //Looks(if possible) at two of the player's dice.
            {
                Debug.Log("Cheating more");
                int index1 = Random.Range(0, playerDice.Count);
                int index2 = index1 - 1;
                if (index1 - 1 >= 0)
                {
                    index2 = index1 + 1;
                }
                if (playerDice[Random.Range(0, playerDice.Count)].faceValue == value)
                    expectedDice++;
            }
            else //Just completely cheats. Absolutely violates the integredy of the game.
            {
                Debug.Log("Cheating a lot");
                for (int i = 0; i < playerDice.Count; i++)
                {
                    if (playerDice[Random.Range(0, playerDice.Count)].faceValue == value)
                        expectedDice++;
                }
            }
            return expectedDice;
        }

        public static bool OverrideChallenge(string count)
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
            int numCount = 0;
            for (int i = 0; i < dealerDice.Count; i++)
            {
                if (dealerDice[i].faceValue == bestBid)
                    numCount++;
            }
            if (numCount > TranslateString(count))
                return true;
            return false;
        }
        public static bool ShouldChallenge(int value, string count)
        {
            if (OverrideChallenge(count)) //If the CPU has a bet locked in larger than the current bet, it shouldn't challenge. No reason.
            {
                return false;
            }
            int expectedDice = 0;
            for (int i = 0; i < dealerDice.Count; i++)
            {
                if (dealerDice[i].faceValue == value) //Finds how many of that number the dealer has
                {
                    expectedDice++;
                }
            }
            int AIDiff = Random.Range(1, 101); //Determines if and how much the AI can cheat. The AI is honestly so stupid and players can beat it like 75% of the time even with it cheating.
            expectedDice += CheatLevel(AIDiff, value);
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
        
        //Begin Numbers methods
        public static TerminalNode BeginNumbers(Terminal __terminal)
        {
             guessableNumber = Random.Range(1, 1001);
             playingNumbers = true;
             playingAdventure = false;
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

        private void OnTerminalExit(object sender, TerminalEventArgs e)
        {
            playingBJ = false;
            bettingBJ = false;
            playingNumbers = false;
            playingGoFish = false;
            playingDice = false;
            playingAdventure = false;
            AdventureMethods.Save();
            //Saves stats about games played. 
            PlayerPrefs.Save();
        }

        private void TextSubmitted(object sender, TerminalParseSentenceEventArgs e)
        {
            if (playingAdventure) //Saves the game on every entry while playing.
            {
                AdventureMethods.Save();
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
