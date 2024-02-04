using UnityEngine;
using static TerminalApi.TerminalApi;
using static SimpleCommand.API.SimpleCommand;
using System.Collections.Generic;
using static TerminalGames.CardClasses;
using static ConsoleGames.DiceClasses;


namespace TerminalGames
{
    internal class BlackjackMethods //This is about to be the ugliest class of all time. But it's fine, just keep the one's you don't need collapsed and it'll be good enough.
    {
        public static TerminalNode BetBlackjack(Terminal __terminal)
        {
            TerminalNode betNode = CreateTerminalNode("Welcome to BlackJack! Ready to gamble?\nEnter how many credits you would like to bet. (Format 'bet [ammount]')\n\n");
            betNode.clearPreviousText = true;
            ConsoleGamesMain.bettingBJ = true;
            ConsoleGamesMain.playingAdventure = false;
            return betNode;
        }

        public static TerminalNode ConfirmBet(Terminal __terminal)
        {
            if (ConsoleGamesMain.bettingBJ || ConsoleGamesMain.bettingDice)
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
                    ConsoleGamesMain.currentBet = result;

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
            if (ConsoleGamesMain.bettingBJ || ConsoleGamesMain.bettingDice)
            {
                TerminalNode rejectNode = CreateTerminalNode("Bet cancelled.\n\n");
                ConsoleGamesMain.currentBet = 0;
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
            Banker banker = new Banker();
            ConsoleGamesMain.dealerBJCards.Clear();
            ConsoleGamesMain.playerBJCards.Clear();
            ConsoleGamesMain.dealerDice.Clear();
            ConsoleGamesMain.playerDice.Clear();
            ConsoleGamesMain.displayPlayerCardVals.Clear(); ConsoleGamesMain.displayPlayerCardSuits.Clear();
            ConsoleGamesMain.displayDealerCardVals.Clear(); ConsoleGamesMain.displayDealerCardSuits.Clear();
            if (ConsoleGamesMain.bettingBJ)
            {
                ConsoleGamesMain.playingBJ = true;
                ConsoleGamesMain.bettingBJ = false;
                banker.MoneyManip(__terminal, (ConsoleGamesMain.currentBet * -1));
                ConsoleGamesMain.dealerBJCards.Add(new BJCard()); ConsoleGamesMain.dealerBJCards.Add(new BJCard());
                ConsoleGamesMain.playerBJCards.Add(new BJCard()); ConsoleGamesMain.playerBJCards.Add(new BJCard());
                if (ConsoleGamesMain.dealerBJCards[0].cardValue + ConsoleGamesMain.dealerBJCards[1].cardValue == 22) //Two aces.
                {
                    ConsoleGamesMain.dealerBJCards[1].cardValue = 1; //Makes one act as a 1.
                }
                ConsoleGamesMain.displayDealerCardVals.Add(ConsoleGamesMain.dealerBJCards[0].randoCard); ConsoleGamesMain.displayDealerCardSuits.Add(ConsoleGamesMain.dealerBJCards[0].randoSuit);
                for (int i = 0; i < 2; i++) //Prepare cards for ASCII display
                {
                    ConsoleGamesMain.displayPlayerCardVals.Add(ConsoleGamesMain.playerBJCards[i].randoCard); ConsoleGamesMain.displayPlayerCardSuits.Add(ConsoleGamesMain.playerBJCards[i].randoSuit);
                }
                string dealerCardsDisplayed = DisplayCards(ConsoleGamesMain.displayDealerCardVals, ConsoleGamesMain.displayDealerCardSuits);
                string cardsDisplayed = DisplayCards(ConsoleGamesMain.displayPlayerCardVals, ConsoleGamesMain.displayPlayerCardSuits);
                if (ConsoleGamesMain.dealerBJCards[0].cardValue + ConsoleGamesMain.dealerBJCards[1].cardValue == 21)
                {
                    ConsoleGamesMain.displayDealerCardVals.Add(ConsoleGamesMain.dealerBJCards[1].randoCard); ConsoleGamesMain.displayDealerCardSuits.Add(ConsoleGamesMain.dealerBJCards[1].randoSuit);
                    dealerCardsDisplayed = DisplayCards(ConsoleGamesMain.displayDealerCardVals, ConsoleGamesMain.displayDealerCardSuits);
                    ConsoleGamesMain.bjGames++;
                    ConsoleGamesMain.creditEarnings -= ConsoleGamesMain.currentBet;
                    PlayerPrefs.SetInt("BJGames", ConsoleGamesMain.bjGames); PlayerPrefs.SetInt("Earnings", ConsoleGamesMain.creditEarnings);
                    ConsoleGamesMain.playingBJ = false;
                    ConsoleGamesMain.bettingBJ = true;
                    TerminalNode lostNode = CreateTerminalNode("Let the game begin!\nDealer's cards:\n" + dealerCardsDisplayed + "\nDealer BlackJack! You have lost " + ConsoleGamesMain.currentBet + " credits.\n\n");
                    lostNode.clearPreviousText = true;
                    ConsoleGamesMain.currentBet = 0;
                    return lostNode;
                }
                else if (ConsoleGamesMain.playerBJCards[0].cardValue + ConsoleGamesMain.playerBJCards[1].cardValue == 21)
                {
                    banker.MoneyManip(__terminal, (ConsoleGamesMain.currentBet * 2) + ConsoleGamesMain.currentBet / 2); //BJ pays out 3:2
                    ConsoleGamesMain.displayDealerCardVals.Add(ConsoleGamesMain.dealerBJCards[1].randoCard); ConsoleGamesMain.displayDealerCardSuits.Add(ConsoleGamesMain.dealerBJCards[1].randoSuit);
                    dealerCardsDisplayed = DisplayCards(ConsoleGamesMain.displayDealerCardVals, ConsoleGamesMain.displayDealerCardSuits);
                    TerminalNode wonNode = CreateTerminalNode("Let the game begin!\nDealer's cards:\n" + dealerCardsDisplayed + "\nYour hand:\n" + cardsDisplayed + "\nBlackJack! You win " + (ConsoleGamesMain.currentBet * 1.5) + " credits.\n\n");
                    ConsoleGamesMain.bjGames++;
                    ConsoleGamesMain.bjWins++;
                    ConsoleGamesMain.creditEarnings += ConsoleGamesMain.currentBet + (ConsoleGamesMain.currentBet / 2);
                    PlayerPrefs.SetInt("BJGames", ConsoleGamesMain.bjGames); PlayerPrefs.SetInt("BJWins", ConsoleGamesMain.bjWins); PlayerPrefs.SetInt("Earnings", ConsoleGamesMain.creditEarnings);
                    ConsoleGamesMain.currentBet = 0;
                    ConsoleGamesMain.playingBJ = false;
                    ConsoleGamesMain.bettingBJ = true;
                    wonNode.clearPreviousText = true;
                    return wonNode;
                }
                TerminalNode startNode = CreateTerminalNode("Let the game begin!\nDealer card:\n" + dealerCardsDisplayed + "\n\nYour hand:\n" + cardsDisplayed + "\nHit, Double or Stand?\n\n");
                startNode.clearPreviousText = true;
                return startNode;
            } //For confirming blackjack
            else if (ConsoleGamesMain.bettingDice)
            {
                string finalDisplay = "Let the game begin!\nYour first roll:\n";
                List<int> delayDisplays = new List<int>();
                ConsoleGamesMain.playingDice = true;
                ConsoleGamesMain.bettingDice = false;
                banker.MoneyManip(__terminal, (ConsoleGamesMain.currentBet * -1));
                for (int i = 0; i < 5; i++)
                {
                    ConsoleGamesMain.dealerDice.Add(new Dice());
                    ConsoleGamesMain.playerDice.Add(new Dice());
                    delayDisplays.Add(ConsoleGamesMain.playerDice[i].faceValue);
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
            Banker banker = new Banker();
            string dealerCardsDisplayed = DisplayCards(ConsoleGamesMain.displayDealerCardVals, ConsoleGamesMain.displayDealerCardSuits);
            int cardTotal = 0;
            TerminalNode resultNode;
            if (ConsoleGamesMain.playingBJ)
            {
                ConsoleGamesMain.playerBJCards.Add(new BJCard());
                ConsoleGamesMain.displayPlayerCardVals.Add(ConsoleGamesMain.playerBJCards[ConsoleGamesMain.playerBJCards.Count - 1].randoCard);
                ConsoleGamesMain.displayPlayerCardSuits.Add(ConsoleGamesMain.playerBJCards[ConsoleGamesMain.playerBJCards.Count - 1].randoSuit);
                for (int index = 0; index < ConsoleGamesMain.playerBJCards.Count; index++)
                {
                    cardTotal += ConsoleGamesMain.playerBJCards[index].cardValue;
                }
                string cardsDisplayed = DisplayCards(ConsoleGamesMain.displayPlayerCardVals, ConsoleGamesMain.displayPlayerCardSuits);
                if (cardTotal == 21) //Forces stand at 21 
                {
                    int dealersVal = ConsoleGamesMain.dealerBJCards[0].cardValue + ConsoleGamesMain.dealerBJCards[1].cardValue;
                    ConsoleGamesMain.displayDealerCardVals.Add(ConsoleGamesMain.dealerBJCards[1].randoCard); ConsoleGamesMain.displayDealerCardSuits.Add(ConsoleGamesMain.dealerBJCards[1].randoSuit);
                    dealerCardsDisplayed = DisplayCards(ConsoleGamesMain.displayDealerCardVals, ConsoleGamesMain.displayDealerCardSuits);
                    ConsoleGamesMain.playingBJ = false;
                    ConsoleGamesMain.bettingBJ = true;
                    if (dealersVal >= 17)
                    {
                        banker.MoneyManip(__terminal, ConsoleGamesMain.currentBet * 2);
                        ConsoleGamesMain.bjGames++;
                        ConsoleGamesMain.bjWins++;
                        ConsoleGamesMain.creditEarnings += ConsoleGamesMain.currentBet;
                        PlayerPrefs.SetInt("BJGames", ConsoleGamesMain.bjGames); PlayerPrefs.SetInt("BJWins", ConsoleGamesMain.bjWins); PlayerPrefs.SetInt("Earnings", ConsoleGamesMain.creditEarnings);
                        resultNode = CreateTerminalNode("Dealer cards:\n" + dealerCardsDisplayed + "\nYour hand:\n" + cardsDisplayed + "\nThat's 21!\nYou win " + ConsoleGamesMain.currentBet + " credits.\n\n");
                        ConsoleGamesMain.currentBet = 0;
                    }
                    else
                    {
                        while (dealersVal < 17)
                        {
                            ConsoleGamesMain.dealerBJCards.Add(new BJCard());
                            ConsoleGamesMain.displayDealerCardVals.Add(ConsoleGamesMain.dealerBJCards[ConsoleGamesMain.dealerBJCards.Count - 1].randoCard); ConsoleGamesMain.displayDealerCardSuits.Add(ConsoleGamesMain.dealerBJCards[ConsoleGamesMain.dealerBJCards.Count - 1].randoSuit);
                            dealersVal += ConsoleGamesMain.dealerBJCards[ConsoleGamesMain.dealerBJCards.Count - 1].cardValue;
                            if (dealersVal > 21)
                            {
                                int cardIndex = HasAnAce(true);
                                if (cardIndex != -1)
                                {
                                    ConsoleGamesMain.dealerBJCards[cardIndex].cardValue = 1;
                                    dealersVal -= 10;
                                }
                            }
                        }
                        dealerCardsDisplayed = DisplayCards(ConsoleGamesMain.displayDealerCardVals, ConsoleGamesMain.displayDealerCardSuits);
                        string finalDisplay = "You have 21. Dealer draws.\nDealer's cards:\n" + dealerCardsDisplayed + "\nYour hand:\n" + cardsDisplayed;
                        if (dealersVal == 21)  //Dealer ties, so it's a push
                        {
                            banker.MoneyManip(__terminal, ConsoleGamesMain.currentBet);
                            ConsoleGamesMain.bjGames++;
                            ConsoleGamesMain.bjWins++;
                            PlayerPrefs.SetInt("BJGames", ConsoleGamesMain.bjGames); PlayerPrefs.SetInt("BJWins", ConsoleGamesMain.bjWins);
                            finalDisplay += "\nDealer has 21! You tied, so your bet of " + ConsoleGamesMain.currentBet + " was returned to your account.\n\n";
                            ConsoleGamesMain.currentBet = 0;
                        }
                        else //Dealer loses, either bust or standing at less than 21
                        {
                            banker.MoneyManip(__terminal, ConsoleGamesMain.currentBet * 2);
                            ConsoleGamesMain.bjGames++;
                            ConsoleGamesMain.bjWins++;
                            ConsoleGamesMain.creditEarnings += ConsoleGamesMain.currentBet;
                            PlayerPrefs.SetInt("BJGames", ConsoleGamesMain.bjGames); PlayerPrefs.SetInt("BJWins", ConsoleGamesMain.bjWins); PlayerPrefs.SetInt("Earnings", ConsoleGamesMain.creditEarnings);
                            if (dealersVal > 21) //Bust
                            {
                                finalDisplay += "\nDealer has " + dealersVal + ", that's a bust! You win " + ConsoleGamesMain.currentBet + " credits.\n\n";
                                ConsoleGamesMain.currentBet = 0;
                            }
                            else //Stand
                            {
                                finalDisplay += "\nDealer has " + dealersVal + ", so they must stand. You win " + ConsoleGamesMain.currentBet + " credits.\n\n";
                                ConsoleGamesMain.currentBet = 0;
                            }

                        }
                        resultNode = CreateTerminalNode(finalDisplay);

                    }
                    resultNode.clearPreviousText = true;
                }
                else if (cardTotal < 21) //Still playing
                {
                    resultNode = CreateTerminalNode("Dealer card:\n" + dealerCardsDisplayed + "\nYour hand:\n" + cardsDisplayed + "\nHit, or stand?\n\n");
                }
                else //More than 21
                {
                    int cardIndex = HasAnAce(false);
                    if (cardIndex != -1 && (cardTotal - 10 < 21)) //Has an ace and it saves from busting
                    {
                        resultNode = CreateTerminalNode("Dealer card:\n" + dealerCardsDisplayed + "\nYour hand:\n" + cardsDisplayed + "\nHit, or stand?\n\n");
                        ConsoleGamesMain.playerBJCards[cardIndex].cardValue = 1;
                    }
                    else //Either has no ace or ace doesn't save them
                    {
                        ConsoleGamesMain.playingBJ = false;
                        ConsoleGamesMain.bettingBJ = true;
                        resultNode = CreateTerminalNode("Dealer card:\n" + dealerCardsDisplayed + "\nYour hand:\n" + cardsDisplayed + "\nThat's a bust. You lost " + ConsoleGamesMain.currentBet + " credits.\n\n");
                        ConsoleGamesMain.creditEarnings -= ConsoleGamesMain.currentBet;
                        ConsoleGamesMain.bjGames++;
                        PlayerPrefs.SetInt("BJGames", ConsoleGamesMain.bjGames); PlayerPrefs.SetInt("Earnings", ConsoleGamesMain.creditEarnings);
                        ConsoleGamesMain.currentBet = 0;
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

        public static TerminalNode DoubleBlackJack(Terminal __terminal)
        {
            Banker banker = new Banker();
            int dealersVal = ConsoleGamesMain.dealerBJCards[0].cardValue + ConsoleGamesMain.dealerBJCards[1].cardValue;
            ConsoleGamesMain.displayDealerCardVals.Add(ConsoleGamesMain.dealerBJCards[1].randoCard); ConsoleGamesMain.displayDealerCardSuits.Add(ConsoleGamesMain.dealerBJCards[1].randoSuit);
            string dealerCardsDisplayed = DisplayCards(ConsoleGamesMain.displayDealerCardVals, ConsoleGamesMain.displayDealerCardSuits);
            int cardTotal = 0;
            int finalResult = 0; //1 is a player win, 2 is a dealer win, 3 is a tie.
            TerminalNode resultNode;
            if (ConsoleGamesMain.playingBJ && ConsoleGamesMain.playerBJCards.Count == 2 )
            {
                if (Banker.PocketWatch(__terminal, ConsoleGamesMain.currentBet))
                {
                    ConsoleGamesMain.playerBJCards.Add(new BJCard());
                    ConsoleGamesMain.displayPlayerCardVals.Add(ConsoleGamesMain.playerBJCards[ConsoleGamesMain.playerBJCards.Count - 1].randoCard);
                    ConsoleGamesMain.displayPlayerCardSuits.Add(ConsoleGamesMain.playerBJCards[ConsoleGamesMain.playerBJCards.Count - 1].randoSuit);
                    for (int index = 0; index < ConsoleGamesMain.playerBJCards.Count; index++)
                    {
                        cardTotal += ConsoleGamesMain.playerBJCards[index].cardValue;
                    }
                    string cardsDisplayed = DisplayCards(ConsoleGamesMain.displayPlayerCardVals, ConsoleGamesMain.displayPlayerCardSuits);
                    ConsoleGamesMain.finalDisplay = "You doubled down. ";
                    if (cardTotal > 21 && HasAnAce(false) == -1)
                    {
                        banker.MoneyManip(__terminal, ConsoleGamesMain.currentBet * -1);
                        ConsoleGamesMain.finalDisplay += "You busted!\nYour hand:\n" + cardsDisplayed + "You lost " + ConsoleGamesMain.currentBet * 2 + " credits.\n\n";
                        finalResult = 2;
                        TerminalNode bustNode = CreateTerminalNode(ConsoleGamesMain.finalDisplay);
                        bustNode.clearPreviousText = true;
                        return bustNode;
                    }
                    ConsoleGamesMain.currentBet *= 2;
                    if (dealersVal >= 17)
                    {
                        ConsoleGamesMain.finalDisplay += "Dealer must stand.\nDealer's cards:\n" + dealerCardsDisplayed + "\nYour hand:\n" + cardsDisplayed;
                    }
                    while (dealersVal < 17) //Dealer draws until they reach 17
                    {
                        ConsoleGamesMain.dealerBJCards.Add(new BJCard());
                        ConsoleGamesMain.displayDealerCardVals.Add(ConsoleGamesMain.dealerBJCards[ConsoleGamesMain.dealerBJCards.Count - 1].randoCard);
                        ConsoleGamesMain.displayDealerCardSuits.Add(ConsoleGamesMain.dealerBJCards[ConsoleGamesMain.dealerBJCards.Count - 1].randoSuit);
                        dealersVal += ConsoleGamesMain.dealerBJCards[ConsoleGamesMain.dealerBJCards.Count - 1].cardValue;
                        if (dealersVal > 21)
                        {
                            int cardIndex = HasAnAce(true);
                            if (cardIndex != -1)
                            {
                                ConsoleGamesMain.dealerBJCards[cardIndex].cardValue = 1;
                                dealersVal -= 10;
                            }
                        }
                    }
                    if (ConsoleGamesMain.dealerBJCards.Count > 2)
                    {
                        dealerCardsDisplayed = DisplayCards(ConsoleGamesMain.displayDealerCardVals, ConsoleGamesMain.displayDealerCardSuits);
                        ConsoleGamesMain.finalDisplay += "Dealer must draw.\nDealer's cards:\n" + dealerCardsDisplayed + "\nYour hand:\n" + cardsDisplayed;
                    }
                    if (dealersVal > 21)
                    {
                        banker.MoneyManip(__terminal, ConsoleGamesMain.currentBet * 4);
                        ConsoleGamesMain.finalDisplay += "The dealer busted! You won " + ConsoleGamesMain.currentBet * 2 + " credits.\n\n";
                        finalResult = 1;
                    }
                    else if (dealersVal < cardTotal)
                    {
                        banker.MoneyManip(__terminal, ConsoleGamesMain.currentBet * 4);
                        ConsoleGamesMain.finalDisplay += "You won " + ConsoleGamesMain.currentBet * 2 + " credits.\n\n";
                        finalResult = 1;
                    }
                    else if (dealersVal > cardTotal)
                    {
                        banker.MoneyManip(__terminal, ConsoleGamesMain.currentBet * -1);
                        ConsoleGamesMain.finalDisplay += "You lost " + ConsoleGamesMain.currentBet * 2 + " credits.\n\n";
                        finalResult = 2;
                    }
                    else
                    {
                        banker.MoneyManip(__terminal, ConsoleGamesMain.currentBet * 2);
                        ConsoleGamesMain.finalDisplay += "A tie! Your bet of " + ConsoleGamesMain.currentBet * 2 + " credits was returned to your account.\n\n";
                        finalResult = 3;
                    }
                    SaveBJStats(finalResult);
                    ConsoleGamesMain.playingBJ = false;
                    ConsoleGamesMain.bettingBJ = true;
                }
                else
                {
                    string cardsDisplayed = DisplayCards(ConsoleGamesMain.displayPlayerCardVals, ConsoleGamesMain.displayPlayerCardSuits);
                    ConsoleGamesMain.finalDisplay = "You don't have enough money!\nHit, or stand?\n" + cardsDisplayed;
                }
                TerminalNode returnNode = CreateTerminalNode(ConsoleGamesMain.finalDisplay);
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

        public static void SaveBJStats(int finalResult)
        {
            if (finalResult == 1) //Win
            {
                ConsoleGamesMain.bjGames++;
                ConsoleGamesMain.bjWins++;
                ConsoleGamesMain.creditEarnings += ConsoleGamesMain.currentBet;
                PlayerPrefs.SetInt("BJGames", ConsoleGamesMain.bjGames); PlayerPrefs.SetInt("BJWins", ConsoleGamesMain.bjWins); PlayerPrefs.SetInt("Earnings", ConsoleGamesMain.creditEarnings);
            }
            else if (finalResult == 2) //Loss
            {
                ConsoleGamesMain.bjGames++;
                ConsoleGamesMain.creditEarnings -= ConsoleGamesMain.currentBet;
                PlayerPrefs.SetInt("BJGames", ConsoleGamesMain.bjGames); PlayerPrefs.SetInt("Earnings", ConsoleGamesMain.creditEarnings);
            }
            else //Tie (counts as a win in stats)
            {
                ConsoleGamesMain.bjGames++;
                ConsoleGamesMain.bjWins++;
                PlayerPrefs.SetInt("BJGames", ConsoleGamesMain.bjGames); PlayerPrefs.SetInt("BJWins", ConsoleGamesMain.bjWins);
            }
        }
        public static TerminalNode StandBlackJack(Terminal __terminal)
        {
            Banker banker = new Banker();
            ConsoleGamesMain.displayDealerCardVals.Add(ConsoleGamesMain.dealerBJCards[1].randoCard); ConsoleGamesMain.displayDealerCardSuits.Add(ConsoleGamesMain.dealerBJCards[1].randoSuit);
            string dealerCardsDisplayed = DisplayCards(ConsoleGamesMain.displayDealerCardVals, ConsoleGamesMain.displayDealerCardSuits);
            int finalResult = 0; //1 is a player win, 2 is a dealer win, 3 is a tie.
            if (ConsoleGamesMain.playingBJ)
            {
                TerminalNode finalNode;
                int cardTotal = 0;
                for (int index = 0; index < ConsoleGamesMain.playerBJCards.Count; index++)
                {
                    cardTotal += ConsoleGamesMain.playerBJCards[index].cardValue;
                }
                string finalDisplay = "You stood on " + cardTotal + ".";
                int dealersVal = ConsoleGamesMain.dealerBJCards[0].cardValue + ConsoleGamesMain.dealerBJCards[1].cardValue;
                if (dealersVal >= 17) //Dealer stands on all 17s.
                {
                    finalDisplay += " Dealer must stand.\nDealer's cards:\n" + dealerCardsDisplayed + "\n";
                    if (cardTotal > dealersVal) //Player wins
                    {
                        banker.MoneyManip(__terminal, ConsoleGamesMain.currentBet * 2);
                        finalResult = 1;
                        finalDisplay += "You won " + ConsoleGamesMain.currentBet + " credits.\n\n";
                        ConsoleGamesMain.playingBJ = false;
                        ConsoleGamesMain.bettingBJ = true;
                    }
                    else if (cardTotal == dealersVal) //Tie
                    {
                        banker.MoneyManip(__terminal, ConsoleGamesMain.currentBet);
                        finalDisplay += "Your bet of " + ConsoleGamesMain.currentBet + " was returned to your account.\n\n";
                        finalResult = 3;
                    }
                    else    //Player loses
                    {
                        finalDisplay += "You lost " + ConsoleGamesMain.currentBet + " credits.\n\n";
                        finalResult = 2;

                    }
                    ConsoleGamesMain.playingBJ = false;
                    ConsoleGamesMain.bettingBJ = true;
                }
                else //if dealer has less than 17
                {
                    while (dealersVal < 17) //Dealer draws until they reach 17
                    {
                        ConsoleGamesMain.dealerBJCards.Add(new BJCard());
                        ConsoleGamesMain.displayDealerCardVals.Add(ConsoleGamesMain.dealerBJCards[ConsoleGamesMain.dealerBJCards.Count - 1].randoCard);
                        ConsoleGamesMain.displayDealerCardSuits.Add(ConsoleGamesMain.dealerBJCards[ConsoleGamesMain.dealerBJCards.Count - 1].randoSuit);
                        dealersVal += ConsoleGamesMain.dealerBJCards[ConsoleGamesMain.dealerBJCards.Count - 1].cardValue;
                        if (dealersVal > 21)
                        {
                            int cardIndex = HasAnAce(true);
                            if (cardIndex != -1)
                            {
                                ConsoleGamesMain.dealerBJCards[cardIndex].cardValue = 1;
                                dealersVal -= 10;
                            }
                        }
                    }
                    dealerCardsDisplayed = DisplayCards(ConsoleGamesMain.displayDealerCardVals, ConsoleGamesMain.displayDealerCardSuits);
                    finalDisplay += "Dealer must draw.\nDealer's cards:\n" + dealerCardsDisplayed + "\n";
                    if (dealersVal > 21) //Dealer busts
                    {
                        banker.MoneyManip(__terminal, ConsoleGamesMain.currentBet * 2);
                        finalResult = 1;
                        finalDisplay += "That's " + dealersVal + ", bust! You win " + ConsoleGamesMain.currentBet + " credits.\n\n";

                    }
                    else if (dealersVal == cardTotal) //Dealer ties 
                    {
                        finalResult = 3;
                        banker.MoneyManip(__terminal, ConsoleGamesMain.currentBet);
                        finalDisplay += "That's " + dealersVal + ", tie! Your bet of " + ConsoleGamesMain.currentBet + " was returned to your account.\n\n";
                    }
                    else if (dealersVal < cardTotal) //Dealer loses
                    {
                        finalResult = 1;
                        banker.MoneyManip(__terminal, ConsoleGamesMain.currentBet * 2);
                        finalDisplay += "That's " + dealersVal + ", so the dealer must stand. You win " + ConsoleGamesMain.currentBet + " credits.\n\n";
                    }
                    else //Dealer wins
                    {
                        finalResult = 2;
                        finalDisplay += "That's " + dealersVal + ", so the dealer must stand. You lose " + ConsoleGamesMain.currentBet + " credits.\n\n";
                    }
                    ConsoleGamesMain.playingBJ = false;
                    ConsoleGamesMain.bettingBJ = true;
                }
                SaveBJStats(finalResult);
                ConsoleGamesMain.currentBet = 0;
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
    }
}
