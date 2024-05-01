namespace CardGames
{
    public enum GameTypes { GameChooser, BlackJack };
    public static class Program
    {
        public static void Main()
        {
            aCard[] deck = GameCards.BJDeck();
            PlayerSave? playerLoad = PlayerSave.LoadSecure();
            //PlayerSave player = (playerLoad != null)? playerLoad : new PlayerSave(Environment.UserName.ToString(), 100.0m);
            PlayerSave player;
            bool bPlayWelcomeMessage = false;
            if (playerLoad != null) player = playerLoad;
            else
            {
                player = new PlayerSave(Environment.UserName.ToString(), 100.0m);
                player.SaveSecure();
                bPlayWelcomeMessage = true;
            }
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            int currentGame = -1;
            // GAMES // 
            IGame[] games = new List<IGame>
            {
                new BlackJack()
                //,new TestGame()
                ,new DoMath()
            }.ToArray();
            // GAMES //



            bool bContinue = true;
            do
            {
                if (currentGame == -1 || games[currentGame] == null)
                {
                    bContinue = GameChooser.Choose(games, ref currentGame, in player, bPlayWelcomeMessage);
                    bPlayWelcomeMessage = false;
                }
                else
                {
                    // each game returns a bool at the end of a cycle, True: run the same game next cycle. False: go to game chooser
                    bool bKeepPlaying = games[currentGame].Run(ref player);
                    if (!bKeepPlaying) currentGame = -1;
                }
            } while (bContinue);

            Environment.Exit(0);
        }
    }
}




