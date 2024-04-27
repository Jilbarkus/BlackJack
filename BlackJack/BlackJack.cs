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
                        $"Current Bet: ${_bjData?.BettingAmmount:0}           Funds: {player.Cash}"
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
                            if (player.Cash >= (_bjSettings.betIncrement + _bjData.BettingAmmount)) _bjData.BettingAmmount += _bjSettings.betIncrement;
                            break;
                        case 2:
                            if (_bjData.BettingAmmount >= _bjSettings.minimumBet)
                            {
                                //resetBJGame();
                                player.AddCash(-_bjData.BettingAmmount);
                                Console.Clear();
                                changeState(GameState.Dealing);
                            }
                            break;
                        case 3:
                            if (_bjData.BettingAmmount - _bjSettings.betIncrement >= 0) _bjData.BettingAmmount -= _bjSettings.betIncrement;
                            break;
                        default: break;
                    }
                    return true;

                // entry into either a new game or an ongoing one
                case GameState.Dealing:
                    // does the player get to input this round?
                    bool bTakeInput = true;
                    // how much player has bet
                    Decimal Bet = _bjData.BettingAmmount;
                    // Case: Round 1
                    if (_bjData.dealerHand.Length == 0)
                    {
                        _bjData.dealerHand = _bjData.dealerHand.Append(Draw()).ToArray();
                        _bjData.PlayerHands[0].DealCardIntoHand(ref _bjData.Deck);
                        bTakeInput = false;
                    }
                    // Case: Round 2
                    else if (_bjData.dealerHand.Length == 1 && _bjData.PlayerHands[0].Cards.Length == 1 && _bjData.PlayerHands.Length == 1)
                    {
                        _bjData.PlayerHands[0].DealCardIntoHand(ref _bjData.Deck);
                        bTakeInput = false;

                    }
                    // Case: Final Round
                    else if (_bjData.dealerHand.Length > 1)
                    {
                        bTakeInput = false;
                    }
                    // calculate player values
                    int dealerHand = GetBlackJackValue(_bjData.dealerHand);
                    int playerHandIndex = FirstUsableHand(in _bjData.PlayerHands);
                    List<int> playerValues = new List<int>();
                    int bestPlayerValue = 0;

                    for (int i = 0; i < _bjData.PlayerHands.Length; i++)
                    {
                        int handValue = GetBlackJackValue(_bjData.PlayerHands[i].Cards);
                        playerValues.Add(handValue);
                        bestPlayerValue = GetBestBlackJackValue(bestPlayerValue, handValue);
                    }

                    // Case: Player Bust
                    if (_bjData.dealerHand.Length == 1 && bestPlayerValue >/*=*/ 21)
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
                    // add a spacer so that player's cards are'nt grouped together with dealer's cards (spacer is different ammount of lines so next bit will be placed in a new block)
                    currentOutput = currentOutput.Append("      ").ToArray();

                    for (int i = 0; i < _bjData.PlayerHands.Length; i++)
                    {
                        // each card in that deck
                        for (int k = 0; k < _bjData.PlayerHands[i].Cards.Length; k++)
                        {
                            currentOutput = currentOutput.AddStringBlockHorizontal(_bjData.PlayerHands[i].Cards[k].CardToString(), GlobalFunctions.CursorMaxX);
                        }

                        // we want to put a card sized spacer between each split deck (only if there is more than 1 deck and we don't need to put a spacer after the last deck)
                        if (_bjData.PlayerHands.Length > 1 && i < _bjData.PlayerHands.Length - 1)
                        {
                            currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.CardSplitSpacer, GlobalFunctions.CursorMaxX);
                        }
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
                                player.AddCash(_bjData.BettingAmmount);
                            }
                            // blackjack
                            else if (bestPlayerValue == 21)
                            {
                                currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.BlackJackStringBlock.StringMultiLineToStringArray());
                                // blackjack without hitting
                                //if (_bjData.playerHandA.Length + _bjData.playerHandB.Length == 2)
                                if (_bjData.PlayerHands.Length == 1 && _bjData.PlayerHands[0].Cards.Length == 2)
                                {
                                    
                                    player.AddCash(1.5m * _bjData.BettingAmmount);
                                }
                                else
                                {
                                    player.AddCash(2m * _bjData.BettingAmmount);
                                }
                            }
                            // dealer bust
                            else if (dealerHand > 21)
                            {
                                currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.DealerBustStringBlock.StringMultiLineToStringArray());
                                player.AddCash(2m * _bjData.BettingAmmount);
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
                                player.AddCash(2m * _bjData.BettingAmmount);
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
                            new string[3] { $"Bank: ${player.Cash}",$"Bet:{_bjData.BettingAmmount}",$"Winnings: ${player.Cash - playerCashBuffer - _bjData.BettingAmmount}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Right, JustifyY.Top);
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
                                    decimal prevBet = _bjData.BettingAmmount;
                                    resetBJGame();
                                    player.AddCash(-prevBet);
                                    _bjData.BettingAmmount = prevBet;
                                    changeState(GameState.Dealing);
                                    Console.Clear();
                                    //break;
                                    return true;
                                    // add more console clears and change AddStringBlockHorizontal so that it doesnt group both hands together
                            }
                        }
                    }

                    if (!bTakeInput) currentOutput.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Top);


                    //new string[2] { $"Bank: ${player.Cash}", $"Bet: ${_bjData.BettingAmmount}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Right, JustifyY.Top);
                    //if (_bjData.playerHandB.Length == 0) new string[2] { $"Dealer's hand: {dealerHand}", $"{player.Name}'s hand: {bestPlayerValue}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Top);
                    //else new string[3] { $"Dealer's hand: {dealerHand}", $"{player.Name}'s best split: {bestPlayerValue}", $"Left Split: {playerHandA}, Right Split: {playerHandB}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Top);
                    // Right Top Text
                    new string[2] { $"Bank: ${player.Cash}", $"Bet: ${_bjData.BettingAmmount}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Right, JustifyY.Top);
                    // Left Top Text
                    if (_bjData.PlayerHands.Length == 1) new string[2] { $"Dealer's hand: {dealerHand}", $"{player.Name}'s hand: {bestPlayerValue}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Top);
                    else
                    {
                        string[] topLeftTextBlock = new string[3] { $"Dealer's hand: {dealerHand}", $"{player.Name}'s best split: {bestPlayerValue}", String.Empty };
                        StringBuilder sb = new StringBuilder();
                        for (int i = 1; i< _bjData.PlayerHands.Length; i++)
                        {
                            sb.Append("Split #");
                            sb.Append(i - 1);
                            sb.Append(": ");
                            sb.Append(playerValues[i]);
                            if (i < _bjData.PlayerHands.Length - 1) sb.Append(",");
                        }
                        topLeftTextBlock[2] = sb.ToString();

                        
                    }

                    //////////// input   ////////////
                    ///
                    if (bTakeInput)
                    {
                        currentOutput.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Top);
                        currentOutput = new string[0];

                        // Case: player can INITIATE a split or double double down
                        if (_bjData.PlayerHands[0].Cards.Length == 2 &&
                            player.Cash >= _bjData.BettingAmmount)
                        {
                            List<int> decksThatCanSplit = new List<int>();
                            List<int> decksThaCanDoubleDown = new List<int>();
                            for (int i = 0; i < _bjData.PlayerHands.Length; i++)
                            {
                                if (_bjData.PlayerHands[i].CanSplit()) decksThatCanSplit.Add(i);
                            }
                            bool bSplittable = (decksThatCanSplit.Count > 0);
                            //bool bDoubleDownable = (playerHandA >= 9 && playerHandA <= 11);
                            bool bDoubleDownable = CardsAreDoubleDownable(_bjData.PlayerHands[0].Cards[0], _bjData.PlayerHands[0].Cards[1], bestPlayerValue);

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
                                    //_bjData.playerHandA = _bjData.playerHandA.Append(Draw()).ToArray();
                                    _bjData.PlayerHands[playerHandIndex].DealCardIntoHand(ref _bjData.Deck);
                                    return true;
                                // stay
                                case 2:
                                    if (_bjData.PlayerHands.Length <= 1)
                                    {
                                        _bjData.dealerHand = _bjData.dealerHand.Append(Draw()).ToArray();
                                    }
                                    else if (_bjData.PlayerHands[playerHandIndex].BHeld == false)
                                    {
                                        _bjData.PlayerHands[playerHandIndex].BHeld = true;
                                        return true;
                                    }

                                    Console.Clear();
                                    Console.WriteLine("Logic Error: attempting to stay on a hand that has already stayed");
                                    return true;

                                //_bjData.dealerHand = _bjData.dealerHand.Append(Draw()).ToArray();
                                //return true;
                                case 3:
                                    // check valid
                                    if (!bSplittable || decksThatCanSplit.Count < 1) return false;
                                    // which hand will be split
                                    int handToBeSplitIndex = decksThatCanSplit[0];
                                    // check valid
                                    if (_bjData.PlayerHands[handToBeSplitIndex].Cards.Length < 2) return false;
                                    // bet
                                    player.AddCash(-_bjData.BettingAmmount);
                                    _bjData.BettingAmmount *= 2;
                                    
                                    // new hand
                                    BJHand freshSplit = new BJHand();
                                    // put the second card from the hand to be split into the new hand
                                    freshSplit.Cards = new aCard[] { _bjData.PlayerHands[handToBeSplitIndex].Cards[1] };
                                    // remove the second card from the first hand
                                    _bjData.PlayerHands[handToBeSplitIndex].Cards = new aCard[] { _bjData.PlayerHands[handToBeSplitIndex].Cards[0] };
                                    // add the new hand into the data
                                    _bjData.PlayerHands = _bjData.PlayerHands.Append(freshSplit).ToArray();
                                    return true;
                                case 4:
                                    if (!bDoubleDownable) return false;
                                    player.AddCash(-_bjData.BettingAmmount);
                                    _bjData.BettingAmmount *= 2;
                                    //_bjData.playerHandA = _bjData.playerHandA.Append(Draw()).ToArray();
                                    _bjData.PlayerHands[playerHandIndex].DealCardIntoHand(ref _bjData.Deck);
                                    _bjData.dealerHand = _bjData.dealerHand.Append(Draw()).ToArray();
                                    return true;

                            }

                        }
                        // case player is in the midst of a split
                        //else if (_bjData.playerHandB.Length > 0)
                        else if (_bjData.PlayerHands.Length > 1)
                        {
                            // TODO: SPLIT LOGIC ////////////////////////////////////////////////////////////////////
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
                                    //_bjData.playerHandA = _bjData.playerHandA.Append(Draw()).ToArray();
                                    _bjData.PlayerHands[playerHandIndex].DealCardIntoHand(ref _bjData.Deck);
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
                        _bjData.Deck = _bjData.Deck.Pop(out aCard card);
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
            public decimal BettingAmmount = 0;// bet that has been removed from player cash and is currently in play
            public aCard[] Deck;
            public aCard[] dealerHand;
            //public aCard[] playerHandA;
            //public aCard[] playerHandB;
            //public List<aCard[]> PlayerHands;
            public BJHand[] PlayerHands;

            public BJData(int cardPacks = 8)
            {
                this.Deck = BlackJackDeck(cardPacks);
                this.dealerHand = new aCard[0];
                //this.playerHandA = new aCard[0];
                //this.playerHandB = new aCard[0];
                //this.PlayerHands = new List<aCard[]> { new aCard[0] };
                this.PlayerHands = new BJHand[1] { new BJHand() };
            }

            //public BJData(decimal bettingAmmountInput, aCard[] deck, aCard[] dealerHand, aCard[] playerHandA, aCard[] playerHandB)
            //{
            //    this.bettingAmmount = bettingAmmountInput;
            //    this.deck = deck;
            //    this.dealerHand = dealerHand;
            //    this.playerHandA = playerHandA;
            //    this.playerHandB = playerHandB;
            //}
        }

        public class BJHand
        {
            internal aCard[] Cards = new aCard[0];
            internal bool BHeld = false;

            internal void DealCardIntoHand(ref aCard[] deckSource)
            {
                deckSource = deckSource.Pop(out aCard card);
                if (card == null) { Console.WriteLine("Failed to Deal Card Into Deck, No Card Recieved?"); return; }
                this.Cards = Cards.Append(card).ToArray();
            }

            internal bool CanSplit()
            {
                if (Cards.Length != 2) return false;
                return CardsAreSplittable(Cards[0], Cards[1]);
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
            if (handIn == null) 
            { 
                Console.WriteLine("tried to calculate BlackJack.GetBlackJackValue with a null input");
                return 0;
            }

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

        public static bool CardsAreDoubleDownable(aCard cardA, aCard cardB, int handValue = -1)
        {
            if (cardA == null || cardB == null) return false;

            if (handValue < 0) handValue = GetBlackJackValue(new aCard[] { cardA, cardB });

            bool bAnyAces = false;

            if (cardA.GetCardType == aCard.Type.Number)
            {
                NumberCard cardAnum = (NumberCard)cardA;
                if (cardAnum != null && cardAnum.GetNumber == 1) bAnyAces = true;
            }

            if (cardB.GetCardType == aCard.Type.Number)
            {
                NumberCard cardBnum = (NumberCard)cardB;
                if (cardBnum != null && cardBnum.GetNumber == 1) bAnyAces = true;
            }

            if (bAnyAces == false && 
                9 <= handValue &&
                handValue <= 11) return true;
            
            if (bAnyAces == true &&
                16 <= handValue &&
                handValue <= 18) return true;

            return false;
        }

        public static int FirstUsableHand(in BJHand[] hands)
        {
            if (hands == null || 
                hands.Length <= 1)
            { return 0; }
            
            for (int i = 0; i < hands.Length; i++)
            {
                if (hands[i].BHeld == false &&
                    GetBlackJackValue(hands[i].Cards) < 21) return i;
            }

            // shouldn't reach here
            Console.Clear();
            Console.WriteLine("error: firstUsableDeck() couldn't find a suitable hand to use, defaulting to first one");
            return 0;
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
