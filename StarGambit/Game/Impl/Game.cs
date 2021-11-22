using System;
using System.Collections.Generic;
using System.Linq;

namespace StarGambit.Game.Impl
{
    internal class Game : IGame
    {
        public GameState GameState { get; }

        public IPlayer GameMaster => GameState.GameMaster;

        public Game(IPlayer gameMaster)
        {
            GameState = new GameState
            {
                GameMaster = gameMaster
            };
        }

        public Game(GameState gameState)
        {
            GameState = gameState;
        }

        public IEnumerable<IPlayer> AddUsers(IEnumerable<IPlayer> users)
        {
            foreach (var user in users)
            {
                if (!GameState.PlayersStates.ContainsKey(user))
                {
                    GameState.PlayersStates[user] = new PlayerState();
                    yield return user;
                }
            }
        }
        /**
         * Distribute n card to user, 
         * if the distribution contain Joker it returns true
         * and rules apply
         */
        public bool Distribute(IPlayer user, int numberCard)
        {
            if (!GameState.PlayersStates.TryGetValue(user, out PlayerState state))
            {
                state = new PlayerState();
                GameState.PlayersStates[user] = state;
            }
            var jokerDrawn = false;
            for (int i = 0; i < numberCard; i++)
            {
                var card = state.Deck.Draw();
                if (card.Color == Card.ColorEnum.Joker)
                {
                    jokerDrawn = state.JokerDrawn = true;
                    state.Discard.Add(card);
                }
                else
                {
                    state.Hand.Add(card);
                }
            }
            return jokerDrawn;
        }

        public IEnumerable<Card> ShowHand(IPlayer user)
        {
            if (!GameState.PlayersStates.ContainsKey(user))
                throw new Exception("User not found");

            return GameState.PlayersStates[user].Hand;
        }

        public void Discard(IPlayer user, IEnumerable<int> cardPosition)
        {
            if (!GameState.PlayersStates.TryGetValue(user, out var playerState))
            {
                throw new Exception("User does not exist");
            }
            if (cardPosition.Any(pos => pos < 0 || pos >= playerState.Hand.Count))
            {
                throw new Exception("One on index is not in range");
            }
            if (!playerState.JokerDrawn)
            {
                throw new Exception("By the rules, you could only discard if a Joker is drawn");
            }
            // Sort in descinding order
            var sortedPosition = cardPosition.OrderBy(a => -a).Distinct();
            foreach (var pos in sortedPosition)
            {
                // PUT THE CARD INTO FUNGUS NETWORK IN ORDER TO AVOID TO BE DRAWN
                playerState.FungusNetwork.Add(playerState.Hand[pos]);
                playerState.Hand.RemoveAt(pos);
            }
        }

        public bool Refill(IPlayer user)
        {
            if (!GameState.PlayersStates.TryGetValue(user, out var playerState))
            {
                throw new Exception("User does not exist");
            }
            if (playerState.Edge <= 0)
            {
                throw new Exception("Edge is not defined");
            }
            var playerHandSize = playerState.Hand.Count();
            if (playerHandSize >= playerState.Edge)
            {
                return false;
            }

            // SHUFFLE DISCARD INTO DECK
            playerState.Deck.Cards.AddRange(playerState.Discard);
            playerState.Deck.Shuffle();
            playerState.Discard.Clear();

            var result = Distribute(user, playerState.Edge - playerHandSize);
            if (!result)
            { // PUT BACK THE FUNGUS NETWORK INTO DISCARD
                playerState.Discard.AddRange(playerState.FungusNetwork);
            }
            return result;
        }

        internal Tuple<int, IEnumerable<Card>> PlayDeck(IPlayer user, Card.ColorEnum color)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PlayerInfo> GeneratePlayersInfos()
        {
            foreach (var pair in GameState.PlayersStates)
            {
                yield return new PlayerInfo
                {
                    User = pair.Key,
                    NumberCardInDeck = pair.Value.Deck.Cards.Count,
                    NumberCardInDiscard = pair.Value.Discard.Count,
                    CardsInHand = pair.Value.Hand,
                };
            }
        }

        public void SetUserEdge(int i, IPlayer player)
        {
            if (!GameState.PlayersStates.TryGetValue(player, out var playerState))
            {
                throw new Exception("User does not exist");
            }
            if (i <= 0 || i > 52)
            {
                throw new Exception("Edge not in range");
            }
            playerState.Edge = i;
        }
    }
}
