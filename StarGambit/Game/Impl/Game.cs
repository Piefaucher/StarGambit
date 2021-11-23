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
            for (int i = 0; i < Math.Min(numberCard, state.Deck.Cards.Count()); i++)
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
                throw new Exception("Player does not exist");

            return GameState.PlayersStates[user].Hand;
        }

        public IEnumerable<Card> Discard(IPlayer user, IEnumerable<int> cardPosition)
        {
            if (!GameState.PlayersStates.TryGetValue(user, out var playerState))
            {
                throw new Exception("Player does not exist");
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
                var card = playerState.Hand[pos];
                playerState.FungusNetwork.Add(card);
                playerState.Hand.RemoveAt(pos);
                yield return card;
            }
        }

        public bool Refill(IPlayer user, bool force = false)
        {
            if (!GameState.PlayersStates.TryGetValue(user, out var playerState))
            {
                throw new Exception("Player does not exist");
            }
            if (playerState.Edge <= 0)
            {
                throw new Exception("Edge is not defined");
            }
            if (!playerState.JokerDrawn && !force)
            {
                throw new Exception("You need to draw a Joker to refill your hand");
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

        public Tuple<int, IEnumerable<Card>> PlayDeck(IPlayer player, Card.ColorEnum color)
        {
            if (!GameState.PlayersStates.TryGetValue(player, out var playerState))
            {
                throw new Exception("Player does not exist");
            }
            var card = playerState.Deck.Draw();
            playerState.Discard.Add(card);
            return PlayCard(playerState, color, card);
        }

        public Tuple<int, IEnumerable<Card>> PlayHand(IPlayer player, Card.ColorEnum color, int pos)
        {
            if (!GameState.PlayersStates.TryGetValue(player, out var playerState))
            {
                throw new Exception("User does not exist");
            }
            if (pos < 0 || pos >= playerState.Hand.Count)
            {
                throw new Exception("CardOutRange");
            }
            var card = playerState.Hand[pos];
            playerState.Discard.Add(card);
            playerState.Hand.RemoveAt(pos);
            return PlayCard(playerState, color, card);
        }

        public Tuple<int, IEnumerable<Card>> PlayDeckWithDiscard(IPlayer player, Card.ColorEnum color, int pos)
        {
            if (!GameState.PlayersStates.TryGetValue(player, out var playerState))
            {
                throw new Exception("Player does not exist");
            }
            if (pos < 0 || pos >= playerState.Hand.Count)
            {
                throw new Exception("CardOutRange");
            }
            // DISCARD THE SELECTED CARD
            var discardedCard = playerState.Hand[pos];
            playerState.Discard.Add(discardedCard);
            playerState.Hand.RemoveAt(pos);
            // PLAY FROM DECK AND FORCE COLOR
            var card = playerState.Deck.Draw();
            playerState.Discard.Add(card);
            return PlayCard(playerState, color, card, false, true);
        }

        private Tuple<int, IEnumerable<Card>> PlayCard(
            PlayerState state,
            Card.ColorEnum color,
            Card card,
            bool previousGoodColor = false,
            bool forceGoodColor = false)
        {
            var playedCard = new List<Card> { card };
            if (card.Color == Card.ColorEnum.Joker)
            {
                state.JokerDrawn = true;
                return Tuple.Create<int, IEnumerable<Card>>(-1, playedCard);
            }
            var goodColor = card.Color == color || forceGoodColor;
            var score = ComputeScore(card);
            if (card.Value == Card.ValueEnum.Jack
                || card.Value == Card.ValueEnum.Queen
                || card.Value == Card.ValueEnum.King)
            {
                var newCard = state.Deck.Draw();
                state.Discard.Add(card);
                playedCard.Add(newCard);

                if (newCard.Color == Card.ColorEnum.Joker)
                {
                    state.JokerDrawn = true;
                    return Tuple.Create<int, IEnumerable<Card>>(-1, playedCard);
                }

                score += ComputeScore(newCard);
            }
            Tuple<int, IEnumerable<Card>> result;
            if (goodColor && !previousGoodColor)
            {
                var newCard = state.Deck.Draw();
                state.Discard.Add(card);
                playedCard.Add(newCard);

                // MAKE SECOND DRAW 
                var secondDraw = PlayCard(state, color, newCard, true);
                playedCard = playedCard.Union(secondDraw.Item2).ToList();
                if (secondDraw.Item1 == -1)
                {// CRITICAL FAILURE
                    result = Tuple.Create(-1, playedCard.Union(secondDraw.Item2));
                }
                else if (newCard.Color == color)
                {
                    result = Tuple.Create<int, IEnumerable<Card>>(secondDraw.Item1 + score, playedCard);
                }
                else
                {
                    result = Tuple.Create<int, IEnumerable<Card>>(Math.Max(secondDraw.Item1, score), playedCard);
                }
            }
            else
            {
                result = Tuple.Create<int, IEnumerable<Card>>(score, playedCard.ToList());
            }
            return result;
        }

        private static int ComputeScore(Card card)
        {
            switch (card.Value)
            {
                case Card.ValueEnum._1: return 1;
                case Card.ValueEnum._2: return 2;
                case Card.ValueEnum._3: return 3;
                case Card.ValueEnum._4: return 4;
                case Card.ValueEnum._5: return 5;
                case Card.ValueEnum._6: return 6;
                case Card.ValueEnum._7: return 7;
                case Card.ValueEnum._8: return 8;
                case Card.ValueEnum._9: return 9;
                case Card.ValueEnum._10: return 10;
                case Card.ValueEnum.Jack: return 1;
                case Card.ValueEnum.Queen: return 2;
                case Card.ValueEnum.King: return 3;
                default:
                    throw new NotImplementedException(Enum.GetName(typeof(Card.ValueEnum), card.Value));

            }
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

        public void SetUserEdge(IPlayer player, int i)
        {
            if (!GameState.PlayersStates.TryGetValue(player, out var playerState))
            {
                throw new Exception("Player does not exist");
            }
            if (i <= 0 || i > 52)
            {
                throw new Exception("Edge not in range");
            }
            playerState.Edge = i;
        }

        public void SetEdge(IPlayer player, int edge)
        {
            if (!GameState.PlayersStates.TryGetValue(player, out var playerState))
            {
                throw new Exception("Player does not exist");
            }
            if (edge<=0 || edge > 50)
            {
                throw new Exception("Edge not in range");
            }
            playerState.Edge = edge;
        }
    }
}
