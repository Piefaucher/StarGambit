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
    }

    public class PlayerInfo
    {
        public IPlayer User { get; set; }
        public int NumberCardInDiscard { get; set; }
        public int NumberCardInDeck { get; set; }
        public IEnumerable<Card> CardsInHand { get; set; }
    }
}
