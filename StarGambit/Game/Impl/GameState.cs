using System.Collections.Generic;
using System.Linq;

namespace StarGambit.Game.Impl
{
    internal class GameState
    {
        public IPlayer GameMaster { get; set; }
        public IDictionary<IPlayer, PlayerState> PlayersStates { get; } = new Dictionary<IPlayer, PlayerState>();

    }

    internal class PlayerState
    {
        internal int Edge { get; set; }
        internal bool JokerDrawn { get; set; }
        internal List<Card> Hand { get; }
        internal List<Card> Discard { get; }
        internal List<Card> FungusNetwork { get; }
        internal Deck Deck { get; }
        internal string Info => $"Deck {Deck.Cards.Count} cartes, Défausses {Discard.Count} cartes\r\nMain = {string.Join(", ", Hand.Select(c => c.ToString()))}";

        internal PlayerState(Deck deck)
        {
            Hand = new List<Card>();
            Discard = new List<Card>();
            FungusNetwork = new List<Card>();
            Deck = deck;
        }

        internal PlayerState(Deck deck, IEnumerable<Card> handCards)
        {
            Hand = handCards.ToList();
            Discard = new List<Card>();
            FungusNetwork = new List<Card>();
            Deck = deck;
        }

        public PlayerState()
        {
            JokerDrawn = false;
            Hand = new List<Card>();
            Discard = new List<Card>();
            FungusNetwork = new List<Card>();
            Deck = new Deck().Shuffle();
        }
    }
}
