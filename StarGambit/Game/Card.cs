using System;

namespace StarGambit.Game
{
    public sealed class Card
    {
        public enum ValueEnum
        {
            _1,
            _2,
            _3,
            _4,
            _5,
            _6,
            _7,
            _8,
            _9,
            _10,
            Jack,
            Queen,
            King,
            JokerA,
            JokerB,
        }

        public enum ColorEnum
        {
            Heart,
            Spade,
            Club,
            Diamond,
            Joker,
        }

        private bool Equals(Card other)
        {
            return other.Color.Equals(Color) && other.Value.Equals(Value);
        }

        public override bool Equals(object other)
        {
            if (other == null) return this == null;
            if (ReferenceEquals(other, this)) return true;
            if (!(other is Card otherCard)) return false;
            return Equals(otherCard);
        }

        internal ValueEnum Value { get; }
        internal ColorEnum Color { get; }

        public override string ToString()
        {
            return GetValueName() + (Color != ColorEnum.Joker ? $" de {GetColorName()}" : "");
        }

        public string GetValueName()
        {
            switch (Value)
            {
                case ValueEnum._1: return "As";
                case ValueEnum._2: return "Deux";
                case ValueEnum._3: return "Trois";
                case ValueEnum._4: return "Quatre";
                case ValueEnum._5: return "Cinq";
                case ValueEnum._6: return "Six";
                case ValueEnum._7: return "Sept";
                case ValueEnum._8: return "Huit";
                case ValueEnum._9: return "Neuf";
                case ValueEnum._10: return "Dix";
                case ValueEnum.Jack: return "Valet";
                case ValueEnum.Queen: return "Dame";
                case ValueEnum.King: return "Roi";
                case ValueEnum.JokerA:
                case ValueEnum.JokerB:
                    return "Joker";
                default: throw new NotImplementedException(Enum.GetName(typeof(ValueEnum), Value));
            }
        }

        public string GetColorName()
        {
            switch (Color)
            {
                case ColorEnum.Heart: return "Coeur";
                case ColorEnum.Diamond: return "Carreau";
                case ColorEnum.Club: return "Trèfle";
                case ColorEnum.Spade: return "Pique";
                case ColorEnum.Joker: return "";
                default: throw new NotImplementedException(Enum.GetName(typeof(ColorEnum), Color));
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Color);
        }

        public Card(ValueEnum value, ColorEnum color)
        {
            Value = value;
            Color = color;
        }
    }
}
