using System;
using System.Collections.Generic;
using System.Linq;

namespace StarGambit.Game.Impl
{
    internal class Deck
    {
        public List<Card> Cards { get; }

        private readonly Random rng;

        internal Deck(IEnumerable<Card> cards)
        {
            rng = new Random(unchecked(Environment.TickCount));
            Cards = cards.ToList();
        }

        public Deck()
        {
            rng = new Random(unchecked(Environment.TickCount));
            Cards = new List<Card>();
            for (int color=0; color<4; color++)
            {
                for (int value=0; value<13; value++)
                {
                    Cards.Add(new Card((Card.ValueEnum)value, (Card.ColorEnum)color));
                }
            }
            Cards.Add(new Card(Card.ValueEnum.JokerA, Card.ColorEnum.Joker));
            Cards.Add(new Card(Card.ValueEnum.JokerB, Card.ColorEnum.Joker));
        }

        public Card Draw()
        {
            var card = Cards[0];
            Cards.RemoveAt(0);
            return card;
        }

        public Deck Shuffle()
        {
            int n = Cards.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = Cards[k];
                Cards[k] = Cards[n];
                Cards[n] = value;
            }
            return this;
        }
    }
}
