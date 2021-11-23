using System;
using System.Collections.Generic;

namespace StarGambit.Game
{
    public interface IGame
    {
        IPlayer GameMaster { get; }
        IEnumerable<PlayerInfo> GeneratePlayersInfos();

        IEnumerable<IPlayer> AddUsers(IEnumerable<IPlayer> users);

        bool Distribute(IPlayer user, int numberCard);

        void Discard(IPlayer user, IEnumerable<int> cardPosition);

        bool Refill(IPlayer user);

        IEnumerable<Card> ShowHand(IPlayer user);

        Tuple<int, IEnumerable<Card>> PlayDeck(IPlayer user, Card.ColorEnum color);
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
