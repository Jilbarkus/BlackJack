using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames
{
    public class ContentPage
    {
        public class TopContentPage
        {
            // Public set variables
            private string[] topContent = new string[0];
            private JustifyX topContentJustify = JustifyX.Left;
            private int topIndent = 0;

            // Actions
            private event Action contentChanged;

            // Class calculated variables
            private int maxRowLength = 0;
            private int cursorStartPos = 0;
            private int minWindowWidth = 0;

            public int MaxRowLength => maxRowLength;
            public int MinWindowWidth => minWindowWidth;
            public void SetTopContent(string[] topContentIn)
            {
                topContent = topContentIn;
                contentChanged?.Invoke();
            }

            public void SetTopContentJustify(JustifyX topContentJustifyIn)
            {
                topContentJustify = topContentJustifyIn;
                contentChanged?.Invoke();
            }

            public void SetTopIndent(int topIndentIn)
            {
                topIndent = topIndentIn;
                contentChanged?.Invoke();
            }

            public TopContentPage(string[] topContentIn, JustifyX topContentjustify = JustifyX.Left, int topIndentIn = 0)
            {
                topContent = topContentIn;
                topContentJustify = topContentjustify;
                topIndent = topIndentIn;
                contentChanged += RecalculatePositions;
                contentChanged?.Invoke();
            }

            private void RecalculatePositions()
            {
                maxRowLength = GlobalFunctions.calculateMaxRowLength(topContent);
                cursorStartPos = GlobalFunctions.GetCursorStartDeltaPos(maxRowLength, topContentJustify);
                minWindowWidth = GlobalFunctions.GetMinimumWindowWidthForWrite(maxRowLength, topContentJustify);
            }

            public void WriteContent()
            {
                if (GlobalFunctions.CursorMaxX < minWindowWidth) Console.WindowWidth = minWindowWidth;
                cursorStartPos = GlobalFunctions.GetCursorStartDeltaPos(maxRowLength, topContentJustify);
                for (int i = 0; i < topContent.Length; i++)
                {
                    GlobalFunctions.WriteAtDelta(topContent[i], cursorStartPos, topIndent + i);
                }
                
            }

            
        }

        public class BottomHorizontalButtonPage
        {
            // Public set variables
            private string[] bottomButtons = new string[0];
            private JustifyX bottomContentJustify = JustifyX.Left;
            private int bottomIndent = 0;
            private int buttonPadding = 0;
            private int buttonSpacing = 1;

            // Actions
            private event Action contentChanged;

            // Class calculated variables
            private int maxButtonWdith = 0;
            private int cursorStartPos = 0;
            private int minWindowWidth = 0;
            private int totalRowWidth = 0;

            public int ButtonPadding => buttonPadding;
            public int MaxRowLength => maxButtonWdith;
            public int MinWindowWidth => minWindowWidth;
            public int ButtonSpacing => buttonSpacing;
            public int TotalRowWidth => totalRowWidth;
            public void SetBottomButtons(string[] bottomButtonsIn)
            {
                bottomButtons = bottomButtonsIn;
                contentChanged?.Invoke();
            }

            public void SetBottomContentJustify(JustifyX bottomContentJustifyIn)
            {
                bottomContentJustify = bottomContentJustifyIn;
                contentChanged?.Invoke();
            }

            public void SetBottomIndent(int bottomIndentIn)
            {
                bottomIndent = bottomIndentIn;
                contentChanged?.Invoke();
            }

            public void SetButtonPadding(int buttonPaddingIn)
            {
                buttonPadding = buttonPaddingIn;
                contentChanged?.Invoke();
            }

            public void SetButtonSpacing(int buttonSpacingIn)
            {
                buttonSpacing = buttonSpacingIn;
                contentChanged?.Invoke();
            }

            public BottomHorizontalButtonPage(string[] BottomButtonsIn, JustifyX BottomContentjustify = JustifyX.Left, int bottomIndentIn = 0, int buttonPaddingIn = 5, int buttonSpacingIn = 1)
            {
                bottomButtons = BottomButtonsIn;
                bottomContentJustify = BottomContentjustify;
                bottomIndent = bottomIndentIn;
                buttonPadding = buttonPaddingIn;
                buttonSpacing = buttonSpacingIn;
                contentChanged += RecalculatePositions;
                contentChanged?.Invoke();
            }

            private void RecalculatePositions()
            {
                maxButtonWdith = GlobalFunctions.calculateMaxRowLength(bottomButtons) + (buttonPadding * 2);
                int stackWidth = 0;
                for (int i = 0; i < bottomButtons.Length; i++)
                {
                    stackWidth += buttonPadding * 2 + bottomButtons[i].Length + buttonSpacing;
                }
                totalRowWidth = stackWidth;
                cursorStartPos = recalculateCursorStartPos();
                minWindowWidth = GlobalFunctions.GetMinimumWindowWidthForWrite(totalRowWidth, bottomContentJustify);
            }

            private int recalculateCursorStartPos()
            {
                return GlobalFunctions.GetCursorStartDeltaPos(totalRowWidth, bottomContentJustify);
            }
            public void WriteContent(int selectedIndex = -1)
            {
                if (GlobalFunctions.CursorMaxX < minWindowWidth && minWindowWidth < Console.LargestWindowWidth) Console.WindowWidth = minWindowWidth;
                cursorStartPos = recalculateCursorStartPos();
                int lastButtonEndX = cursorStartPos;
                for (int i = 0; i < bottomButtons.Length; i++)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append('[');
                    sb.Append(' ', buttonPadding - 1);
                    sb.Append(bottomButtons[i]);
                    sb.Append(' ', buttonPadding - 1);
                    sb.Append(']');
                    IntVector2 finalCursorDelta = GlobalFunctions.WriteAtDelta(sb.ToString(), lastButtonEndX + buttonSpacing, GlobalFunctions.CursorMaxY - bottomIndent, i == selectedIndex);
                    lastButtonEndX = finalCursorDelta.X;
                }

            }


        }



        //internal class TopContentPageType : TopContentPage
        //{
        //    public TopContentPageType(string[] topContentIn, Justify topContentjustify = Justify.Left) : base(topContentIn, topContentjustify)
        //    {
        //    }
        //}
    }
}
