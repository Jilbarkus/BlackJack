using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames
{
    internal class GameCards
    {
        public static aCard[] BJDeck()
        {
            aCard[] cards = new aCard[52];
            int counter = 0;
            aCard.Suit suit;
            for (int i = 0; i < 4; i++)
            {
                suit = (aCard.Suit)i;
                cards[counter] = new FaceCard(suit, FaceCard.Face.King);
                counter++;
                cards[counter] = new FaceCard(suit, FaceCard.Face.Queen);
                counter++;
                cards[counter] = new FaceCard(suit, FaceCard.Face.Jack);
                counter++;
                for (int j = 1; j < 11; j++)
                {
                    cards[counter] = new NumberCard(suit, j);
                    counter++;
                }
            }
            return cards;
        }
    }
}
