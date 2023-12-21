using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TerminalGames
{
    public class CardClasses //File that handles the different types of card classes needed for the games. Also houses card related methods.
    {
        public static List<OneDeckCard> discardList = new List<OneDeckCard>();
        public class BJCard //Card class for BJ. Multiple decks, so more than four of the same number is okay.
        {
            public int randoCard;
            public string valueRep;
            Terminal theTerminal;
            public int cardValue = 10; //The BJ value. 10 by default, if not a face it updates.
            public BJCard() //For making a new card in blackjack
            {
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



        public class DeckOfCards
        {
            public List<OneDeckCard> internalDeck;

            public DeckOfCards()
            {
                internalDeck = new List<OneDeckCard>();
                for (int value = 1; value <= 13; value++)
                {
                    for (int suit = 1; suit <= 4; suit++)
                    {
                        internalDeck.Add(new OneDeckCard(value, suit)); //Creates a deck of every card.
                    }
                }
            }
            public OneDeckCard Draw() //Chooses a random spot in the deck and removes it from the deck and gives it to whoever drew it.
            {
                int randomIndex = Random.Range(0, internalDeck.Count);
                OneDeckCard tempCard = internalDeck[randomIndex];
                internalDeck.RemoveAt(randomIndex);
                return tempCard;

            }
        }

        public class OneDeckCard //Card class for games where only one deck is in play such as Go Fish. 
        {
            public string valueRep;
            public string suitRep;
            public int cardValue;
            public int cardSuit;

            public OneDeckCard(int value, int suit)
            {
                cardValue = value;
                cardSuit = suit;
                switch (cardSuit)
                {
                    case 1:
                        suitRep = "diamonds";
                        break;
                    case 2:
                        suitRep = "hearts";
                        break;
                    case 3:
                        suitRep = "spades";
                        break;
                    default:
                        suitRep = "clubs";
                        break;
                }

                switch (cardValue)
                {
                    case 1:
                        valueRep = "the Ace";
                        break;
                    case 2:
                        valueRep = "the two";
                        break;
                    case 3:
                        valueRep = "the three";
                        break;
                    case 4:
                        valueRep = "the four";
                        break;
                    case 5:
                        valueRep = "the five";
                        break;
                    case 6:
                        valueRep = "the six";
                        break;
                    case 7:
                        valueRep = "the seven";
                        break;
                    case 8:
                        valueRep = "the eight";
                        break;
                    case 9:
                        valueRep = "the nine";
                        break;
                    case 10:
                        valueRep = "the ten";
                        break;
                    case 11:
                        valueRep = "the Jack";
                        break;
                    case 12:
                        valueRep = "the Queen";
                        break;
                    default:
                        valueRep = "the King";
                        break;
                }

            }
        }
        public static bool CardRemaining(DeckOfCards deck)   //Checks if there are any "cards" left in the "deck".
        {
            if (deck.internalDeck.Count == 0) //The deck has no cards left
            {
                return false;
            }
            return true;
        }
        public static int HasAnAce(bool isDealer) //Checks if there is an ace in the list. false for player, true for dealer.
        {
            int returnCard = -1;
            bool hasAnAce = false;
            List<BJCard> tempList;
            tempList = ConsoleGamesMain.dealerBJCards;
            if (!isDealer)
            {
                tempList = ConsoleGamesMain.playerBJCards;
            }
            for (int index = 0; index < tempList.Count; index++)
            {
                if (tempList[index].cardValue == 11)
                {
                    returnCard = index;
                }
            }
            return returnCard;
        }
        public static bool HasABook(bool isDealer) //Checks if either the player or the dealer has created a book(4 cards of the same number)
        {
            List<OneDeckCard> cardList = isDealer ? ConsoleGamesMain.dealerODCards : ConsoleGamesMain.playerODCards;
            var valuesCount = cardList
                .Select(card => card.valueRep)
                .GroupBy(value => value)
                .ToDictionary(group => group.Key, group => group.Count());

            foreach (var kvp in valuesCount)
            {
                if (kvp.Value == 4)
                {
                    string bookValue = kvp.Key;

                    if (isDealer)
                    {
                        ConsoleGamesMain.dealerBooks++;
                        ConsoleGamesMain.dealerODCards.RemoveAll(card => card.valueRep.Equals(bookValue, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        ConsoleGamesMain.playerBooks++;
                        ConsoleGamesMain.playerODCards.RemoveAll(card => card.valueRep.Equals(bookValue, StringComparison.OrdinalIgnoreCase));
                    }

                    return true; // Indicates a book was found and processed
                }
            }

            return false; // No book found
        }
    }
}
