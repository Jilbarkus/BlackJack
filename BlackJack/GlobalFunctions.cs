using System.Reflection;
using System.Text;
using static CardGames.BlackJack;
//using static CardGames.ContentPage;

namespace CardGames
{

    public enum JustifyX { Left, Center, Right }
    public enum JustifyY { Top, Center, Bottom }
    public class IntVector2
    {
        public int X;
        public int Y;

        public IntVector2(int xIn, int yIn)
        {
            this.X = xIn;
            this.Y = yIn;
        }
    }
    internal static class GlobalFunctions
    {
        public static int CursorMinX => 0;
        public static int CursorMinY => 0;

        public static int CursorMaxX => Console.WindowWidth - 1;
        public static int CursorMaxY => Console.WindowHeight - 1;

        public static Random Rng = new Random();

        public static IntVector2 ConsoleCursorLocation => new IntVector2(Console.CursorLeft, Console.CursorTop);

        public static string GetPlayerSavePath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\csinoData.xml";

        // returns index of char that user inputed, if not valid then return -1
        public static int GetKeyPress(Char[] validChars)
        {
            ConsoleKeyInfo keyPressed;
            keyPressed = Console.ReadKey();
            int charzIndex =  Array.FindIndex(validChars, ch => ch.Equals(Char.ToUpper(keyPressed.KeyChar)) || ch.Equals(ch));
            return charzIndex;
        }

        public static int GetKeyPress(ConsoleKeyInfo[] validKeyCombos)
        {
            ConsoleKeyInfo keyPressed;
            keyPressed = Console.ReadKey();

            int charzIndex = Array.FindIndex(validKeyCombos, cki => cki.Key.Equals(keyPressed.Key) || (cki.Modifiers == keyPressed.Modifiers && (cki.KeyChar == Char.ToUpper(keyPressed.KeyChar) || Char.ToUpper(cki.KeyChar) == keyPressed.KeyChar)));

            //if (charzIndex  == -1) Console.WriteLine("\n Unknown Command: char: {0}, KeyCode: {1}, modifiers: {2}", keyPressed.KeyChar, keyPressed.Key, keyPressed.Modifiers );
            if (charzIndex == -1) return 0;
            return charzIndex;
        }

        public static IntVector2 WriteAtDelta(String s, int x, int y, bool highlight = false)
        {
            try
            {
                IntVector2 originalPos = new IntVector2(Console.CursorLeft, Console.CursorTop);

                if (Console.CursorLeft + x + s.Length > CursorMaxX)
                {
                    int splits = (int)Math.Ceiling((double)s.Length / CursorMaxX);
                    int sectionSize = (int)Math.Ceiling((double)s.Length / splits);
                    if (y > CursorMaxY * 0.5)
                    {
                        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop + y - splits);
                    }
                    StringBuilder sb = new StringBuilder(s);
                    for (int i = 0; i < splits; i++)
                    {
                        if (i != splits - 1)
                        {
                            string section = string.Empty;
                            try
                            {
                                section = sb.ToString(0, sectionSize);
                                if (section != string.Empty)
                                {
                                    sb.Remove(0, sectionSize);
                                    if (highlight) ReverseConsoleColours();
                                    Console.WriteLine(section);
                                    if (highlight) ResetConsoleColours();
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            s = sb.ToString();
                            if (highlight) ReverseConsoleColours();
                            Console.WriteLine(s);
                            if (highlight) ResetConsoleColours();
                        }
                    }
                }
                else
                {
                    Console.SetCursorPosition(Console.CursorLeft + x, Console.CursorTop + y);
                    if (highlight) ReverseConsoleColours();
                    Console.Write(s);
                    if (highlight) ResetConsoleColours();
                } 
                IntVector2 finalDelta = new IntVector2(Console.CursorLeft - originalPos.X, Console.CursorTop - originalPos.Y);
                Console.SetCursorPosition(originalPos.X, originalPos.Y);
                return finalDelta;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.Clear();
                IntVector2 originalPos = new IntVector2(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine(e.Message);
                IntVector2 finalDelta = new IntVector2(Console.CursorLeft - originalPos.X, Console.CursorTop - originalPos.Y);
                return finalDelta;
            }
        }

        // true if inside window and can write, otherwise return false
        public static bool WriteAt(String s, int x, int y)
        {
            IntVector2 originalPos = new IntVector2(Console.CursorLeft, Console.CursorTop);
            if (
                x > CursorMinX &&
                x + s.Length < CursorMaxX &&
                y > CursorMinY
                )
            {
                
                Console.SetCursorPosition(x, y);
                Console.Write(s);
                Console.SetCursorPosition(originalPos.X, originalPos.Y);
                return true;
            }
            else return false;
        }

        public static void PrintArrayToConsole(this string[] content, IntVector2 origin, JustifyX horizontalJustify, JustifyY verticalJustify, bool[]? hiLighted = null)
        {
            int maxX = CursorMaxX;
            if (content.Length == 0) return;
            int biggestRowSize = content.Max(x => x.Length);
            
            if ( biggestRowSize > maxX) 
            {
                bool bHiLight = (hiLighted != null && hiLighted.Any(x => x == true));
                List<string> preContent = new List<string>();
                List<bool> preContentHighLighted = new List<bool>();
                // TODO: need special logic
                for (int i = 0; i < content.Length; i ++)
                {
                    if (content[i].Length > maxX)
                    {
                        StringBuilder largeRow = new StringBuilder(content[i]);
                        int slices = (int)Math.Ceiling((double)content[i].Length / maxX);
                        int sliceSize = maxX - 1;
                        for (int j = 0; j < slices - 1; j++)   // all but the last slice
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append(largeRow.ToString(0, sliceSize));
                            preContent.Add(sb.ToString());
                            if (bHiLight && getExistsAndIsTrue(hiLighted, i)) preContentHighLighted.Add(true);
                            else preContentHighLighted.Add(false);
                            largeRow.Remove(0, sliceSize);
                        }
                        //                                     // last slice here
                        preContent.Add(largeRow.ToString());
                        if (bHiLight && getExistsAndIsTrue(hiLighted, i)) preContentHighLighted.Add(true);
                        else preContentHighLighted.Add(false);

                    }
                    else
                    {
                        preContent.Add(content[i]);
                        if (bHiLight && getExistsAndIsTrue(hiLighted, i)) preContentHighLighted.Add(true);
                        else preContentHighLighted.Add(false);
                    }
                    //preContent.Add(content[i]);
                }
                content = preContent.ToArray();
                hiLighted = preContentHighLighted.ToArray();
            }

            int rowQuantity = content.Length;
            int startY = getStartYPos(rowQuantity, verticalJustify);
            //int startX = getStartXPos(content.Max(x => x.Length), horizontalJustify);
            for (int i = 0; i < rowQuantity; i ++)
            {
                int startX = getStartXPos(content[i].Length, horizontalJustify);
                WriteAtDelta(content[i], startX, startY + i, getExistsAndIsTrue(hiLighted, i));
            }

            int getStartYPos(in int rowQuantity, in JustifyY justifyY )
            {
                switch ( justifyY )
                {
                    case JustifyY.Top: return 0;
                    case JustifyY.Center: return (int)Math.Floor((double)CursorMaxY / 2d);
                    case JustifyY.Bottom: return CursorMaxY - rowQuantity;
                }
                return 0;
            }

            int getStartXPos(in int rowSize, in JustifyX justifyX)
            {
                switch(justifyX)
                {
                    case JustifyX.Left: return 0;
                    case JustifyX.Center:
                        int centerScreen = (int)Math.Floor(CursorMaxX / 2d);
                        int rowHalfSize = (int)Math.Ceiling(rowSize / 2d);
                        int rowStartPos = centerScreen - rowHalfSize;
                        return rowStartPos;
                    case JustifyX.Right: return CursorMaxX - rowSize;
                }
                return 0;
            }
        }

        public static bool getExistsAndIsTrue(this bool[] bools, int index)
        {
            bool result = false;
            if (index >= 0 && bools != null && bools.Length > index)
            {
                if (bools.ElementAt(index) == true) result = true;
            }
            return result;
        }

        public static int calculateMaxRowLength(in string[] rows)
        {
            int maxRowLength = 0;
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i].Length > maxRowLength) maxRowLength = rows[i].Length;
            }
            return maxRowLength;
        }

        public static int GetMinimumWindowWidthForWrite(int contentLength, JustifyX justify)
        {
            if (justify == JustifyX.Center)
            {
                int halfSize = (int)Math.Ceiling((double)contentLength / 2d);
                return halfSize * 2 + 2;
            }
            else return contentLength;
        }

        public static int GetCursorStartDeltaPos(int contentLength, JustifyX justify)
        {
            switch (justify)
            {
                case JustifyX.Left:
                    return 0;
                case JustifyX.Center:
                    int halfSize = (int)Math.Ceiling((double)contentLength / 2d);
                    int screenCenter = (int)Math.Round((double)CursorMaxX / 2d, 0);
                    return screenCenter - halfSize;
                case JustifyX.Right:
                    return CursorMaxX - contentLength + 1;
                default: return 0;
            }
        }

        public static void ReverseConsoleColours()
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public static void ResetConsoleColours()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static string[] CreateButtonStringArray(in string[] buttonsText, in int maxRowWidthIn, int minbuttonPaddingIn = 5, int buttonSpacingIn = 1)
        {
            List<string> buttons = new List<string>();
            if (minbuttonPaddingIn < 2) minbuttonPaddingIn = 2;
            if (buttonSpacingIn < 0) buttonSpacingIn = 0;
            int largestButtonTextWidth = buttonsText.Max(x => x.Length);
            int buttonWidth = largestButtonTextWidth + minbuttonPaddingIn * 2;
            for (int i = 0; i < buttonsText.Length; i++)
            {
                int padding = (int)Math.Ceiling ((buttonWidth - buttonsText[i].Length) / 2d);
                if (padding < minbuttonPaddingIn) padding = minbuttonPaddingIn;

                StringBuilder sb = new StringBuilder();
                sb.Append('[');
                sb.Append(' ', padding - 1);
                sb.Append(buttonsText[i]);
                if (buttonsText[i].Length % 2 != 0) sb.Append(' ');
                sb.Append(' ', padding - 1);
                sb.Append(']');

                if (sb.Length> maxRowWidthIn)
                {
                    string[] splitButton = SplitStringByMaxLength(sb.ToString(), maxRowWidthIn);
                    for (int j = 0; j < splitButton.Length; j++) buttons.Add(splitButton[j]);
                }
                else buttons.Add(sb.ToString());
            }
            return buttons.ToArray();
        }

        public static string[] CreateButtonStringArray(in string[] buttonsText,ref bool[] hilighted, in int maxRowWidthIn, int minbuttonPaddingIn = 5, int buttonSpacingIn = 1)
        {
            //Dictionary<string, bool> buttons = new Dictionary<string, bool>();
            List<string> buttons = new List<string>();
            List<bool> prehiLighted = new List<bool>();

            if (minbuttonPaddingIn < 2) minbuttonPaddingIn = 2;
            if (buttonSpacingIn < 0) buttonSpacingIn = 0;
            int largestButtonTextWidth = buttonsText.Max(x => x.Length);
            int buttonWidth = largestButtonTextWidth + minbuttonPaddingIn * 2;
            for (int i = 0; i < buttonsText.Length; i++)
            {
                int padding = (int)Math.Ceiling((buttonWidth - buttonsText[i].Length) / 2d);
                if (padding < minbuttonPaddingIn) padding = minbuttonPaddingIn;

                StringBuilder sb = new StringBuilder();
                sb.Append('[');
                sb.Append(' ', padding - 1);
                sb.Append(buttonsText[i]);
                if (buttonsText[i].Length % 2 != 0) sb.Append(' ');
                sb.Append(' ', padding - 1);
                sb.Append(']');

                if (sb.Length > maxRowWidthIn)
                {
                    string[] splitButton = SplitStringByMaxLength(sb.ToString(), maxRowWidthIn);
                    //for (int j = 0; j < splitButton.Length; j++) buttons.Add(splitButton[j], getExistsAndIsTrue(hilighted, i));
                    for (int j = 0; j < splitButton.Length; j++)
                    {
                        buttons.Add(splitButton[j]);
                        prehiLighted.Add(getExistsAndIsTrue(hilighted, i));
                    }
                }
                else/* buttons.Add(sb.ToString(), getExistsAndIsTrue(hilighted, i));*/
                {
                    buttons.Add(sb.ToString());
                    prehiLighted.Add(getExistsAndIsTrue(hilighted, i));
                }
            }
            //return buttons;
            hilighted = prehiLighted.ToArray();
            return buttons.ToArray();
        }

        public static string[] CreateHorizontalButtonStringRowsArray(in string[] buttonsText,in bool[] hilighted, in int maxRowWidthIn, out Dictionary<IntVector2,int> hilightedOut, int minbuttonPaddingIn = 5, int buttonSpacingIn = 1)
        {
            List<string> buttonRows = new List<string>();
            hilightedOut = new Dictionary<IntVector2, int>();  // each hilight Start index and end index stored inside the vector2, the int is the RowIndex 

            if (minbuttonPaddingIn < 2) minbuttonPaddingIn = 2;
            if (buttonSpacingIn < 0) buttonSpacingIn = 0;
            int largestButtonTextWidth = buttonsText.Max(x => x.Length);
            int buttonWidth = largestButtonTextWidth + minbuttonPaddingIn * 2;
            int currentRowWidthUsed = 0;
            int rowCount = 1;
            StringBuilder sbRow = new StringBuilder();
            for (int i = 0; i < buttonsText.Length; i++)
            {
                int padding = (int)Math.Ceiling((buttonWidth - buttonsText[i].Length) / 2d);
                if (padding < minbuttonPaddingIn) padding = minbuttonPaddingIn;

                StringBuilder sb = new StringBuilder();
                sb.Append('[');
                sb.Append(' ', padding - 1);
                sb.Append(buttonsText[i]);
                sb.Append(' ', padding - 1);
                sb.Append(']');

                sb.Append(' ', buttonSpacingIn);

                bool bhiLight = getExistsAndIsTrue(hilighted, i);

                if (currentRowWidthUsed + sb.Length > maxRowWidthIn)
                {
                    buttonRows.Add(sbRow.ToString());
                    sbRow.Clear();
                    rowCount++;
                    currentRowWidthUsed = 0;

                    string[] splitButton = SplitStringByMaxLength(sb.ToString(), maxRowWidthIn);
                    for (int j = 0; j < splitButton.Length - 1; j++)
                    {
                        if (bhiLight) hilightedOut.Add(new IntVector2(0, maxRowWidthIn), rowCount);
                        buttonRows.Add(splitButton[j]);
                        rowCount++;
                    }
                    sbRow.Append(splitButton[splitButton.Length - 1]);
                    currentRowWidthUsed = splitButton[splitButton.Length - 1].Length;
                    if (bhiLight && currentRowWidthUsed > buttonSpacingIn) hilightedOut.Add(new IntVector2(0, (currentRowWidthUsed - buttonSpacingIn > 0)? currentRowWidthUsed - buttonSpacingIn: currentRowWidthUsed), rowCount);
                }
                else
                {
                    if (bhiLight) hilightedOut.Add(new IntVector2(currentRowWidthUsed, (currentRowWidthUsed + sb.Length - buttonSpacingIn > 0) ? currentRowWidthUsed + sb.Length - buttonSpacingIn : currentRowWidthUsed + sb.Length), rowCount);
                    sbRow.Append(sb.ToString());
                    currentRowWidthUsed += sb.Length;
                    
                    //buttonRows.Add(buttonsText[i], getExistsAndIsTrue(hilighted, i));
                } 
            }
            buttonRows.Add(sbRow.ToString());
            return buttonRows.ToArray();
        }

        public static string[] SplitStringByMaxLength(in string textIn,in int maxRowWidth)
        {
            int textLength = textIn.Length;
            if (textLength <= maxRowWidth) return new string[] { textIn };

            int piecesAmmount = (int)Math.Ceiling((double)textLength / maxRowWidth);
            int pieceSize = (int)Math.Ceiling((double)textLength / piecesAmmount);
            List<string> preResult = new List<string>();
            StringBuilder sb = new StringBuilder(textIn);
            for (int i = 0; i < piecesAmmount - 1;i++) // not the last piece
            {
                try
                {
                    string piece = sb.ToString(0, pieceSize);
                    if (piece != null && piece.Length > 0)
                    {
                        preResult.Add(piece);
                        sb.Remove(0, pieceSize);
                    }
                }
                catch { };
            }
            preResult.Add(sb.ToString());
            return preResult.ToArray();
        }

        public static string[] CardToString(this aCard inCard)
        {
            string[] result = new string[] { "CardToString error" };
            Char suitChar = '\u2667';
            switch (inCard.GetSuit)
            {
                case aCard.Suit.Spade: suitChar = '\u2660'; break;
                case aCard.Suit.Club: suitChar = '\u2663'; break;
                case aCard.Suit.Heart: suitChar = '\u2665'; break;
                case aCard.Suit.Diamond: suitChar = '\u2666';break;
                default:throw new Exception("could not locate card's suit"); break;
            }

            //Char valueCharA = '?';
            //Char valueCharB = ' ';
            switch (inCard.GetCardType)
            {
                case aCard.Type.Face:
                    FaceCard faceCard = null;
                    if (inCard is FaceCard) faceCard = inCard as FaceCard;
                    if (faceCard == null) return result;

                    switch (faceCard.GetFace)
                    {
                        case FaceCard.Face.King: return new string[]
                            {
@"+---------+",
@"|K        |",
@$"|{suitChar} _.+._  |",
@"|  \/^\/  |",
@"|  (*@*)  |",
@$"|  '---' {suitChar}|",
@"|        K|",
@"+---------+"
                            };
                        case FaceCard.Face.Queen: return new string[]
                            {
@"+---------+",
@"|Q        |",
@$"|{suitChar}   +    |",
@"|  qoOop  |",
@"|  (===)  |",
$"|  \"\"\"\"\" {suitChar}|",
@"|        Q|",
@"+---------+"
                            };
                        case FaceCard.Face.Jack: return new string[]
                            {
@"+---------+",
@"|J        |",
@$"|{suitChar}        |",
@"|   _+_   |",
@"|  |===|  |",
@$"|  '---' {suitChar}|",
@"|        J|",
@"+---------+"
                            };

                        default: return result;

                    }
                    break;

                case aCard.Type.Number:

                    NumberCard numberCard = null;
                    if (inCard is NumberCard) numberCard = inCard as NumberCard;
                    if (numberCard == null) return result;

                    switch (numberCard.GetNumber)
                    {
                        case 10: return new string[]
                        {
@"+---------+",
@"|10       |",
@$"|{suitChar} {suitChar} {suitChar} {suitChar}  |",
@$"| {suitChar} {suitChar} {suitChar} {suitChar} |",
@$"|  {suitChar} {suitChar} {suitChar}  |",
@$"|        {suitChar}|",
@"|       10|",
@"+---------+"
                        };
                        case 9: return new string[]
                        {
@"+---------+",
@"|9        |",
@$"|{suitChar} {suitChar} {suitChar} {suitChar}  |",
@$"|  {suitChar} {suitChar} {suitChar}  |",
@$"|  {suitChar} {suitChar} {suitChar}  |",
@$"|        {suitChar}|",
@"|        9|",
@"+---------+"
                        };
                        case 8: return new string[]
                        {
@"+---------+",
@"|8        |",
@$"|{suitChar} {suitChar} {suitChar} {suitChar}  |",
@$"|   {suitChar} {suitChar}   |",
@$"|  {suitChar} {suitChar} {suitChar}  |",
@$"|        {suitChar}|",
@"|        8|",
@"+---------+"
                        };
                        case 7: return new string[]
                        {
@"+---------+",
@"|7        |",
@$"|{suitChar}  {suitChar} {suitChar}   |",
@$"|  {suitChar} {suitChar} {suitChar}  |",
@$"|   {suitChar} {suitChar}   |",
$@"|        {suitChar}|",
@"|        7|",
@"+---------+"
                        };
                        case 6: return new string[]
                            {
@"+---------+",
@"|6        |",
@$"|{suitChar}  {suitChar} {suitChar}   |",
@$"|   {suitChar} {suitChar}   |",
@$"|   {suitChar} {suitChar}   |",
$@"|        {suitChar}|",
@"|        6|",
@"+---------+"
                            };
                        case 5: return new string[]
                            {
@"+---------+",
@"|5        |",
$@"|{suitChar}  {suitChar} {suitChar}   |",
$@"|    {suitChar}    |",
$@"|   {suitChar} {suitChar}   |",
$@"|        {suitChar}|",
@"|        5|",
@"+---------+"
                            };
                        case 4: return new string[]
                            {
@"+---------+",
@"|4        |",
$@"|{suitChar}  {suitChar} {suitChar}   |",
@"|         |",
$@"|   {suitChar} {suitChar}   |",
$@"|        {suitChar}|",
@"|        4|",
@"+---------+"
                            };
                        case 3: return new string[]
                            {
@"+---------+",
@"|3        |",
$@"|{suitChar}   {suitChar}    |",
$@"|    {suitChar}    |",
$@"|    {suitChar}    |",
$@"|        {suitChar}|",
@"|        3|",
@"+---------+"
                            };
                        case 2: return new string[]
                        {
@"+---------+",
@"|2        |",
$@"|{suitChar}   {suitChar}    |",
@"|         |",
$@"|    {suitChar}    |",
$@"|        {suitChar}|",
@"|        2|",
@"+---------+"
                        };
                        case 1:
                            switch(inCard.GetSuit)
                            {
                                case aCard.Suit.Spade: return new string[]
                                {
@"+---------+",
@"|A        |",
$@"|{suitChar}   ^    |",
@"|   /.\   |",
@"|  (_._)  |",
$@"|    |   {suitChar}|",
@"|        A|",
@"+---------+"

                                };
                                case aCard.Suit.Diamond: return new string[]
                                {
@"+---------+",
@"|A        |",
$@"|{suitChar}   ^    |",
@"|   / \   |",
@"|   \ /   |",
$@"|    v   {suitChar}|",
@"|        A|",
@"+---------+"
                                };
                                case aCard.Suit.Club: return new string[]
                                {
@"+---------+",
@"|A        |",
$@"|{suitChar}        |",
@"|   (`)   |",
@"|  (_'_)  |",
$@"|    |   {suitChar}|",
@"|        A|",
@"+---------+"
                                };
                                case aCard.Suit.Heart: return new string[]
                                {
@"+---------+",
@"|A        |",
$@"|{suitChar}        |",
@"|  (`v`)  |",
@"|   \ /   |",
$@"|    v   {suitChar}|",
@"|        A|",
@"+---------+"
                                };
                                default: return result;         // trace back to parameter: inCard for error source

                            }

                        default: return result;
                    }

                default: return result;
            }

            return result;
        }

        public static string[] CardBack => new string[]
                                {
@"+---------+",
@"|.........|",
@"|.........|",
@"|.........|",
@"|.........|",
@"|.........|",
@"|.........|",
@"+---------+"
                                };

        public static string[] CardSplitSpacer => new string[]
                                {
@"           ",
@"           ",
@"           ",
@"           ",
@"   SPLIT   ",
@"           ",
@"           ",
@"           "
                                };
        public static string[] CardSplitSpacerArrowLeft => new string[]
                                {
@"           ",
@"           ",
@"           ",
@"           ",
@"   SPLIT   ",
@"           ",
@"   <----   ",
@"           "
                                };
        public static string[] CardSplitSpacerArrowRight => new string[]
                                {
@"           ",
@"           ",
@"           ",
@"           ",
@"   SPLIT   ",
@"           ",
@"   ---->   ",
@"           "
                                };
        public static BJData CheckOrFixDeckLength(this BJData bjData)
        {
            if (bjData.Deck.Length <= 1)
            { bjData.Deck = BlackJackDeck(); }
            return bjData;
        }

        public static string[] AddStringBlockHorizontal(this string[] output, in string[] addition, int maxXLength = -1)
        {
            List<string> fireList = output.ToList();
            bool bAddNewLines = false;

            int outputEndLength = 0;
            if (output.Length > 0) outputEndLength = fireList.Last().Length;
            if (output.Length < addition.Length) bAddNewLines = true;
            else
            {
                int additionIndex = addition.Length - 1;
                for (int i = output.Length - 1; i > -1; i--)
                {
                    if (additionIndex < 0) break;
                    if (output[i].Length != outputEndLength) bAddNewLines = true;
                    if (maxXLength != -1 && output[i].Length + addition[additionIndex].Length > maxXLength) bAddNewLines = true;
                    additionIndex--;
                }
            }
            if (bAddNewLines)
            {
                for (int i = 0; i < addition.Length; i++)
                {
                    fireList.Add(addition[i]);
                }
                return fireList.ToArray();
            }
            else
            {
                int counter = 0;
                for (int i = output.Length - addition.Length; i < output.Length; i++)
                {
                    StringBuilder sb = new StringBuilder(output[i]);
                    sb.Append(addition[counter]);
                    output[i] = sb.ToString();
                    counter++;
                }
                return output;
            }
        }

        public static string[] StringMultiLineToStringArray(this string input) => input.Split("\n", StringSplitOptions.RemoveEmptyEntries);

        public static bool StringArraysAreSameLength(this string[] inputA, in string[] inputB)
        {
            if (inputA.Length != inputB.Length) { return false; }
            for (int i = 0; i < inputA.Length; i++) if (inputA[i].Length != inputB[i].Length) { return false; }
            return true;
        }

        public static string WinnerWinnerChickenDinner => @"
 __          __  _____   _   _   _   _   ______   _____     __          __  _____   _   _   _   _   ______   _____  
 \ \        / / |_   _| | \ | | | \ | | |  ____| |  __ \    \ \        / / |_   _| | \ | | | \ | | |  ____| |  __ \ 
  \ \  /\  / /    | |   |  \| | |  \| | | |__    | |__) |    \ \  /\  / /    | |   |  \| | |  \| | | |__    | |__) |
   \ \/  \/ /     | |   | . ` | | . ` | |  __|   |  _  /      \ \/  \/ /     | |   | . ` | | . ` | |  __|   |  _  / 
    \  /\  /     _| |_  | |\  | | |\  | | |____  | | \ \       \  /\  /     _| |_  | |\  | | |\  | | |____  | | \ \ 
     \/  \/     |_____| |_| \_| |_| \_| |______| |_|  \_\       \/  \/     |_____| |_| \_| |_| \_| |______| |_|  \_\
   _____   _    _   _____    _____   _  __  ______   _   _     _____    _____   _   _   _   _   ______   _____      
  / ____| | |  | | |_   _|  / ____| | |/ / |  ____| | \ | |   |  __ \  |_   _| | \ | | | \ | | |  ____| |  __ \     
 | |      | |__| |   | |   | |      | ' /  | |__    |  \| |   | |  | |   | |   |  \| | |  \| | | |__    | |__) |    
 | |      |  __  |   | |   | |      |  <   |  __|   | . ` |   | |  | |   | |   | . ` | | . ` | |  __|   |  _  /     
 | |____  | |  | |  _| |_  | |____  | . \  | |____  | |\  |   | |__| |  _| |_  | |\  | | |\  | | |____  | | \ \     
  \_____| |_|  |_| |_____|  \_____| |_|\_\ |______| |_| \_|   |_____/  |_____| |_| \_| |_| \_| |______| |_|  \_\                                                                                                
";

        public static string BustStringBlock => @"
  ____    _    _    _____   _______ 
 |  _ \  | |  | |  / ____| |__   __|
 | |_) | | |  | | | (___      | |   
 |  _ <  | |  | |  \___ \     | |   
 | |_) | | |__| |  ____) |    | |   
 |____/   \____/  |_____/     |_|   
";

        public static string BustStringBlock2 => @"
▄▄▄▄· ▄• ▄▌.▄▄ · ▄▄▄▄▄
▐█ ▀█▪█▪██▌▐█ ▀. •██  
▐█▀▀█▄█▌▐█▌▄▀▀▀█▄ ▐█.▪
██▄▪▐█▐█▄█▌▐█▄▪▐█ ▐█▌·
·▀▀▀▀  ▀▀▀  ▀▀▀▀  ▀▀▀ ";

        public static string DealerBustStringBlockDeprecated => @"
  _____    ______              _        ______   _____      ____    _    _    _____   _______ 
 |  __ \  |  ____|     /\     | |      |  ____| |  __ \    |  _ \  | |  | |  / ____| |__   __|
 | |  | | | |__       /  \    | |      | |__    | |__) |   | |_) | | |  | | | (___      | |   
 | |  | | |  __|     / /\ \   | |      |  __|   |  _  /    |  _ <  | |  | |  \___ \     | |   
 | |__| | | |____   / ____ \  | |____  | |____  | | \ \    | |_) | | |__| |  ____) |    | |   
 |_____/  |______| /_/    \_\ |______| |______| |_|  \_\   |____/   \____/  |_____/     |_|   
";

        public static string DealerWinStringBlockDeprecated => @"
  _____    ______              _        ______   _____      __          __  _____   _   _ 
 |  __ \  |  ____|     /\     | |      |  ____| |  __ \     \ \        / / |_   _| | \ | |
 | |  | | | |__       /  \    | |      | |__    | |__) |     \ \  /\  / /    | |   |  \| |  
 | |  | | |  __|     / /\ \   | |      |  __|   |  _  /       \ \/  \/ /     | |   | . ` |   
 | |__| | | |____   / ____ \  | |____  | |____  | | \ \        \  /\  /     _| |_  | |\  |
 |_____/  |______| /_/    \_\ |______| |______| |_|  \_\        \/  \/     |_____| |_| \_|   
";

        public static string DealerBlackJackStringBlock => @"
  _____   ______            _       ______  _____    ____   _                 _____  _  __     _           _____  _  __
 |  __ \ |  ____|    /\    | |     |  ____||  __ \  |  _ \ | |         /\    / ____|| |/ /    | |   /\    / ____|| |/ /
 | |  | || |__      /  \   | |     | |__   | |__) | | |_) || |        /  \  | |     | ' /     | |  /  \  | |     | ' / 
 | |  | ||  __|    / /\ \  | |     |  __|  |  _  /  |  _ < | |       / /\ \ | |     |  <  _   | | / /\ \ | |     |  <  
 | |__| || |____  / ____ \ | |____ | |____ | | \ \  | |_) || |____  / ____ \| |____ | . \| |__| |/ ____ \| |____ | . \ 
 |_____/ |______|/_/    \_\|______||______||_|  \_\ |____/ |______|/_/    \_\\_____||_|\_\\____//_/    \_\\_____||_|\_\
";

        // font name: elite, sizes: full
        public static string BlackJackStringBlockDeprecated => @"
▄▄▄▄· ▄▄▌   ▄▄▄·  ▄▄· ▄ •▄  ▐▄▄▄ ▄▄▄·  ▄▄· ▄ •▄ 
▐█ ▀█▪██•  ▐█ ▀█ ▐█ ▌▪█▌▄▌▪  ·██▐█ ▀█ ▐█ ▌▪█▌▄▌▪
▐█▀▀█▄██▪  ▄█▀▀█ ██ ▄▄▐▀▀▄·▪▄ ██▄█▀▀█ ██ ▄▄▐▀▀▄·
██▄▪▐█▐█▌▐▌▐█ ▪▐▌▐███▌▐█.█▌▐▌▐█▌▐█ ▪▐▌▐███▌▐█.█▌
▀▀▀▀ .▀▀▀  ▀  ▀ ·▀▀▀ ·▀  ▀ ▀▀▀• ▀  ▀ ·▀▀▀ ·▀  ▀";

        public static string DealerBlackJackStringBlock2 => @"
·▄▄▄▄  ▄▄▄ . ▄▄▄· ▄▄▌  ▄▄▄ .▄▄▄      ▄▄▄▄· ▄▄▌   ▄▄▄·  ▄▄· ▄ •▄  ▐▄▄▄ ▄▄▄·  ▄▄· ▄ •▄ 
██▪ ██ ▀▄.▀·▐█ ▀█ ██•  ▀▄.▀·▀▄ █·    ▐█ ▀█▪██•  ▐█ ▀█ ▐█ ▌▪█▌▄▌▪  ·██▐█ ▀█ ▐█ ▌▪█▌▄▌▪
▐█· ▐█▌▐▀▀▪▄▄█▀▀█ ██▪  ▐▀▀▪▄▐▀▀▄     ▐█▀▀█▄██▪  ▄█▀▀█ ██ ▄▄▐▀▀▄·▪▄ ██▄█▀▀█ ██ ▄▄▐▀▀▄·
██. ██ ▐█▄▄▌▐█ ▪▐▌▐█▌▐▌▐█▄▄▌▐█•█▌    ██▄▪▐█▐█▌▐▌▐█ ▪▐▌▐███▌▐█.█▌▐▌▐█▌▐█ ▪▐▌▐███▌▐█.█▌
▀▀▀▀▀•  ▀▀▀  ▀  ▀ .▀▀▀  ▀▀▀ .▀  ▀    ·▀▀▀▀ .▀▀▀  ▀  ▀ ·▀▀▀ ·▀  ▀ ▀▀▀• ▀  ▀ ·▀▀▀ ·▀  ▀";

        public static string DeadHeatStringBlock => @"
·▄▄▄▄  ▄▄▄ . ▄▄▄· ·▄▄▄▄       ▄ .▄▄▄▄ . ▄▄▄· ▄▄▄▄▄
██▪ ██ ▀▄.▀·▐█ ▀█ ██▪ ██     ██▪▐█▀▄.▀·▐█ ▀█ •██  
▐█· ▐█▌▐▀▀▪▄▄█▀▀█ ▐█· ▐█▌    ██▀▐█▐▀▀▪▄▄█▀▀█  ▐█.▪
██. ██ ▐█▄▄▌▐█ ▪▐▌██. ██     ██▌▐▀▐█▄▄▌▐█ ▪▐▌ ▐█▌·
▀▀▀▀▀•  ▀▀▀  ▀  ▀ ▀▀▀▀▀•     ▀▀▀ · ▀▀▀  ▀  ▀  ▀▀▀ ";

        public static string HandLostStringBlock => @"
 ▄ .▄ ▄▄▄·  ▐ ▄ ·▄▄▄▄      ▄▄▌        .▄▄ · ▄▄▄▄▄
██▪▐█▐█ ▀█ •█▌▐███▪ ██     ██•  ▪     ▐█ ▀. •██  
██▀▐█▄█▀▀█ ▐█▐▐▌▐█· ▐█▌    ██▪   ▄█▀▄ ▄▀▀▀█▄ ▐█.▪
██▌▐▀▐█ ▪▐▌██▐█▌██. ██     ▐█▌▐▌▐█▌.▐▌▐█▄▪▐█ ▐█▌·
▀▀▀ · ▀  ▀ ▀▀ █▪▀▀▀▀▀•     .▀▀▀  ▀█▄▀▪ ▀▀▀▀  ▀▀▀ ";

        // Font Name: Shaded Blocky Height: smush(U), Length: Full
        public static string WinningHandStringBlock => @"
 ░  ░░░░  ░░        ░░   ░░░  ░░   ░░░  ░░        ░░       ░░
 ▒  ▒  ▒  ▒▒▒▒▒  ▒▒▒▒▒    ▒▒  ▒▒    ▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒
 ▓        ▓▓▓▓▓  ▓▓▓▓▓  ▓  ▓  ▓▓  ▓  ▓  ▓▓      ▓▓▓▓       ▓▓
 █   ██   █████  █████  ██    ██  ██    ██  ████████  ███  ██
█  ████  ██        ██  ███   ██  ███   ██        ██  ████  █";

        public static string BlackJackStringBlock => @"
 ░       ░░░  ░░░░░░░░░      ░░░░      ░░░  ░░░░  ░░░░░░░░        ░░░      ░░░░      ░░░  ░░░░  ░
 ▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒  ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒  ▒▒
 ▓       ▓▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓▓▓▓▓     ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓  ▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓▓▓▓▓     ▓▓▓▓
 █  ████  ██  ████████        ██  ████  ██  ███  █████████  ████  ██        ██  ████  ██  ███  ██
█       ███        ██  ████  ███      ███  ████  █████████      ███  ████  ███      ███  ████  █";

        public static string DealerBustStringBlock => @"
 ░       ░░░        ░░░      ░░░  ░░░░░░░░        ░░       ░░░░░░░░░       ░░░  ░░░░  ░░░      ░░░        ░
 ▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒
 ▓  ▓▓▓▓  ▓▓      ▓▓▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓▓▓▓▓      ▓▓▓▓       ▓▓▓▓▓▓▓▓▓       ▓▓▓  ▓▓▓▓  ▓▓▓      ▓▓▓▓▓▓  ▓▓▓▓
 █  ████  ██  ████████        ██  ████████  ████████  ███  █████████  ████  ██  ████  ████████  █████  ████
█       ███        ██  ████  ██        ██        ██  ████  ████████       ████      ████      ██████  ████";

        // Font Name: Varsity Height: smush(U), Length: smush(U)
        public static string WinningHandStringBlockDeprecated => @"
 ____      ____ _____ ____  _____ ____  _____ _____ ____  _____  ______    ____  ____      _      ____  _____ ______    
|_  _|    |_  _|_   _|_   \|_   _|_   \|_   _|_   _|_   \|_   _.' ___  |  |_   ||   _|    / \    |_   \|_   _|_   _ `.  
  \ \  /\  / /   | |   |   \ | |   |   \ | |   | |   |   \ | |/ .'   \_|    | |__| |     / _ \     |   \ | |   | | `. \ 
   \ \/  \/ /    | |   | |\ \| |   | |\ \| |   | |   | |\ \| || |   ____    |  __  |    / ___ \    | |\ \| |   | |  | | 
    \  /\  /    _| |_ _| |_\   |_ _| |_\   |_ _| |_ _| |_\   |\ `.___]  |  _| |  | |_ _/ /   \ \_ _| |_\   |_ _| |_.' / 
     \/  \/    |_____|_____|\____|_____|\____|_____|_____|\____`._____.'  |____||____|____| |____|_____|\____|______.'  ";
        public static string WinningsStringBlock => @"
           _             _                 
          (_)           (_)                
 __      ___ _ __  _ __  _ _ __   __ _ ___ 
 \ \ /\ / / | '_ \| '_ \| | '_ \ / _` / __|
  \ V  V /| | | | | | | | | | | | (_| \__ \
   \_/\_/ |_|_| |_|_| |_|_|_| |_|\__, |___/
                                  __/ |    
                                 |___/
";

        public static string CashSignStringBlock=> @"
    _    
 __|_|___
(  _____/
| (|_|__ 
(_____  )
/\_|_|) |
\_______)
   |_|   
";

        public static string CashSignStringBlock2(Decimal money) =>
$"    _    \n"+
$" __|_|___\n"+
$"(  _____/\n"+
$"| (|_|__ \n"+
$"(_____  )    {money.ToString()}\n"+
$"/\\_|_|) |\n" +
$"\\_______)\n" +
$"   |_|   \n";

        public static string CashSignStringBlock3(Decimal money) =>
$"  ┏┻┓\n" +
$"  ┗━┓ {money.ToString()}\n" +
$"  ┗┳┛\n";

        public static string CashSignStringBlock2Negative(Decimal money) =>
$"          _    \n" +
$"       __|_|___\n" +
$"      (  _____/\n" +
$"      | (|_|__ \n" +
$" _____(_____  )    {money.ToString()}\n" +
$"(____\\_|_|) |\n" +
$"    \\_______)\n" +
$"         |_|   \n";

        // Font Name: tmplr, Character Height: Smush(U)
        public static string CashSignStringBlock3Negative(Decimal money) =>
$"  ┏┻┓\n" +
$"━━┗━┓ {money.ToString()}\n" +
$"  ┗┳┛\n";

        public static T[] Shuffle<T>(this T[] array) => array.OrderBy(x=> Random.Shared.Next()).ToArray();

        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            T[] dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        public static T[] Pop<T>(this T[] array, out T element)
        {
            if (array == null || array.Length == 0) { throw new Exception("array too small"); }
            element = array[0]; 
            if (array.Length == 1)
                { return new T[0]; }
            int pendingArrayLength = array.Length - 1;
            T[] bufferArray = new T[pendingArrayLength];
            Array.Copy(array, 1, bufferArray, 0, pendingArrayLength);
            array = bufferArray;
            return array;
        }

        //public static string[] AddCardsToStringArray(this in string[] stringArrayIn, in aCard[] cardsIn, int maxLengthX = -1)
        //{

        //}
    }
}
