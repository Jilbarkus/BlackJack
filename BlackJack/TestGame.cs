using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames
{
    internal class TestGame : IGame
    {
        public bool Run(ref PlayerSave player)
        {
            Console.Clear();

            //return EmptyDeckValueTest();
            //return BestHandChooserTest();
            //return SplitViabilityTest();
            return DeckValueTest();
        }

        private bool BestHandChooserTest()
        {
            Console.Clear();
            new string[] { $"Best Hand Out Of Two Chooser", "","Type Deck A value then press Enter:", "", "X to Quit","" }.PrintArrayToConsole(GlobalFunctions.ConsoleCursorLocation, JustifyX.Center, JustifyY.Center);
            string? usrInput = Console.ReadLine();
            if (usrInput == null || (usrInput != null && (usrInput == "x" || usrInput == "X"))) { return false; }
            else if (usrInput != null && int.TryParse(usrInput, out int handA))
            {
                Console.Clear();
                new string[] { "Type Deck B value then press Enter:", "", "X to Quit", "" }.PrintArrayToConsole(GlobalFunctions.ConsoleCursorLocation, JustifyX.Center, JustifyY.Center);
                usrInput = Console.ReadLine();
                if (usrInput == null || (usrInput != null && (usrInput == "x" || usrInput == "X"))) { return false; }
                else if (usrInput != null && int.TryParse(usrInput, out int handB))
                {
                    Console.Clear();
                    new string[] { $"A:{handA} versus B:{handB},     RESULT: {BlackJack.GetBestBlackJackValue(handA, handB)}", "", "X to Quit", "" }.PrintArrayToConsole(GlobalFunctions.ConsoleCursorLocation, JustifyX.Center, JustifyY.Center);
                    char usrInput2 = GetUserKeyToChar();
                    if (usrInput2=='x'|| usrInput2 == 'X') return false;
                    else return true;
                }
                else return true;
            }
            else return true;
        }

        private bool SplitViabilityTest()
        {
            Console.Clear();
            new string[] { 
                $"Two Aces Splittable: {BlackJack.CardsAreSplittable(new NumberCard(aCard.Suit.Spade, 1), new NumberCard(aCard.Suit.Heart, 1))}",
                $"Two 10s Splittable: {BlackJack.CardsAreSplittable(new NumberCard(aCard.Suit.Spade, 10), new NumberCard(aCard.Suit.Heart, 10))}",
                $"An ace and a 10 Splittable: {BlackJack.CardsAreSplittable(new NumberCard(aCard.Suit.Spade, 10), new NumberCard(aCard.Suit.Heart, 1))}",
                $"Two Kings: {BlackJack.CardsAreSplittable(new FaceCard(aCard.Suit.Spade, FaceCard.Face.King), new FaceCard(aCard.Suit.Diamond, FaceCard.Face.King))}",
                $"King and Jack: {BlackJack.CardsAreSplittable(new FaceCard(aCard.Suit.Spade, FaceCard.Face.Jack), new FaceCard(aCard.Suit.Diamond, FaceCard.Face.King))}",
                $"Ace and Jack: {BlackJack.CardsAreSplittable(new NumberCard(aCard.Suit.Spade, 1), new FaceCard(aCard.Suit.Diamond, FaceCard.Face.King))}"
            }.PrintArrayToConsole(GlobalFunctions.ConsoleCursorLocation, JustifyX.Center, JustifyY.Center);
            char keyToChar = GetUserKeyToChar();
            if (keyToChar == 'X' || Char.ToUpper(keyToChar) == 'X') return false;
            else return true;
        }

        private bool EmptyDeckValueTest()
        {
            aCard[] emptyDeck = new aCard[0];
            new string[] { $"empty deck value: {BlackJack.GetBlackJackValue(emptyDeck)}", "", "X to Quit" }.PrintArrayToConsole(GlobalFunctions.ConsoleCursorLocation, JustifyX.Center, JustifyY.Center);
            char keyToChar = GetUserKeyToChar();
            if (keyToChar == 'X' || Char.ToUpper(keyToChar) == 'X') return false;
            else return true;
        }

        private bool DeckValueTest()
        {
            Console.Clear();
            aCard[] hand = new aCard[0];
            bool bKeepGoing = true;
            Console.WriteLine("Enter value for number card");
            Console.WriteLine("Letter for face card");
            Console.WriteLine("Blank to reset hand");
            Console.WriteLine("X to Quit");
            while (bKeepGoing)
            {
                Console.WriteLine($"hand value: {BlackJack.GetBlackJackValue(hand)}");
                string? usrInput = Console.ReadLine();
                if (usrInput == null || (usrInput != null && (usrInput == "x" || usrInput == "X"))) { return false; }
                else if (usrInput != null &&
                    int.TryParse(usrInput, out int numCardValue) &&
                    0 < numCardValue &&
                    numCardValue < 11)
                {
                    NumberCard numberCard = new NumberCard(aCard.Suit.Heart, numCardValue);
                    if (numberCard != null)
                    {
                        hand = hand.Append(numberCard).ToArray();
                    }
                }
                else if (usrInput != null && 
                    (usrInput == "K" || usrInput == "k"))
                {
                    FaceCard faceCard = new FaceCard(aCard.Suit.Heart, FaceCard.Face.King);
                    if (faceCard != null)
                    {
                        hand = hand.Append(faceCard).ToArray();
                    }
                }
                else if (usrInput != null &&
                    (usrInput == "Q" || usrInput == "q"))
                {
                    FaceCard faceCard = new FaceCard(aCard.Suit.Heart, FaceCard.Face.Queen);
                    if (faceCard != null)
                    {
                        hand = hand.Append(faceCard).ToArray();
                    }
                }
                else if (usrInput != null &&
                    (usrInput == "J" || usrInput == "j"))
                {
                    FaceCard faceCard = new FaceCard(aCard.Suit.Heart, FaceCard.Face.Jack);
                    if (faceCard != null)
                    {
                        hand = hand.Append(faceCard).ToArray();
                    }
                }
                else if (usrInput != null &&
                    (usrInput == "" || usrInput == " "))
                {
                    hand = new aCard[0];
                }
            }

            return false;
        }

        private char GetUserKeyToChar()
        {
            ConsoleKeyInfo keyPressed;
            char keyToChar = '-';
            try
            {
                keyPressed = Console.ReadKey();
                keyToChar = keyPressed.KeyChar;
            }
            catch
            {
                keyToChar = '-';
            }
            return keyToChar;
        }
    }
}
