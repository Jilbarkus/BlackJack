using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames
{
    public class FaceCard : aCard
    {
        public enum Face { King, Queen, Jack };
        public Face GetFace => face;

        protected Face face;

        public FaceCard(Suit suit, Face face)
        {
            base.suit = suit;
            base.type = Type.Face;
            this.face = face;
        }

        public FaceCard()
        {
            base.type = Type.Face;
            Array suits = Enum.GetValues(typeof(Suit));
            Random random = new Random();
            base.suit = (Suit)suits.GetValue(random.Next(suits.Length));
            Array faces = Enum.GetValues(typeof(Face));
            this.face = (Face)faces.GetValue(random.Next(faces.Length));
        }

        //public int GetBJValue() => 10;
        //protected int BJValue;
        //public BJCard(Suit suit, Type type, int BJValueIn) : base(suit, type)
        //{
        //    BJValue = BJValueIn;
        //}
        //public FaceCard(Suit suit, Type type = Type.Face, Face faceIn)/* : base(suit, type = Type.Face)*/
        //{
        //    base(suit, type);
        //    face = faceIn;
        //}
    }

    public class NumberCard : aCard
    {
        public int GetNumber => number;
        protected int number = -1;

        public NumberCard(Suit suit, int number)
        {
            base.suit = suit;
            base.type = Type.Number;
            this.number = number;
        }

        public NumberCard()
        {
            base.type = Type.Number;
            Array suits = Enum.GetValues(typeof(Suit));
            Random random = new Random();
            base.suit = (Suit)suits.GetValue(random.Next(suits.Length));
            this.number = random.Next(1,11);
        }
    }
}
