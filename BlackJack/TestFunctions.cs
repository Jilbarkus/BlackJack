using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CardGames
{
    internal class TestFunctions
    {

        public static void TestDisplayNewDeck()
        {
            aCard[] deck = GameCards.BJDeck();
            deck = deck.Shuffle();
            string[] currentOutput = new string[0];
            for (int i = 0; i < deck.Length; i++)
            {
                currentOutput = currentOutput.AddStringBlockHorizontal(deck[i].CardToString(), GlobalFunctions.CursorMaxX);
            }
            currentOutput.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Center);
        }
        public static bool TestBlackJackInputs()
        {
            switch (getBettingInput())
            {
                case 0: return false;
                case 1:
                    //TestFunctions.ShowScreenCorners2();
                    Console.Clear();
                    string[] testB = new string[] { "Testx", "Home", "dkey ated as ordinary input or as an interruption that is handled by the operating system" };
                    //string[] buttonsB = GlobalFunctions.CreateButtonStringArray(testB, GlobalFunctions.CursorMaxX);
                    //buttonsB.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Bottom);

                    bool[] hiLighted = new bool[] { true, true, true };
                    string[] buttons2 = GlobalFunctions.CreateButtonStringArray(testB, ref hiLighted, GlobalFunctions.CursorMaxX);
                    buttons2.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Bottom, hiLighted);
                    return true;
                case 2:
                    Console.Clear();
                    //string[] test2 = new string[] { "Testx", "Home", "dkey ated as ordinary " };
                    //string[] buttons2 = GlobalFunctions.CreateHorizontalButtonStringRowsArray()
                    string[] test = new string[] { "Testx", "Home", "dkey ated as ordinary " };
                    string[] buttons = GlobalFunctions.CreateHorizontalButtonStringRowsArray(test, null, GlobalFunctions.CursorMaxX, out Dictionary<IntVector2, int> hilightedOut);
                    buttons.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Center);
                    //test.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Right, JustifyY.Top);
                    //ContentPage.BottomHorizontalButtonPage buttonsContent = new ContentPage.BottomHorizontalButtonPage(test, JustifyX.Center);
                    //buttonsContent.WriteContent(1);
                    return true;
                case 3:
                    //Console.Clear();
                    //                            string[] cardTest = new string[]
                    //                            {
                    //@"+---------+",
                    //@"|         |",
                    //@"|         |",
                    //@"|         |",
                    //@"|         |",
                    //@"|         |",
                    //@"+---------+"
                    //                            };
                    //                            //cardTest[1] = cardTest[1].Insert(1, "g");
                    //                            cardTest.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Top);

                    //                            Console.Clear();
                    //                            char suitTest = '$';
                    //                            char cardValueTest = '7';
                    //                            string[] cardTest2 = new string[]
                    //                            {
                    //@"+---------+",
                    //$@"|{cardValueTest}{suitTest}       |",
                    //@"|         |",
                    //$@"|    {suitTest}    |",
                    //@"|         |",
                    //$@"|       {cardValueTest}{suitTest}|",
                    //@"+---------+"
                    //                            };
                    //                            cardTest2.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Top);

                    //                            return true;

                    //    Console.Clear();
                    //    string[] cardTest = new string[]
                    //    {
                    //@"+---------+",
                    //@"|         |",
                    //@"|         |",
                    //@"|         |",
                    //@"|         |",
                    //@"|         |",
                    //@"+---------+"
                    //    };
                    //    string[] cardTest2 = new string[cardTest.Length];
                    //    int extraCardsNum = 5;
                    //    for (int i = 0; i < cardTest.Length; i++)
                    //    {
                    //        StringBuilder sb2 = new StringBuilder();
                    //        for (int j = 0; j < extraCardsNum; j++) sb2.Append(cardTest[i]);
                    //        cardTest2[i] = sb2.ToString();
                    //    }
                    //    cardTest2.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Top);
                    //    //TestFunctions.UnicodeTest();
                    //    aCard[] deck = GameCards.BJDeck();
                    //    StringBuilder sb = new StringBuilder();
                    //    for (int i = 0; i < deck.Length; i++) sb.Append(GlobalFunctions.CardToString(deck[i])[0]);
                    //    string[] suits = new string[] { sb.ToString() };
                    //    suits.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Bottom);

                    Console.Clear();
                    string[] cardCollection = new string[0];
                    //FaceCard faceCard = new FaceCard(aCard.Suit.Heart, FaceCard.Face.King);
                    FaceCard faceCard = new FaceCard();
                    string[] cardString = GlobalFunctions.CardToString(faceCard);
                    cardCollection = cardCollection.AddStringBlockHorizontal(cardString, GlobalFunctions.CursorMaxX);
                    NumberCard numberCard = new NumberCard(aCard.Suit.Club, 7);
                    cardString = GlobalFunctions.CardToString(numberCard);
                    cardCollection = cardCollection.AddStringBlockHorizontal(cardString, GlobalFunctions.CursorMaxX);

                    for (int i = 0; i < 12; i++)
                    {
                        numberCard = new NumberCard();
                        cardString = GlobalFunctions.CardToString(numberCard);
                        cardCollection = cardCollection.AddStringBlockHorizontal(cardString, GlobalFunctions.CursorMaxX);
                    }
                    cardCollection.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Bottom);
                    return true;
                default: return true;
            }

            int getBettingInput()
            {
                ConsoleKeyInfo[] validKeys = new ConsoleKeyInfo[]
                {
                new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false),
                new ConsoleKeyInfo('1', ConsoleKey.UpArrow, false, false, false),
                new ConsoleKeyInfo('2', ConsoleKey.DownArrow, false, false, false),
                new ConsoleKeyInfo('3', ConsoleKey.Spacebar, false, false, false)
                };
                return GlobalFunctions.GetKeyPress(validKeys);
            }
        }
        public static void TestBlackJackDeck(aCard[] deck)
        {
            if (deck == null) deck = GameCards.BJDeck();
            Console.WriteLine($"{deck.Length} Cards: \n");
            for (int i = 0; i < deck.Length; i++)
            {
                if (deck[i] is FaceCard)
                {
                    FaceCard faceCard = (FaceCard)deck[i];
                    Console.WriteLine($"{faceCard.GetSuit} {faceCard.GetFace} \n");
                }
                else if (deck[i] is NumberCard)
                {
                    NumberCard numberCard = (NumberCard)deck[i];
                    Console.WriteLine($"{numberCard.GetSuit} {numberCard.GetNumber} \n");
                }
                else
                {
                    Console.WriteLine("error: WTF lol?\n");
                }
            }
        }

        public static void UnicodeTest()
        {
            //// See https://aka.ms/new-console-template for more information
            

            ////Console.WriteLine($"{Console.OutputEncoding}");
            ////Console.Write('9824');
            Console.OutputEncoding = System.Text.Encoding.Default;
            //Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Write('\u2660');
            Console.Write('\u9824');
            Console.WriteLine();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            for (var i = 0; i <= 1000; i++)
            {
                Console.Write(Strings.ChrW(i));
                if (i % 50 == 0)
                { // break every 50 chars
                    Console.WriteLine();
                }
            }
            Console.ReadKey();
        }

        public static void ReflectionTest()
        {
            var type = typeof(IGame);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

            foreach (System.Type globalBehaviour in types)
            {

            }


        }

        public static void ShowScreenCorners()
        {
            Console.Clear();
            int originalX = Console.CursorLeft;
            int originalY = Console.CursorTop;
            Console.SetCursorPosition(0, 0);
            Console.Write('+');
            Console.SetCursorPosition(Console.BufferWidth - 1, 0);
            Console.Write('+');
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write('+');
            Console.SetCursorPosition(Console.BufferWidth - 1, Console.WindowHeight - 1);
            Console.Write('+');
            Console.SetCursorPosition(originalX, originalY);
        }

        public static void ShowScreenCorners2()
        {
            Console.Clear();
            GlobalFunctions.WriteAtDelta("+", 0, 0);
            GlobalFunctions.WriteAtDelta("+", 0, GlobalFunctions.CursorMaxY);
            GlobalFunctions.WriteAtDelta("+", GlobalFunctions.CursorMaxX, 0);
            GlobalFunctions.WriteAtDelta("+", GlobalFunctions.CursorMaxX, GlobalFunctions.CursorMaxY);
        }

        public static void ShowScreenCorners3()
        {
            Console.Clear();
            GlobalFunctions.WriteAt("o", 1, 1);
            GlobalFunctions.WriteAt("o", 1, GlobalFunctions.CursorMaxY - 1);
            GlobalFunctions.WriteAt("o", GlobalFunctions.CursorMaxX - 2, 1);
            GlobalFunctions.WriteAt("o", GlobalFunctions.CursorMaxX - 2, GlobalFunctions.CursorMaxY - 1);
        }
    }

    public static class TypeLoaderExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
