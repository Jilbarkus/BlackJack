﻿namespace CardGames
{
    public enum GameTypes { GameChooser, BlackJack };
    public static class Program
    {
        public static void Main()
        {
            aCard[] deck = GameCards.BJDeck();
            PlayerSave player = new PlayerSave(500.0m, Environment.UserName.ToString());
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            int currentGame = -1;
            // GAMES // 
            IGame[] games = new List<IGame>
            {
                new BlackJack()
            }.ToArray();
            // GAMES //



            bool bContinue = true;
            do
            {
                if (currentGame == -1 || games[currentGame] == null)
                {
                    bContinue = GameChooser.Choose(games, ref currentGame, in player);
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



