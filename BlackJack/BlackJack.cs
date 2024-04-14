using System.Text;

namespace CardGames
{
    internal class BlackJack: IGame
    {
        private GameState _state = GameState.Betting;
        private BJData? _bjData = new BJData(8);
        private BJSettings _bjSettings = new BJSettings();
        
        public enum GameState { Betting, Dealing}
        //public enum HitActions {Normal, Split, DoubleDown, }
        public bool Run(ref PlayerSave player)
        {
            // in case there is no game start one
            if (_bjData == null) { resetBJGame(); changeState(GameState.Betting); return true; }
            string[] currentOutput = new string[0];
            switch (_state)
            {
                case GameState.Betting:
                    currentOutput = new string[] {
                        $"Please place your bets for the next hand, the table minimum is ${_bjSettings.minimumBet}",
                        $"Current Bet: ${_bjData?.bettingAmmount:0}           Funds: {player.Cash}"
                    };
                    bool[] hiLighted = new bool[] { false, false, false, false };
                    string[] buttons = GlobalFunctions.CreateButtonStringArray(new string[] { $"\u2191 Add Bet +{_bjSettings.betIncrement}", "\u2192 Start Dealing", "\u2193 Reduce Bet   ", $"x Exit to Menu " }, ref hiLighted, GlobalFunctions.CursorMaxX);
                    for (int i = 0; i < buttons.Length; i++) currentOutput = currentOutput.Append(buttons[i]).ToArray();
                    Console.Clear();
                    currentOutput.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Center);

                    switch (getBettingInput())
                    {
                        case 0: return false;
                        case 1:
                            if (player.Cash >= (_bjSettings.betIncrement + _bjData.bettingAmmount)) _bjData.bettingAmmount += _bjSettings.betIncrement;
                            break;
                        case 2:
                            if (_bjData.bettingAmmount >= _bjSettings.minimumBet)
                            {
                                //resetBJGame();
                                player.AddCash(-_bjData.bettingAmmount);
                                Console.Clear();
                                changeState(GameState.Dealing);
                            }
                            break;
                        case 3:
                            if (_bjData.bettingAmmount - _bjSettings.betIncrement >= 0) _bjData.bettingAmmount -= _bjSettings.betIncrement;
                            break;
                        default: break;
                    }
                    return true;

                // entry into either a new game or an ongoing one
                case GameState.Dealing:
                    // does the player get to input this round?
                    bool bTakeInput = true;
                    // how much player has bet
                    Decimal Bet = _bjData.bettingAmmount;
                    // Case: Round 1
                    if (_bjData.dealerHand.Length == 0)
                    {
                        _bjData.dealerHand = _bjData.dealerHand.Append(Draw()).ToArray();
                        _bjData.playerHandA = _bjData.playerHandA.Append(Draw()).ToArray();
                        bTakeInput = false;
                    }
                    // Case: Round 2
                    else if (_bjData.dealerHand.Length == 1 && _bjData.playerHandA.Length == 1 && _bjData.playerHandB.Length == 0)
                    {
                        _bjData.playerHandA = _bjData.playerHandA.Append(Draw()).ToArray();
                        bTakeInput = false;

                    }
                    // Case: Final Round
                    else if (_bjData.dealerHand.Length > 1)
                    {
                        bTakeInput = false;
                    }
                    // calculate player values
                    int dealerHand = GetBlackJackValue(_bjData.dealerHand);
                    int playerHandA = GetBlackJackValue(_bjData.playerHandA);
                    int playerHandB = GetBlackJackValue(_bjData.playerHandB);
                    int bestPlayerValue = GetBestBlackJackValue(playerHandA, playerHandB);

                    // Case: Player Bust
                    if (_bjData.dealerHand.Length == 1 && bestPlayerValue >= 21)
                    {
                        _bjData.dealerHand = _bjData.dealerHand.Append(Draw()).ToArray();
                        return true;
                    }

                    //////////// display cards ////////////
                    ///
                    currentOutput = new string[0];
                    // dealer value 
                    //currentOutput = currentOutput.Append($"Dealer's hand: {dealerHand}").ToArray();
                    // dealer cards 
                    for (int i = 0; i < _bjData.dealerHand.Length; i++)
                    {
                        currentOutput = currentOutput.AddStringBlockHorizontal(_bjData.dealerHand[i].CardToString(), GlobalFunctions.CursorMaxX);
                    }
                    // dealer's face down card
                    if (_bjData.dealerHand.Length == 1)
                    {
                        currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.CardBack, GlobalFunctions.CursorMaxX);
                    }
                    // add a spacer so that player's cards are'nt grouped together with dealer's cards
                    //currentOutput = currentOutput.AddStringBlockHorizontal(new string[] { "         " }, GlobalFunctions.CursorMaxX);
                    currentOutput = currentOutput.Append("      ").ToArray();
                    // player cards A
                    for (int i = 0; i < _bjData.playerHandA.Length; i++)
                    {
                        currentOutput = currentOutput.AddStringBlockHorizontal(_bjData.playerHandA[i].CardToString(), GlobalFunctions.CursorMaxX);
                    }
                    //spacer between player's two split hands
                    if (_bjData.playerHandB.Length > 0) currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.CardSplitSpacer, GlobalFunctions.CursorMaxX);
                    // player cards B
                    for (int i = 0; i < _bjData.playerHandB.Length; i++)
                    {
                        currentOutput = currentOutput.AddStringBlockHorizontal(_bjData.playerHandB[i].CardToString(), GlobalFunctions.CursorMaxX);
                    }
                    //////////// endgame ////////////
                    ///
                    // Case: Last round
                    if (_bjData.dealerHand.Length > 1)
                    {
                        bool bEndGame = dealerHand > bestPlayerValue || dealerHand >= 16;
                        decimal playerCashBuffer = player.Cash;
                        if (bEndGame)
                        {
                            // add a spacer so that winning string block isn't grouped together with dealer's cards
                            currentOutput = currentOutput.Append("      ").ToArray();
                            //dead heat
                            if ((dealerHand > 21 && bestPlayerValue > 21) || dealerHand == bestPlayerValue)
                            {
                                currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.DeadHeatStringBlock.StringMultiLineToStringArray());
                                player.AddCash(_bjData.bettingAmmount);
                            }
                            // blackjack
                            else if (bestPlayerValue == 21)
                            {
                                currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.BlackJackStringBlock.StringMultiLineToStringArray());
                                // blackjack without hitting
                                if (_bjData.playerHandA.Length + _bjData.playerHandB.Length == 2)
                                {
                                    
                                    player.AddCash(1.5m * _bjData.bettingAmmount);
                                }
                                else
                                {
                                    player.AddCash(2m * _bjData.bettingAmmount);
                                }
                            }
                            // dealer bust
                            else if (dealerHand > 21)
                            {
                                currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.DealerBustStringBlock.StringMultiLineToStringArray());
                                player.AddCash(2m * _bjData.bettingAmmount);
                            }
                            // player bust
                            else if (bestPlayerValue > 21)
                            {
                                currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.BustStringBlock2.StringMultiLineToStringArray());
                            }
                            // player winning hand - needs change
                            else if (bestPlayerValue > dealerHand)
                            {
                                currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.WinningHandStringBlock.StringMultiLineToStringArray());
                                player.AddCash(2m * _bjData.bettingAmmount);
                            }
                            // dealer winning hand
                            else
                            {
                                currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.HandLostStringBlock.StringMultiLineToStringArray());
                            }

                        }

                        if (bEndGame)
                        {
                            buttons = EndGameButtonStringBlock();
                            currentOutput = currentOutput.AddStringBlockHorizontal(buttons, GlobalFunctions.CursorMaxX);
                            Console.Clear();
                            currentOutput.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Bottom);
                            new string[3] { $"Bank: ${player.Cash}",$"Bet:{_bjData.bettingAmmount}",$"Winnings: ${player.Cash - playerCashBuffer - _bjData.bettingAmmount}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Right, JustifyY.Top);
                            new string[2] { $"Dealer's hand: {dealerHand}", $"{player.Name}'s hand: {bestPlayerValue}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Top);
                            switch (GlobalFunctions.GetKeyPress(
                        new ConsoleKeyInfo[] {
                            new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false),
                            new ConsoleKeyInfo('1', ConsoleKey.LeftArrow, false, false, false),
                            new ConsoleKeyInfo('2', ConsoleKey.RightArrow, false, false, false)}))
                            {
                                // quit
                                case 0:
                                    resetBJGame();
                                    Console.Clear();
                                    changeState(GameState.Betting);
                                    return false;

                                // adjust bet
                                case 1:
                                    resetBJGame();
                                    Console.Clear();
                                    changeState(GameState.Betting);
                                    return true;

                                // same again
                                default:
                                    decimal prevBet = _bjData.bettingAmmount;
                                    resetBJGame();
                                    player.AddCash(-prevBet);
                                    _bjData.bettingAmmount = prevBet;
                                    changeState(GameState.Dealing);
                                    Console.Clear();
                                    //break;
                                    return true;
                                    // add more console clears and change AddStringBlockHorizontal so that it doesnt group both hands together
                            }
                        }
                    }

                    if (!bTakeInput) currentOutput.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Top);


                    new string[2] { $"Bank: ${player.Cash}", $"Bet: ${_bjData.bettingAmmount}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Right, JustifyY.Top);
                    if (_bjData.playerHandB.Length == 0) new string[2] { $"Dealer's hand: {dealerHand}", $"{player.Name}'s hand: {bestPlayerValue}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Top);
                    else new string[3] { $"Dealer's hand: {dealerHand}", $"{player.Name}'s best split: {bestPlayerValue}", $"Left Split: {playerHandA}, Right Split: {playerHandB}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Top);
                    
                    //////////// input   ////////////
                    ///
                    if (bTakeInput)
                    {
                        currentOutput.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Top);
                        currentOutput = new string[0];
                        
                        // Case: player can INITIATE a split or double double down
                        if (_bjData.playerHandA.Length == 2 &&
                            _bjData.playerHandB.Length == 0 &&
                            player.Cash >= _bjData.bettingAmmount)
                        {
                            bool bSplittable = CardsAreSplittable(_bjData.playerHandA[0], _bjData.playerHandA[1]);
                            bool bDoubleDownable = (playerHandA >= 9 && playerHandA <= 11);

                            string[] buttonsText = new string[2] { $"\u2191 Hit Me", "\u2192 Stay" };
                            int buttonQuantity = 3;
                            if (bSplittable)
                            {
                                buttonsText = buttonsText.Append($"\u2190 Split ").ToArray();
                                buttonQuantity++;
                            }
                            if (bDoubleDownable)
                            {
                                buttonsText = buttonsText.Append($"\u2193 Double").ToArray();
                                buttonQuantity++;
                            }
                            buttonsText = buttonsText.Append("x Exit to Menu ").ToArray();

                            hiLighted = new bool[buttonQuantity];
                            buttons = GlobalFunctions.CreateButtonStringArray(buttonsText, ref hiLighted, GlobalFunctions.CursorMaxX);
                            buttons.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Bottom);




                            switch (GlobalFunctions.GetKeyPress(
                        new ConsoleKeyInfo[] {
                            new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false),
                            new ConsoleKeyInfo('1', ConsoleKey.UpArrow, false, false, false),
                            new ConsoleKeyInfo('2', ConsoleKey.RightArrow, false, false, false),
                            new ConsoleKeyInfo('3', ConsoleKey.LeftArrow, false, false, false),
                            new ConsoleKeyInfo('4', ConsoleKey.DownArrow, false, false, false)
                        }))
                            {
                                // quit
                                case 0:
                                    return false;
                                // hit me
                                case 1:
                                    _bjData.playerHandA = _bjData.playerHandA.Append(Draw()).ToArray();
                                    return true;
                                // stay
                                case 2:
                                    _bjData.dealerHand = _bjData.dealerHand.Append(Draw()).ToArray();
                                    return true;
                                case 3:
                                    if (!bSplittable) return false;
                                    player.AddCash(-_bjData.bettingAmmount);
                                    _bjData.bettingAmmount *= 2;
                                    _bjData.playerHandB = new aCard[1] { _bjData.playerHandA[1] };
                                    _bjData.playerHandA = new aCard[1] { _bjData.playerHandA[0] };
                                    return true;
                                case 4: 
                                    if (!bDoubleDownable) return false;
                                    player.AddCash(-_bjData.bettingAmmount);
                                    _bjData.bettingAmmount *= 2;
                                    _bjData.playerHandA = _bjData.playerHandA.Append(Draw()).ToArray();
                                    _bjData.dealerHand = _bjData.dealerHand.Append(Draw()).ToArray();
                                    return true;

                            }

                        }
                        // case player is in the midst of a split
                        else if (_bjData.playerHandB.Length > 0)
                        {
                            // TODO: SPLIT LOGIC
                        }
                        else
                        {
                            // regular behaviour
                            hiLighted = new bool[] { false, false, false };
                            buttons = GlobalFunctions.CreateButtonStringArray(new string[] { $"\u2191 Hit Me", "\u2192 Stay", $"x Exit to Menu " }, ref hiLighted, GlobalFunctions.CursorMaxX);
                            buttons.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Bottom);

                            switch (GlobalFunctions.GetKeyPress(
                        new ConsoleKeyInfo[] {
                            new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false),
                            new ConsoleKeyInfo('1', ConsoleKey.UpArrow, false, false, false),
                            new ConsoleKeyInfo('2', ConsoleKey.RightArrow, false, false, false)
                        }))
                            {
                                // quit
                                case 0:
                                    return false;
                                // hit me
                                case 1:
                                    _bjData.playerHandA = _bjData.playerHandA.Append(Draw()).ToArray();
                                    return true;
                                // stay
                                default:
                                    _bjData.dealerHand = _bjData.dealerHand.Append(Draw()).ToArray();
                                    return true;
                            }
                        }

                        // TEMP THIS
                        return true;
                        // TEMP THIS
                    }
                    // Case: any round but the last round
                    else if (_bjData.dealerHand.Length == 1)
                    {
                        Console.Clear();
                        return true;
                    }
                    // last round (dealer round)
                    else
                    {
                        // dealer hits if under 16
                        if (dealerHand < 16)
                        {
                            _bjData.dealerHand = _bjData.dealerHand.Append(Draw()).ToArray();
                        }
                        return true;
                    }

                    aCard Draw()
                    {
                        _bjData.deck = _bjData.deck.Pop(out aCard card);
                        return card;
                    }


            }
            return false;
        }

        private string[] EndGameButtonStringBlock()
        {
            string[] output = new string[0];
            bool[] hiLighted = new bool[] { false, false, /*false,*/ false };
            //string[] buttons = GlobalFunctions.CreateButtonStringArray(new string[] { $"\u2190 Betting ", $"\u2192 Bet Same", /*"\u2193 Reduce Bet   ",*/ $"x Exit to Menu " }, ref hiLighted, GlobalFunctions.CursorMaxX);
            string[] buttons = GlobalFunctions.CreateButtonStringArray(new string[] { $"<- Adjust Bet", $"-> Same Again", /*"\u2193 Reduce Bet   ",*/ $"x Exit to Menu " }, ref hiLighted, GlobalFunctions.CursorMaxX);
            //string[] buttons = GlobalFunctions.CreateButtonStringArray(new string[] { $"<- Adjust Bet", $"-> Same Bet:${bjData?.bettingAmmountInput}", /*"\u2193 Reduce Bet   ",*/ $"x Exit to Menu " }, ref hiLighted, GlobalFunctions.CursorMaxX);
            for (int i = 0; i < buttons.Length; i++) output = output.Append(buttons[i]).ToArray();
            return output;
        }

        //private bool isGameOver(aCard[] deckIn)
        //{

        //}

        public class BJSettings
        {
            public decimal minimumBet;
            public decimal betIncrement;
            public int deckAmmountPerGame;

            public BJSettings(decimal minimumBet = 20.0m, decimal betIncrement = 20.0m, int deckAmmountPerGame = 1)
            {
                this.minimumBet = minimumBet;
                this.betIncrement = betIncrement;
                this.deckAmmountPerGame = deckAmmountPerGame;
            }
        }

        public class BJData
        {    
            public decimal bettingAmmount = 0;// bet that has been removed from player cash and is currently in play
            public aCard[] deck;
            public aCard[] dealerHand;
            public aCard[] playerHandA;
            public aCard[] playerHandB;

            public BJData(int cardPacks = 8)
            {
                this.deck = BlackJackDeck(cardPacks);
                this.dealerHand = new aCard[0];
                this.playerHandA = new aCard[0];
                this.playerHandB = new aCard[0];
            }

            public BJData(decimal bettingAmmountInput, aCard[] deck, aCard[] dealerHand, aCard[] playerHandA, aCard[] playerHandB)
            {
                this.bettingAmmount = bettingAmmountInput;
                this.deck = deck;
                this.dealerHand = dealerHand;
                this.playerHandA = playerHandA;
                this.playerHandB = playerHandB;
            }
        }

        public static aCard[] BlackJackDeck(int cardPacks = 8)
        {
            if (cardPacks < 1) cardPacks = 1;
            aCard[] deck = new aCard[cardPacks * 52];
            for (int i = 0; i < cardPacks; i++)
            {
                Array.Copy(GameCards.BJDeck(), 0, deck, i * 52, 52);
            }
            return deck.Shuffle();
        }

        

        private void resetBJGame()
        {
            _bjData = new BJData();
        }

        public static int GetBlackJackValue(in aCard[] handIn)
        {
            int output = 0;
            // aces are added up at the end
            int acesCount = 0;
            // go through each card
            for (int i = 0; i < handIn.Length; i++)
            {
                switch (handIn[i].GetCardType)
                {
                    // all face cards are value of 10
                    case aCard.Type.Face: 
                        output += 10; 
                        continue;

                    // number cards are at face value except for aces
                    case aCard.Type.Number:
                        //try to cast into number card
                        NumberCard? numberCard = null;
                        if (handIn[i] is NumberCard)
                        {
                            numberCard = (NumberCard)handIn[i];
                        }
                        // on fail
                        if (numberCard == null)
                        {
                            output += 10;
                            continue;
                        }
                        // gather up an ace
                        else if (numberCard.GetNumber == 1)
                        {
                            acesCount++;
                            continue;
                        }
                        // add the card's face value to the total
                        else
                        {
                            output += numberCard.GetNumber;
                            continue;
                        }
                        // jokers
                    default:
                        Console.WriteLine("UNHANDLED:!!black jack get value function got a joker as input!!");
                        output += 10;
                        continue;
                }
            }

            // go through the aces and try to get as many full value aces as squezed in as possible
            for (int i = acesCount; i > 0; i--)
            {
                int value = output + (i * 11) + (acesCount - i);
                if (value <= 21)
                {
                    return value;
                }
                else continue;
            }

            // if we reached here it means we weren't able to squeeze any full value aces in
            return output + acesCount;
        }

        public static int GetBestBlackJackValue(int handA, int handB)
        {
            //both bust or both empty
            if ((handA > 21 && handB > 21) || (handA <= 0 && handB <= 0)) return handA;
            // one bust or empty
            else if (handA <= 0) return handB;
            else if (handB <= 0) return handA;
            else if (handA > 21) return handB;
            else if (handB > 21) return handA;
            else
            {
                int distFromTargetA = Math.Abs(21 - handA);
                int distFromTargetB = Math.Abs(21 - handB);
                if (distFromTargetA <= distFromTargetB) return handA;
                else return handB;
            }
        }

        public static bool CardsAreSplittable(aCard cardA, aCard cardB)
        {
            if (cardA == null || cardB == null) return false;

            aCard.Type cardAType = cardA.GetCardType;

            // cards must be same type
            if (cardAType != cardB.GetCardType) return false;

            switch (cardAType)
            {
                // don't be silly
                case aCard.Type.Joker: return false;

                // if both are face cards
                case aCard.Type.Face:
                    // try cast them to face cards
                    FaceCard? faceCardA = cardA as FaceCard;
                    FaceCard? faceCardB = cardB as FaceCard;
                    // if it worked on both and both are the same face as well then they are splittable
                    if (faceCardA != null &&
                        faceCardB != null &&
                        faceCardA.GetFace == faceCardB.GetFace) return true;
                    else return false;

                // if both are number cards or aces
                case aCard.Type.Number:
                    // try cast them to face cards
                    NumberCard? numberCardA = cardA as NumberCard;
                    NumberCard? numberCardB = cardB as NumberCard;
                    // if it worked on both and both are the same number (or aces) as well then they are splittable
                    if (numberCardA != null &&
                        numberCardB != null &&
                        numberCardA.GetNumber == numberCardB.GetNumber) return true;
                    else return false;

                default: return false;
            }
        }

        private void displayWelcomeMessage()
        {
            Console.Clear();
            string welcome = $"\nWelcome to the BlackJack table {Environment.UserName}\n";
            string instruction = $"\nPress 'x' at any time to exit.\nPlease place your bets for the next hand, the table minimum is ${_bjSettings.minimumBet}\n";
            Console.WriteLine(welcome);
            Console.WriteLine(instruction);
        }



        private void changeState(GameState targetState) 
        {
            switch (targetState)
            {
                default: _state = targetState;
                    return;
            }
        }

        private int getBettingInput()
        {
            ConsoleKeyInfo[] validKeys = new ConsoleKeyInfo[] 
            { 
                new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false),
                new ConsoleKeyInfo('1', ConsoleKey.UpArrow, false, false, false),
                new ConsoleKeyInfo('2', ConsoleKey.RightArrow, false, false, false),
                new ConsoleKeyInfo('2', ConsoleKey.DownArrow, false, false, false)
            };
            return GlobalFunctions.GetKeyPress(validKeys);
        }
    }

    public static class BlackJackExtensions
    {
        public static aCard[] Pop(this aCard[] deck, out aCard card)
        {
            if (deck.Length <= 1) deck = BlackJack.BlackJackDeck();
            deck = GlobalFunctions.Pop(deck, out card);
            return deck;
        }
    }
}
