using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames
{
    internal class DoMath : IGame
    {
        private int _questionsAsked = 0;
        bool IGame.Run(ref PlayerSave player)
        {
            if (_questionsAsked == 0)
            {
                Console.WriteLine($"Welcome to the math room {player.Name}");
                Console.WriteLine("Enter X at any time to quit to main menu");
            }
            return MathQuestion(ref player);
        }

        private bool MathQuestion(ref PlayerSave player)
        {
            _questionsAsked++;
            Random random = new Random();
            decimal winningsPerQuestion = (decimal)random.Next(1,11);
            int numA;
            int numB;
            int answer;
            string question;

            switch (random.Next(0, 3)) 
            {
                // addition question
                case 0:
                    numA = random.Next(0, 500);
                    numB = random.Next(0, 500);
                    answer = numA + numB;
                    question = $"{numA} + {numB} = ?";
                    break;
                case 1:
                    numA = random.Next(1, 301);
                    numB = random.Next(0, numA);
                    answer = numA - numB;
                    question = $"{numA} - {numB} = ?";
                    break;
                case 2:
                    numA = random.Next(1, 100);
                    numB = random.Next(1, 10);
                    answer = numA * numB;
                    question = $"{numA} x {numB} = ?";
                    break;
                default:
                    Console.WriteLine("error");
                    return false;
            }

            Console.WriteLine(question);
            Console.WriteLine("");
            string? usrInput = Console.ReadLine();
            Console.WriteLine("");
            if (usrInput == null) return true;
            if (usrInput.Contains('x') || usrInput.Contains('X'))
            {
                return false;
            }
            else if (int.TryParse(usrInput, out int usrAnswer))
            {
                if (usrAnswer == answer)
                {
                    Console.WriteLine($"Great job kid! here's ${winningsPerQuestion}");
                    player.AddCash(winningsPerQuestion);
                }
                else
                {
                    Console.WriteLine($"Unlucky! correct answer was {answer}");
                }
            }
            int cursorX = Console.CursorLeft;
            int cursorY = Console.CursorTop;
            Console.CursorLeft = 0; 
            Console.CursorTop = 0;
            new string[] { $"            Bank:${player.Cash}"}.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Right, JustifyY.Top);
            Console.CursorLeft = cursorX;
            Console.CursorTop = cursorY;
            return true;
        }
    }
}
