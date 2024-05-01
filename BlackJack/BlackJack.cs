using System.Diagnostics;
using System.Text;
using static CardGames.BlackJack;

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
            //clear screen from last 'Tick', each iteration of this method will fill the Console window with a new frame
            Console.Clear();
            // in case there is no game start one
            if (_bjData == null) { resetBJGame(); changeState(GameState.Betting); return true; }
            string[] currentOutput = new string[0];
            switch (_state)
            {
                case GameState.Betting:
                    currentOutput = new string[] {
                        $"Please place your bets for the next hand, the table minimum is ${_bjSettings.minimumBet}",
                        $"Current Bet: ${_bjData.PlayerHands[0].Bet}           Funds: {player.Cash}"
                    };
                    bool[] hiLighted = new bool[] { false, false, false, false };
                    string[] buttons = GlobalFunctions.CreateButtonStringArray(new string[] { $"\u2191 Add Bet +{_bjSettings.betIncrement}", "\u2192 Start Dealing", "\u2193 Reduce Bet   ", $"x Exit to Menu " }, ref hiLighted, GlobalFunctions.CursorMaxX);
                    for (int i = 0; i < buttons.Length; i++) currentOutput = currentOutput.Append(buttons[i]).ToArray();
                    //Console.Clear();
                    currentOutput.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Center);

                    switch (getBettingInput())
                    {
                        case 0: return false;
                        case 1:
                            if (player.Cash >= _bjSettings.betIncrement)
                            {
                                _bjData.PlayerHands[0].Bet += _bjSettings.betIncrement;
                                player.AddCashAndSave(-_bjSettings.betIncrement);

                            }
                            break;
                        case 2:
                            if (_bjData.PlayerHands[0].Bet >= _bjSettings.minimumBet)
                            {
                                //resetBJGame();
                                
                                //Console.Clear();
                                changeState(GameState.Dealing);
                                _bjSettings.mostRecentBet = _bjData.PlayerHands[0].Bet;
                            }
                            break;
                        case 3:
                            if (_bjData.PlayerHands[0].Bet - _bjSettings.betIncrement >= 0m)
                            {
                                _bjData.PlayerHands[0].Bet -= _bjSettings.betIncrement;
                                player.AddCashAndSave(_bjSettings.betIncrement);
                            }
                            break;
                        default: break;
                    }
                    return true;

                // entry into either a new game or an ongoing one
                case GameState.Dealing:
                    //////////// game logic    ////////////
                    ///
                    // does the player get to input this round?
                    bool bTakeInput = true;
                    // how much player has bet
                    Decimal totalBet = 0m;
                    for (int i = 0; i < _bjData.PlayerHands.Length; i++) totalBet += _bjData.PlayerHands[i].Bet;
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
                    bool bAllPlayerHandsStay = (playerHandIndex == -1);
                    if (bAllPlayerHandsStay) playerHandIndex = 0;
                    List<int> playerValues = new List<int>();
                    int bestPlayerValue = 0;

                    for (int i = 0; i < _bjData.PlayerHands.Length; i++)
                    {
                        int handValue = GetBlackJackValue(_bjData.PlayerHands[i].Cards);
                        playerValues.Add(handValue);
                        bestPlayerValue = GetBestBlackJackValue(bestPlayerValue, handValue);
                    }

                    // Case: Initiate Start dealer logic loop:
                    // -Player Bust
                    // -all player hands have 'stayed'
                    if (_bjData.dealerHand.Length == 1 && 
                        (bestPlayerValue > 21 || bAllPlayerHandsStay))
                    {
                        _bjData.dealerHand = _bjData.dealerHand.Append(Draw()).ToArray();
                        return true;
                    }

                    //////////// display cards ////////////
                    ///
                    currentOutput = new string[0];
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
                            //currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.CardSplitSpacer, GlobalFunctions.CursorMaxX);

                            if (i >= playerHandIndex) currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.CardSplitSpacerArrowLeft, GlobalFunctions.CursorMaxX);
                            else currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.CardSplitSpacerArrowRight, GlobalFunctions.CursorMaxX);
                        }
                    }

                    // print to screen
                    currentOutput.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Top);
                    currentOutput = new string[0];
                    // Top Left Text
                    new string[2] { " ", $"Dealer's hand: {dealerHand}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Top);
                    // Middle Left Text
                    if (playerValues.Count > playerHandIndex) new string[1] { $"{player.Name}'s hand: {playerValues[playerHandIndex]}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Center);

                    //////////// endgame ////////////
                    ///
                    // Case: Last round
                    if (_bjData.dealerHand.Length > 1)
                    {
                        bool bEndGame = dealerHand > bestPlayerValue || dealerHand >= 16;
                        decimal playerCashBuffer = player.Cash;
                        #region old splash screen and bet calculation
                        //if (bEndGame)
                        //{
                        //    // add a spacer so that winning string block isn't grouped together with dealer's cards
                        //    currentOutput = currentOutput.Append("      ").ToArray();
                        //    //dead heat
                        //    if ((dealerHand > 21 && bestPlayerValue > 21) || dealerHand == bestPlayerValue)
                        //    {
                        //        currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.DeadHeatStringBlock.StringMultiLineToStringArray());
                        //        player.AddCash(_bjData.BettingAmmount);
                        //    }
                        //    // blackjack
                        //    else if (bestPlayerValue == 21)
                        //    {
                        //        currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.BlackJackStringBlock.StringMultiLineToStringArray());
                        //        // blackjack without hitting
                        //        //if (_bjData.playerHandA.Length + _bjData.playerHandB.Length == 2)
                        //        if (_bjData.PlayerHands.Length == 1 && _bjData.PlayerHands[0].Cards.Length == 2)
                        //        {

                        //            player.AddCash(1.5m * _bjData.BettingAmmount);
                        //        }
                        //        else
                        //        {
                        //            player.AddCash(2m * _bjData.BettingAmmount);
                        //        }
                        //    }
                        //    // dealer bust
                        //    else if (dealerHand > 21)
                        //    {
                        //        currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.DealerBustStringBlock.StringMultiLineToStringArray());
                        //        player.AddCash(2m * _bjData.BettingAmmount);
                        //    }
                        //    // player bust
                        //    else if (bestPlayerValue > 21)
                        //    {
                        //        currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.BustStringBlock2.StringMultiLineToStringArray());
                        //    }
                        //    // player winning hand - needs change
                        //    else if (bestPlayerValue > dealerHand)
                        //    {
                        //        currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.WinningHandStringBlock.StringMultiLineToStringArray());
                        //        player.AddCash(2m * _bjData.BettingAmmount);
                        //    }
                        //    // dealer winning hand
                        //    else
                        //    {
                        //        currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.HandLostStringBlock.StringMultiLineToStringArray());
                        //    }

                        //} 
                        #endregion

                        if (bEndGame)
                        {
                            // Splash Message and buttons
                            if ((dealerHand > 21 && bestPlayerValue > 21) || dealerHand == bestPlayerValue)
                            {
                                currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.DeadHeatStringBlock.StringMultiLineToStringArray());
                            }
                            // blackjack
                            else if (bestPlayerValue == 21)
                            {
                                currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.BlackJackStringBlock.StringMultiLineToStringArray());
                            }
                            // dealer bust
                            else if (dealerHand > 21)
                            {
                                currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.DealerBustStringBlock.StringMultiLineToStringArray());
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
                            }
                            // dealer winning hand
                            else
                            {
                                currentOutput = currentOutput.AddStringBlockHorizontal(GlobalFunctions.HandLostStringBlock.StringMultiLineToStringArray());
                            }
                            buttons = EndGameButtonStringBlock();
                            currentOutput = currentOutput.AddStringBlockHorizontal(buttons, GlobalFunctions.CursorMaxX);
                            currentOutput.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Bottom);

                            // Winnings Calculation for each hand
                            for (int i = 0; i < playerValues.Count; i++)
                            {
                                // check data exists
                                if (_bjData.PlayerHands[i] == null) { Console.WriteLine("Error: Couldn't locate hand assosciated with hand value!"); continue; } 
                                // dead heat
                                if (dealerHand > 21 && playerValues[i] > 21
                                    || dealerHand == playerValues[i])
                                {
                                    player.AddCashAndSave(_bjData.PlayerHands[i].Bet);
                                }
                                // blackjack
                                else if (playerValues[i] == 21)
                                {
                                    // Case: blackjack without hitting
                                    if (_bjData.PlayerHands[i].Cards.Length == 2)
                                    {
                                        player.AddCashAndSave(_bjData.PlayerHands[i].Bet * 1.5m);
                                    }
                                    // Case: blackjack after hitting at least one card
                                    else
                                    {
                                        player.AddCashAndSave(_bjData.PlayerHands[i].Bet * 2m);
                                    }
                                }
                                // dealer bust
                                else if (dealerHand > 21)
                                {
                                    player.AddCashAndSave(_bjData.PlayerHands[i].Bet * 2m);
                                }
                                // player bust
                                else if (playerValues[i] > 21)
                                {
                                    // no winnings given
                                }
                                // player winning hand
                                else if (playerValues[i] > dealerHand)
                                {
                                    player.AddCashAndSave(_bjData.PlayerHands[i].Bet * 2m);
                                }
                                // dealer winning hand
                                else
                                {
                                    // no winnings given
                                }
                            }
                            // Top Right  Text
                            new string[3] { $"Bank: ${player.Cash}",$"Total Bet:-${totalBet}",$"Winnings: ${player.Cash - playerCashBuffer /*- _bjData.BettingAmmount*/}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Right, JustifyY.Top);
                            //new string[2] { $"Dealer's hand: {dealerHand}", $"{player.Name}'s hand: {bestPlayerValue}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Left, JustifyY.Top);
                            switch (GlobalFunctions.GetKeyPress(
                        new ConsoleKeyInfo[] {
                            new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false),
                            new ConsoleKeyInfo('1', ConsoleKey.LeftArrow, false, false, false),
                            new ConsoleKeyInfo('2', ConsoleKey.RightArrow, false, false, false)}))
                            {
                                // quit
                                case 0:
                                    resetBJGame();
                                    //Console.Clear();
                                    changeState(GameState.Betting);
                                    return false;

                                // adjust bet
                                case 1:
                                    resetBJGame();
                                    //Console.Clear();
                                    changeState(GameState.Betting);
                                    return true;

                                // same again
                                default:
                                    resetBJGame();
                                    if (player.Cash < _bjSettings.mostRecentBet) return false;
                                    player.AddCashAndSave(-_bjSettings.mostRecentBet);
                                    _bjData.PlayerHands[0].Bet = _bjSettings.mostRecentBet;
                                    changeState(GameState.Dealing);
                                    return true;
                            }
                        }
                        new string[1] { "Dealing..." }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Center);
                        // each time the dealer gets a new card give him a second to 'think'
                        Thread.Sleep(300);
                    }

                    //// print to screen
                    //currentOutput.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Top);

                    
                    // Top Right  Text
                    if (_bjData.PlayerHands.Length > playerHandIndex) new string[3] { " ", $"Bank: ${player.Cash}", $"Current Hand's Bet: -${_bjData.PlayerHands[playerHandIndex].Bet}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Right, JustifyY.Top);

                    

                    //////////// input   ////////////
                    ///
                    if (bTakeInput)
                    {
                        //currentOutput = new string[0];
                        int splitIndexA = 0;
                        int splitIndexB = 1;

                        // input
                        bool bCanAffordToDouble = player.Cash >= _bjData.PlayerHands[playerHandIndex].Bet;
                        bool bSplittable2 = bCanAffordToDouble && _bjData.PlayerHands[playerHandIndex].CanSplit(out splitIndexA, out splitIndexB);
                        bool bDoubleDownable2 = bCanAffordToDouble && _bjData.PlayerHands[playerHandIndex].CanDoubleDown();
                        string[] buttonsText2 = new string[2] { $"\u2191 Hit Me", "\u2192 Stay" };
                        if (bSplittable2)
                        {
                            buttonsText2 = buttonsText2.Append($"\u2190 Split ").ToArray();
                        }
                        if (bDoubleDownable2)
                        {
                            buttonsText2 = buttonsText2.Append($"\u2193 Double").ToArray();
                        }
                        buttonsText2 = buttonsText2.Append("x Exit to Menu ").ToArray();

                        hiLighted = new bool[buttonsText2.Length];
                        buttons = GlobalFunctions.CreateButtonStringArray(buttonsText2, ref hiLighted, GlobalFunctions.CursorMaxX);
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
                                _bjData.PlayerHands[playerHandIndex].DealCardIntoHand(ref _bjData.Deck);
                                return true;
                            // stay
                            case 2:
                                //// Case: player only has one hand and it has stayed, let dealer start
                                //if (_bjData.PlayerHands.Length <= 1)
                                //{
                                //    _bjData.PlayerHands[0].BHeld = true;
                                //    _bjData.dealerHand = _bjData.dealerHand.Append(Draw()).ToArray();
                                //}
                                //// Case player has split and can move on to next hand
                                //else if (_bjData.PlayerHands[playerHandIndex].BHeld == false)
                                //{
                                //    _bjData.PlayerHands[playerHandIndex].BHeld = true;
                                //    return true;
                                //}

                                //Console.Clear();
                                //Console.WriteLine("Logic Error: attempting to stay on a hand that has already stayed");
                                //return true;
                                _bjData.PlayerHands[playerHandIndex].BHeld = true;
                                return true;
                            case 3:
                                // check valid
                                if (!bSplittable2) return true;
                                // new hand
                                BJHand freshSplit = new BJHand();
                                // betting
                                player.AddCashAndSave(-_bjData.PlayerHands[playerHandIndex].Bet);
                                freshSplit.Bet = _bjData.PlayerHands[playerHandIndex].Bet;
                                // splitting cards
                                List<aCard> playerHandCards = new List<aCard>();
                                List<aCard> freshSplitCards = new List<aCard>();
                                for (int i = 0; i < _bjData.PlayerHands[playerHandIndex].Cards.Length; i++)
                                {
                                    // put the second card from the hand to be split into the new hand
                                    if (i == splitIndexB) freshSplitCards.Add(_bjData.PlayerHands[playerHandIndex].Cards[i]);
                                    // all others to main hand
                                    else playerHandCards.Add(_bjData.PlayerHands[playerHandIndex].Cards[i]);
                                }
                                _bjData.PlayerHands[playerHandIndex].Cards = playerHandCards.ToArray();
                                freshSplit.Cards = freshSplitCards.ToArray();
                                // add the new hand into the data
                                _bjData.PlayerHands = _bjData.PlayerHands.Append(freshSplit).ToArray();
                                return true;
                            case 4:
                                if (!bDoubleDownable2) return true;
                                player.AddCashAndSave(-_bjData.PlayerHands[playerHandIndex].Bet);
                                _bjData.PlayerHands[playerHandIndex].Bet *= 2;
                                _bjData.PlayerHands[playerHandIndex].DealCardIntoHand(ref _bjData.Deck);
                                //_bjData.dealerHand = _bjData.dealerHand.Append(Draw()).ToArray();
                                _bjData.PlayerHands[playerHandIndex].BHeld = true;
                                return true;

                        }
                        // TEMP THIS
                        return true;
                        // TEMP THIS
                    }
                    // Case: any round but the last round
                    else if (_bjData.dealerHand.Length == 1)
                    {
                        //Console.Clear();
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

        public class BJSettings
        {
            public decimal minimumBet;
            public decimal betIncrement;
            public decimal mostRecentBet;
            public int deckAmmountPerGame;

            public BJSettings(decimal minimumBet = 20.0m, decimal betIncrement = 20.0m, int deckAmmountPerGame = 1, decimal mostRecentBet = 0)
            {
                this.minimumBet = minimumBet;
                this.betIncrement = betIncrement;
                this.deckAmmountPerGame = deckAmmountPerGame;
                this.mostRecentBet = mostRecentBet;
            }
        }

        public class BJData
        {    
            //public decimal BettingAmmount = 0;// bet that has been removed from player cash and is currently in play
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
                this.PlayerHands = new BJHand[1] { new BJHand() };
            }
        }

        public class BJHand
        {
            internal aCard[] Cards = new aCard[0];
            internal bool BHeld = false;
            internal decimal Bet = 0m;

            internal void DealCardIntoHand(ref aCard[] deckSource)
            {
                deckSource = deckSource.Pop(out aCard card);
                if (card == null) { Console.WriteLine("Failed to Deal Card Into Deck, No Card Recieved?"); return; }
                this.Cards = Cards.Append(card).ToArray();
            }

            internal bool CanSplit(out int cardIndexA, out int cardIndexB)
            {
                cardIndexA = -1;
                cardIndexB = -1;
                if (Cards == null) return false;
                int cardLength = Cards.Length;
                if (cardLength < 2) return false;

                for (int i = 0; 
                    i < cardLength - 1; // don't do last card
                    i++)
                {
                    for (int k = i + 1;
                        k < cardLength;
                        k++)
                    {
                        if (CardsAreSplittable(Cards[i], Cards[k]))
                        {
                            cardIndexA = i;
                            cardIndexB = k;
                            return true;
                        }
                    }
                }

                return false;
            }

            internal bool CanDoubleDown(int handValue = 1)
            {
                if (Cards == null || 
                    Cards.Length != 2) return false;

                return CardsAreDoubleDownable(Cards[0], Cards[1]/*, handValue*/);
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

        //public static bool CardsAreDoubleDownable(aCard cardA, aCard cardB, int handValue = -1)
        //{
        //    if (cardA == null || cardB == null) return false;

        //    if (handValue < 0) handValue = GetBlackJackValue(new aCard[] { cardA, cardB });

        //    bool bAnyAces = false;

        //    if (cardA.GetCardType == aCard.Type.Number)
        //    {
        //        NumberCard cardAnum = (NumberCard)cardA;
        //        if (cardAnum != null && cardAnum.GetNumber == 1) bAnyAces = true;
        //    }

        //    if (cardB.GetCardType == aCard.Type.Number)
        //    {
        //        NumberCard cardBnum = (NumberCard)cardB;
        //        if (cardBnum != null && cardBnum.GetNumber == 1) bAnyAces = true;
        //    }

        //    if (bAnyAces == false && 
        //        9 <= handValue &&
        //        handValue <= 11) return true;
            
        //    if (bAnyAces == true &&
        //        16 <= handValue &&
        //        handValue <= 18) return true;

        //    return false;
        //}

        public static bool CardsAreDoubleDownable(aCard cardA, aCard cardB)
        {
            if (cardA == null || cardB == null) return false;
            int handTotal = 0;
            int aceCount = 0;
            // card A
            if (cardA.GetCardType == aCard.Type.Number)
            {
                if (HandleNumberCard(cardA) == false) return false;
            }
            else if (cardA.GetCardType == aCard.Type.Face)
            {
                if (HandleFaceCard(cardA) == false) return false;
            }
            else return false;
            // card B
            if (cardB.GetCardType == aCard.Type.Number)
            {
                if (HandleNumberCard(cardB) == false) return false;
            }
            else if (cardB.GetCardType == aCard.Type.Face)
            {
                if (HandleFaceCard(cardB) == false) return false;
            }
            else return false;
            // Logic
            if (aceCount == 0 &&
                WithinDoubleDownRange(handTotal)) return true;
            else if (aceCount == 1 &&
                (WithinDoubleDownRange(handTotal + 1) || WithinDoubleDownRange(handTotal + 11))) return true;
            else if (aceCount == 2) return false;
            else return false;

            bool HandleNumberCard(aCard card)
            {
                // try cast
                NumberCard cardNum = (NumberCard)card;
                if (cardNum == null) { Console.WriteLine("CardsAreDoubleDownable() failing to cast card!"); return false; }
                // add number card value to total or add ace counter if an ace
                if (cardNum.GetNumber == 1) aceCount++;
                else handTotal += cardNum.GetNumber;
                return true;
            }
            bool HandleFaceCard(aCard card)
            {
                // try cast
                FaceCard cardFace = (FaceCard)card;
                if (cardFace == null) { Console.WriteLine("CardsAreDoubleDownable() failing to cast card!"); return false; }
                // add 10
                handTotal += 10;
                return true;
            }
            bool WithinDoubleDownRange(int value)
            {
                if (9 <= value &&
                    value <= 11) return true;
                return false;
            }
        }

        /// <summary>
        /// First usable Hand Index or -1
        /// </summary>
        /// <param name="hands"></param>
        /// <returns>index of first usable hand that hasn't stayed or busted, if none found will return -1</returns>
        public static int FirstUsableHand(in BJHand[] hands)
        {
            if (hands == null || 
                hands.Length </*=*/ 1)
            { return 0; }
            
            for (int i = 0; i < hands.Length; i++)
            {
                if (hands[i].BHeld == false &&
                    GetBlackJackValue(hands[i].Cards) < 21) return i;
            }
            // no hand found
            return -1;
        }

        private void displayWelcomeMessage()
        {
            //Console.Clear();
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
