using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames
{
    public abstract class aCard
    {
        public enum Type { Number, Face, Joker }

        public enum Suit { Heart, Spade, Club, Diamond }

        public Suit GetSuit => suit;
        public Type GetCardType=> type;

        protected Suit suit;
        protected Type type;
    }
}
