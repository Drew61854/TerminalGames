using System;
using System.Collections.Generic;
using System.Linq;
using static TerminalGames.CardClasses;
using static TerminalApi.TerminalApi;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TerminalGames
{
    internal class GoFishMethods
    {

        public static TerminalNode BeginGoFish(Terminal __terminal)
        {
            string finalDisplay = "May the best fisher win!\nYour hand:\n";
            ConsoleGamesMain.playerODCards.Clear();
            ConsoleGamesMain.dealerODCards.Clear();
            ConsoleGamesMain.displayPlayerCardVals.Clear(); ConsoleGamesMain.displayPlayerCardSuits.Clear();
            ConsoleGamesMain.displayDealerCardVals.Clear(); ConsoleGamesMain.displayDealerCardSuits.Clear();
            ConsoleGamesMain.deck = new DeckOfCards();
            for (int x = 0; x < 7; x++)
            {
                ConsoleGamesMain.playerODCards.Add(ConsoleGamesMain.deck.Draw());
                ConsoleGamesMain.dealerODCards.Add(ConsoleGamesMain.deck.Draw());
            }
            ConsoleGamesMain.playingGoFish = true;
            List<OneDeckCard> sortedPlayerHand = ConsoleGamesMain.playerODCards.OrderBy(card => card.cardValue).ToList();
            foreach (OneDeckCard card in sortedPlayerHand)
            {
                ConsoleGamesMain.displayPlayerCardVals.Add(card.cardValue); ConsoleGamesMain.displayPlayerCardSuits.Add(card.cardSuit);
            }
            string addedDisplay = DisplayCards(ConsoleGamesMain.displayPlayerCardVals, ConsoleGamesMain.displayPlayerCardSuits);
            finalDisplay += addedDisplay;
            finalDisplay += "It's your turn, request a card from your opponent. (Format 'request [card]' in words)\n\n";
            TerminalNode returnNode = CreateTerminalNode(finalDisplay);
            returnNode.clearPreviousText = true;
            return returnNode;
        }

        public static TerminalNode RequestGoFish(Terminal __terminal)
        {
            ConsoleGamesMain.finalDisplay = "";
            string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
            input = input.ToLower();
            input = input.Substring(8);
            TerminalNode returnNode;
            if (ConsoleGamesMain.playingGoFish)
            {
                int numCards = 0;
                if (GoFishMethods.PlayerTurn(input)) //PlayerTurn returns false if it cannot parse the input. If it does not return false it handles all of the player's turn.
                {
                    if (ConsoleGamesMain.playingGoFish)  //If the game didn't end on the player's turn
                        GoFishMethods.OpponentTurn(); //Handles the opponents turn.
                }
                returnNode = CreateTerminalNode(ConsoleGamesMain.finalDisplay);
                returnNode.clearPreviousText = true;
                return returnNode;
            }
            else
            {
                ConsoleGamesMain.finalDisplay = "There was an issue with your request.\n\n";
            }
            returnNode = CreateTerminalNode(ConsoleGamesMain.finalDisplay);
            returnNode.clearPreviousText = true;
            return returnNode;
        }

        public static bool PlayerTurn(string input)
        {
            TerminalNode returnNode;
            if (!ConsoleGamesMain.cardNames.Contains(input))
            {
                ConsoleGamesMain.finalDisplay = "There was an issue with what you requested. Please try again (format 'request [card]' in words)\n\n";
                return false;
            }
            int numCards = HandlePlayerTurn(input);
            HandlePostTurn(numCards);
            return true;
        }

        private static int HandlePlayerTurn(string input)
        {
            int numCards = 0;
            for (int i = ConsoleGamesMain.dealerODCards.Count - 1; i >= 0; i--)
            {
                if (ConsoleGamesMain.dealerODCards.Count != 0 && ConsoleGamesMain.dealerODCards[i].valueRep.Substring(4).ToLower().Equals(input))
                {
                    numCards++;
                    ConsoleGamesMain.playerODCards.Add(ConsoleGamesMain.dealerODCards[i]);
                    ConsoleGamesMain.dealerODCards.RemoveAt(i);
                }
            }

            ConsoleGamesMain.finalDisplay += "Your opponent had " + numCards;
            return numCards;
        }

        private static void HandlePostTurn(int numCards)
        {
            if (numCards == 0 && CardRemaining(ConsoleGamesMain.deck))
            {
                ConsoleGamesMain.playerODCards.Add(ConsoleGamesMain.deck.Draw());
                ConsoleGamesMain.finalDisplay += "\nGo fish! You drew " + ConsoleGamesMain.playerODCards[ConsoleGamesMain.playerODCards.Count - 1].valueRep + " of " + ConsoleGamesMain.playerODCards[ConsoleGamesMain.playerODCards.Count - 1].suitRep;
            }
            else if (numCards == 0 && !CardRemaining(ConsoleGamesMain.deck))
            {
                ConsoleGamesMain.finalDisplay += "\nNo more cards in the deck!";
            }

            if (HasABook(false))
            {
                ConsoleGamesMain.finalDisplay += "\nYou completed a book! That makes " + ConsoleGamesMain.playerBooks + " for you, and " + ConsoleGamesMain.dealerBooks + " for your opponent.\n";
            }

            if (!CardRemaining(ConsoleGamesMain.deck) && (ConsoleGamesMain.dealerODCards.Count + ConsoleGamesMain.playerODCards.Count == 0))
            {
                ConsoleGamesMain.finalDisplay += GetGameResultMessage();
                ConsoleGamesMain.playingGoFish = false;

            }
        }

        private static string GetGameResultMessage()
        {
            string resultMessage = "That's game! With a final score of " + ConsoleGamesMain.playerBooks + " books to " + ConsoleGamesMain.dealerBooks + ", ";
            resultMessage += (ConsoleGamesMain.playerBooks > ConsoleGamesMain.dealerBooks) ? "you win!\n\n" : "your opponent wins! Better luck next time.\n\n";
            ConsoleGamesMain.fishGames++;
            if (ConsoleGamesMain.playerBooks > ConsoleGamesMain.dealerBooks)
            {
                ConsoleGamesMain.fishWins++;
                PlayerPrefs.SetInt("FishWins", ConsoleGamesMain.fishGames);
            }
            ConsoleGamesMain.averageBooks = ConsoleGamesMain.averageBooks + ConsoleGamesMain.playerBooks;
            PlayerPrefs.SetInt("FishGames", ConsoleGamesMain.fishGames); PlayerPrefs.SetInt("AvgBooks", ConsoleGamesMain.averageBooks);
            return resultMessage;
        }

        public static void OpponentTurn()
        {
            int seed = Random.Range(1, 11);
            int askFor = Random.Range(0, ConsoleGamesMain.dealerODCards.Count);
            askFor = GoFishAI(askFor, seed);
            string askForRep = GetCardRepresentation(askFor);

            ConsoleGamesMain.finalDisplay += "\nIt's your opponent's turn. They ask for " + askForRep;
            int secondNumCards = HandleOpponentTurn(askFor);
            HandlePostOpponentTurn(secondNumCards);
        }

        private static int GoFishAI(int original, int seed)
        {
            if (seed <= 8)
            {
                if (original >= 0 && original < ConsoleGamesMain.dealerODCards.Count)
                {
                    original = ConsoleGamesMain.dealerODCards[original].cardValue;
                }
                else if (ConsoleGamesMain.dealerODCards.Count > 0)
                {
                    original = Random.Range(0, ConsoleGamesMain.dealerODCards.Count);
                    original = ConsoleGamesMain.dealerODCards[original].cardValue;
                }
                else
                {
                    original = Random.Range(1, 14);
                    while (ConsoleGamesMain.bookedCards.Contains(original))
                    {
                        original = Random.Range(1, 14);
                    }
                }
            }
            else
            {
                original = Random.Range(1, 14);
                while (ConsoleGamesMain.bookedCards.Contains(original))
                {
                    original = Random.Range(1, 14);
                }
            }
            int tries = 0;
            while (ConsoleGamesMain.lastCardAsked == original && ConsoleGamesMain.dealerODCards.Count > 0)
            {
                original = ConsoleGamesMain.dealerODCards[Random.Range(0, ConsoleGamesMain.dealerODCards.Count)].cardValue;
                tries++;
                if (tries == 25) //Ugly, ugly way to handle this.
                {
                    original = Random.Range(1, 14);
                    break;
                }
            }
            ConsoleGamesMain.lastCardAsked = original;
            return original;
        }
        private static int HandleOpponentTurn(int askFor)
        {
            int secondNumCards = 0;

            for (int i = 0; i < ConsoleGamesMain.playerODCards.Count; i++)
            {
                if (ConsoleGamesMain.playerODCards[i].cardValue == askFor)
                {
                    secondNumCards++;
                    ConsoleGamesMain.dealerODCards.Add(ConsoleGamesMain.playerODCards[i]);
                    ConsoleGamesMain.playerODCards.RemoveAt(i);
                    i--;
                }
            }

            ConsoleGamesMain.finalDisplay += ". You had " + secondNumCards;
            return secondNumCards;
        }

        private static void HandlePostOpponentTurn(int secondNumCards)
        {
            if (secondNumCards == 0 && CardRemaining(ConsoleGamesMain.deck))
            {
                ConsoleGamesMain.dealerODCards.Add(ConsoleGamesMain.deck.Draw());
                ConsoleGamesMain.finalDisplay += "\nGo fish! Your opponent drew a card.";
            }
            else if (!CardRemaining(ConsoleGamesMain.deck) && secondNumCards == 0)
            {
                ConsoleGamesMain.finalDisplay += "\nNo more cards in the deck!";
            }

            if (HasABook(true))
            {
                ConsoleGamesMain.finalDisplay += "\nYour opponent completed a book! That makes " + ConsoleGamesMain.playerBooks + " for you, and " + ConsoleGamesMain.dealerBooks + " for your opponent.\n";
            }

            if (!CardRemaining(ConsoleGamesMain.deck) && (ConsoleGamesMain.dealerODCards.Count + ConsoleGamesMain.playerODCards.Count == 0))
            {
                ConsoleGamesMain.finalDisplay += GetGameResultMessage();
                ConsoleGamesMain.playingGoFish = false;
            }
            else
            {
                DisplayPlayerHand();
            }
        }

        private static void DisplayPlayerHand()
        {
            ConsoleGamesMain.finalDisplay += "\nYour hand:\n";

            List<OneDeckCard> sortedPlayerHand = ConsoleGamesMain.playerODCards.OrderBy(card => card.cardValue).ToList();
            ConsoleGamesMain.displayPlayerCardSuits.Clear(); ConsoleGamesMain.displayPlayerCardVals.Clear();
            foreach (OneDeckCard card in sortedPlayerHand)
            {
                ConsoleGamesMain.displayPlayerCardVals.Add(card.cardValue); ConsoleGamesMain.displayPlayerCardSuits.Add(card.cardSuit);
            }
            ConsoleGamesMain.finalDisplay += DisplayCards(ConsoleGamesMain.displayPlayerCardVals, ConsoleGamesMain.displayPlayerCardSuits);
            ConsoleGamesMain.finalDisplay += "It's your turn, request a card from your opponent. (Format 'request [card]' in words)\n\n";
        }

        private static string GetCardRepresentation(int cardValue)
        {
            string[] cardNames = { "aces", "twos", "threes", "fours", "fives", "sixes", "sevens", "eights", "nines", "tens", "jacks", "queens", "kings" };
            return cardNames[cardValue - 1];
        }
    }
}
