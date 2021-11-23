using System;
using System.Collections.Generic;

namespace StarGambit.Game
{
    public interface IGame
    {
        IPlayer GameMaster { get; }
        IEnumerable<PlayerInfo> GeneratePlayersInfos();

        IEnumerable<IPlayer> AddUsers(IEnumerable<IPlayer> users);
        void SetEdge(IPlayer player, int edge);
        bool Distribute(IPlayer player, int numberCard);

        IEnumerable<Card> Discard(IPlayer player, IEnumerable<int> cardPosition);

        bool Refill(IPlayer player, bool force = false);

        IEnumerable<Card> ShowHand(IPlayer player);

        Tuple<int, IEnumerable<Card>> PlayDeck(IPlayer player, Card.ColorEnum color);
        Tuple<int, IEnumerable<Card>> PlayHand(IPlayer player, Card.ColorEnum color, int pos);
        Tuple<int, IEnumerable<Card>> PlayDeckWithDiscard(IPlayer player, Card.ColorEnum color, int pos);
    }

    public class PlayerInfo
    {
        public IPlayer User { get; set; }
        public int NumberCardInDiscard { get; set; }
        public int NumberCardInDeck { get; set; }
        public IEnumerable<Card> CardsInHand { get; set; }
    }
}
