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
            return BestHandChooserTest();
            //return EmptyDeckValueTest();
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

        private bool EmptyDeckValueTest()
        {
            aCard[] emptyDeck = new aCard[0];
            new string[] { $"empty deck value: {BlackJack.GetBlackJackValue(emptyDeck)}", "", "X to Quit" }.PrintArrayToConsole(GlobalFunctions.ConsoleCursorLocation, JustifyX.Center, JustifyY.Center);
            char keyToChar = GetUserKeyToChar();
            if (keyToChar == 'X' || Char.ToUpper(keyToChar) == 'X') return false;
            else return true;
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
