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
        private const string modVersion = "1.0.0";
        //Initializing patch vars
        private static ConsoleGamesMain Instance;
        internal ManualLogSource logSource;
        public static bool bettingBJ = false;
        public static bool playingBJ = false;
        public static bool playingNumbers = false;
        public static List<string> bjWords = new List<string>() {"blackjack", "bj", "black", "bet", "confirm", "stand"};
        public static int currentBet = 0;
        public static List<BJCard> dealerCards = new List<BJCard>();
        public static List<BJCard> playerCards = new List<BJCard>();
        public static int guessableNumber;
        public static int tries = 0;
        private readonly Harmony harmony = new Harmony(modGUID);

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
            
            SimpleCommandModule blackjackConfirmed = new SimpleCommandModule()
            {
                DisplayName = "continue",
                Description = "Begin the game.",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = BeginBlackjack,
            };

            SimpleCommandModule blackjackDenied = new SimpleCommandModule()
            {
                DisplayName = "back",
                Description = "Do not begin the game.",
                HasDynamicInput = true,
                HideFromCommandList = true,
                Method = DenyBlackjack,
            };

            SimpleCommandModule blackjackBet = new SimpleCommandModule()
            {
                DisplayName = "bet",
                Description = "Choose bet ammount. Type 'bet' followed by your bet.",
                HasDynamicInput = true,
                Arguments = new string[] {"'bet'", "number of credits"},
                HideFromCommandList = true,
                Method = ConfirmBlackjack,
            };

            SimpleCommandModule blackjack = new SimpleCommandModule()
            {
                DisplayName = "blackjack",
                Description = "Begins blackjack.",
                HasDynamicInput = false,
                Abbreviations = new string[] { "bj", "black" },
                HideFromCommandList = true,
                Method = BetBlackjack,
                ChildrenModules = new SimpleCommandModule[] { blackjackBet }
            };
            //End BlackJack modules

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

            SimpleCommandModule gameList = new SimpleCommandModule()
            {
                DisplayName = "list",
                Description = "Lists available games.\nType list for game list.",
                HasDynamicInput = false,
                Abbreviations = null,
                Method = ListGames,
            };

            
            

            AddSimpleCommand(gameList);
            AddSimpleCommand(blackjack);
            AddSimpleCommand(blackjackBet);
            AddSimpleCommand(blackjackConfirmed);
            AddSimpleCommand(blackjackDenied);
            AddSimpleCommand(blackjackConfirmed);
            AddSimpleCommand(blackjackHit);
            AddSimpleCommand(blackjackStand);
            AddSimpleCommand(beginNumbers);
            AddSimpleCommand(guessNumbers);
            harmony.PatchAll(typeof(ConsoleGamesMain));

        }

        public static TerminalNode ListGames(Terminal __terminal)
        {
            TerminalNode listNode = CreateTerminalNode(">BlackJack\nGamble your credits! Play responsibly.\n\n>Numbers\nCan you guess a number 1-1000 in 10 or less guesses?\n\n");
            listNode.clearPreviousText = true;
            return listNode;
        }
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
                        tries = 0;
                        playingNumbers = false;
                    }
                    else if (result > guessableNumber) //If you're too high
                    {
                        if (tries == 10)
                        {
                            finalDisplayText += "too high. That was guess eight, and you've lost. The correct number was " + guessableNumber + ".\n\n";
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
                            finalDisplayText += "too low. That was guess five, and you've lost. The correct number was " + guessableNumber + ".\n\n";
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

        public static TerminalNode ConfirmBlackjack(Terminal __terminal)
        {
            if (bettingBJ)
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
        public static TerminalNode DenyBlackjack(Terminal __terminal)
        {
            if (bettingBJ)
            {
                TerminalNode rejectNode = CreateTerminalNode("BlackJack cancelled.\n\n");
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

        public static TerminalNode BeginBlackjack(Terminal __terminal)
        {
            dealerCards.Clear();
            playerCards.Clear();
            if (bettingBJ)
            {
                playingBJ = true;
                bettingBJ = false;
                Banker.MoneyManip(__terminal, (currentBet * -1));
                dealerCards.Add(new BJCard()); dealerCards.Add(new BJCard());
                playerCards.Add(new BJCard()); playerCards.Add(new BJCard());
                if (dealerCards[0].cardValue + dealerCards[1].cardValue == 21)
                {
                    TerminalNode lostNode = CreateTerminalNode("Let the game begin!\nThe dealer has BlackJack! You have lost " + currentBet + " credits.\n\n");
                    playingBJ = false;
                    bettingBJ = true;
                    lostNode.clearPreviousText = true;
                    currentBet = 0;
                    return lostNode;
                }
                else if (playerCards[0].cardValue + playerCards[1].cardValue == 21)
                {
                    Banker.MoneyManip(__terminal, (currentBet * 2) + currentBet / 2); //BJ pays out 3:2
                    TerminalNode wonNode = CreateTerminalNode("Let the game begin!\nYou have " + playerCards[0].valueRep + " and " + playerCards[1].valueRep + ". BlackJack! You win " + (currentBet * 1.5) + " credits.\n\n");
                    currentBet = 0;
                    playingBJ = false;
                    bettingBJ = true;
                    wonNode.clearPreviousText = true;
                    return wonNode;
                }
                TerminalNode startNode = CreateTerminalNode("Let the game begin!\nThe dealer is showing " + dealerCards[1].valueRep + ".\n\nYou have " + playerCards[0].valueRep + " and " + playerCards[1].valueRep + ", for a total of " + (playerCards[0].cardValue + playerCards[1].cardValue) + ".\n\nHit, or Stand?\n\n");
                startNode.clearPreviousText = true;
                return startNode;
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
                playerCards.Add(new BJCard());
                for (int index = 0; index < playerCards.Count; index++)
                {
                    cardTotal += playerCards[index].cardValue;
                }

                if (cardTotal == 21) //Forces stand at 21 
                {
                    int dealersVal = dealerCards[0].cardValue + dealerCards[1].cardValue;
                    playingBJ = false;
                    bettingBJ = true;
                    if (dealersVal >= 17)
                    {
                        Banker.MoneyManip(__terminal, currentBet * 2);
                        resultNode = CreateTerminalNode("You hit and recieved " + playerCards[playerCards.Count - 1].valueRep + ". That's 21!\n\nDealer has " + dealersVal + ", so they stand. You win " + currentBet + " credits.\n\n");
                        currentBet = 0;
                    }
                    else
                    {
                        string finalDisplay = "You hit and recieved " + playerCards[playerCards.Count - 1].valueRep + ". That's 21! Dealer has " + dealersVal + ", so they must draw. They draw ";  //Base of the final terminal node's text
                        while (dealersVal < 17)
                        {
                            dealerCards.Add(new BJCard());
                            finalDisplay += dealerCards[dealerCards.Count - 1].valueRep;   //Add what the dealer drew to the string
                            dealersVal += dealerCards[dealerCards.Count - 1].cardValue;
                            if (dealersVal > 21)
                            {
                                int cardIndex = HasAnAce(true);
                                if (cardIndex != -1)
                                {
                                    dealerCards[cardIndex].cardValue = 1;
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
                            finalDisplay += ". Dealer has 21! You tied, so your bet of " + currentBet + " was returned to your account.\n\n";
                            currentBet = 0;
                        }
                        else //Dealer loses, either bust or standing at less than 21
                        {
                            Banker.MoneyManip(__terminal, currentBet * 2);
                            if (dealersVal > 21) //Bust
                            {
                                finalDisplay += ". Dealer has " + dealersVal + ", that's a bust! You win " + currentBet + " credits.\n\n";
                                currentBet = 0;
                            }
                            else //Stand
                            {
                                finalDisplay += ". Dealer has " + dealersVal + ", so they must stand. You win " + currentBet + "credits.\n\n";
                                currentBet = 0;
                            }

                        }
                        resultNode = CreateTerminalNode(finalDisplay);

                    }
                    resultNode.clearPreviousText = true;
                }
                else if (cardTotal < 21) //Still playing
                {
                    resultNode = CreateTerminalNode("You hit and recieved " + playerCards[playerCards.Count - 1].valueRep + ", for a total of " + cardTotal + ". Hit, or stand?\n\n");
                }
                else //More than 21
                {
                    int cardIndex = HasAnAce(false);
                    if (cardIndex != -1 && (cardTotal - 10 < 21)) //Has an ace and it saves from busting
                    {
                        resultNode = CreateTerminalNode("You hit and recieved " + playerCards[playerCards.Count - 1].valueRep + ", for a total of " + cardTotal + ". You have an ace, so now your total is " + (cardTotal - 10) + ". Hit, or stand?\n\n");
                        playerCards[cardIndex].cardValue = 1;
                    }
                    else //Either has no ace or ace doesn't save them
                    {
                        playingBJ = false;
                        bettingBJ = true;
                        resultNode = CreateTerminalNode("You hit and recieved " + playerCards[playerCards.Count - 1].valueRep + ", for a total of " + cardTotal + ". That's a bust. You lost " + currentBet + " credits.\n\n");
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
            if (playingBJ)
            {
                TerminalNode finalNode;
                int cardTotal = 0;
                for (int index = 0; index < playerCards.Count; index++)
                {
                    cardTotal += playerCards[index].cardValue;
                }
                string finalDisplay = "You stood on " + cardTotal + ".";
                int dealersVal = dealerCards[0].cardValue + dealerCards[1].cardValue;
                if (dealersVal >= 17) //Dealer stands on all 17s.
                {
                    finalDisplay += " Dealer has " + dealersVal + ", so they must stand.";
                    if (cardTotal > dealersVal) //Player wins
                    {
                        Banker.MoneyManip(__terminal, currentBet * 2);
                        finalDisplay += " You won " + currentBet + " credits.\n\n";
                        currentBet = 0;
                        playingBJ = false;
                        bettingBJ = true;
                    }
                    else if (cardTotal == dealersVal) //Tie
                    {
                        Banker.MoneyManip(__terminal, currentBet);
                        finalDisplay += " Your bet of " + currentBet + " was returned to your account.\n\n";
                    }
                    else    //Player loses
                    {
                        finalDisplay += " You lost " + currentBet + " credits.\n\n";
                        
                    }
                    playingBJ = false;
                    bettingBJ = true;
                    currentBet = 0;
                }
                else //if dealer has less than 17
                {
                    finalDisplay += "Dealer has " + dealersVal + ", so they must draw. They draw ";
                    while (dealersVal < 17) //Dealer draws until they reach 17
                    {
                        dealerCards.Add(new BJCard());
                        finalDisplay += dealerCards[dealerCards.Count - 1].valueRep;   //Add what the dealer drew to the string
                        dealersVal += dealerCards[dealerCards.Count - 1].cardValue;
                        if (dealersVal > 21)
                        {
                            int cardIndex = HasAnAce(true);
                            if (cardIndex != -1)
                            {
                                dealerCards[cardIndex].cardValue = 1;
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
                        finalDisplay += ". That's " + dealersVal + ", bust! You win " + currentBet + " credits.\n\n";

                    }
                    else if (dealersVal == cardTotal) //Dealer ties 
                    {
                        Banker.MoneyManip(__terminal, currentBet);
                        finalDisplay += ". That's " + dealersVal + ", tie! Your bet of " + currentBet + " was returned to your account.\n\n";
                    }
                    else if (dealersVal < cardTotal) //Dealer loses
                    {
                        Banker.MoneyManip(__terminal, currentBet * 2);
                        finalDisplay += ". That's " + dealersVal + ", so the dealer must stand. You win " + currentBet + " credits.\n\n";
                    }
                    else //Dealer wins
                    {
                        finalDisplay += ". That's " + dealersVal + ", so the dealer must stand. You lose " + currentBet + " credits.\n\n";
                    }
                    playingBJ = false;
                    bettingBJ = true;
                    currentBet = 0;
                }
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

        public static int HasAnAce(bool isDealer) //Checks if there is an ace in the list. false for player, true for dealer.
        {
            int returnCard = -1;
            bool hasAnAce = false;
            if (!isDealer)
            {
                for (int index = 0; index < playerCards.Count; index++)
                {
                    if (playerCards[index].valueRep.Equals("an Ace"))
                    {
                        returnCard = index;
                    }
                }
            }
            else
            {
                for (int index = 0; index < dealerCards.Count; index++)
                {
                    if (dealerCards[index].valueRep.Equals("an Ace"))
                    {
                        returnCard = index;
                    }
                }
            }
            return returnCard;
        }
        //End BlackJack methods
        private void OnTerminalExit(object sender, TerminalEventArgs e)
        {
            ConsoleGamesMain.playingBJ = false;
            ConsoleGamesMain.bettingBJ = false;
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

    public class BJCard
    {
        public int randoSuit;
        public int randoCard;
        public string valueRep;
        Terminal theTerminal;
        public  int cardValue = 10; //The BJ value. 10 by default, if not a face it updates.
        public BJCard() //For making a new card in blackjack
        {
            randoSuit = Random.Range(1, 5); //Chooses the card suit. Kinda pointless for BlackJack(especially since I don't tell the player what the suit is), but maybe I'll add another card game in a later version.
            randoCard = Random.Range(1, 14); //Chooses the card's value
            switch (randoCard)
            {
                case 1:
                    valueRep = "an Ace";
                    cardValue = 11;
                    break;
                case 8: //I hate how 8 is "an eight", while everything else is "a 2", "a 3", "a 4". Screw 8.
                    valueRep = "an 8";
                    cardValue = 8;
                    break;
                case 11:
                    valueRep = "a Jack";
                    break;
                case 12:
                    valueRep = "a Queen";
                    break;
                case 13:
                    valueRep = "a King";
                    break;
                default:
                    valueRep = "a " + randoCard.ToString();
                    cardValue = randoCard;
                    break;
            }

        }
    }
}  //EOF