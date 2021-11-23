using NUnit.Framework;
using StarGambit.Game;
using StarGambit.Game.Impl;
using StarGambit.Test.Mock;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StarGambitTest
{
    public class GameTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetGameMaster()
        {
            const string gm = "EXPECTED_GAMEMASTER";
            IGame game = new Game(new MockUser(gm));
            Assert.That(game.GameMaster.Name, Is.EqualTo(gm));
        }

        [Test]
        public void TestThatShuffleKeepTheDeckDistinct()
        {
            var deck = new Deck();

            deck.Shuffle();

            var hashSetCard = new HashSet<Card>();
            foreach (var card in deck.Cards)
            {
                Assert.That(hashSetCard.Add(card));
            }
        }

        [Test]
        public void TestDrawCard()
        {
            // PREPARE
            const string gm = "EXPECTED_GAMEMASTER";
            const string player1 = "PLAYER1";
            var game = new Game(new MockUser(gm));
            game.AddUsers(new[] { new MockUser(player1) });

            // ACT
            var jokerDrawn = game.Distribute(new MockUser(player1), 10);
            game.SetUserEdge(new MockUser(player1), 10);
            while (jokerDrawn)
            {
                jokerDrawn = game.Refill(new MockUser(player1));
            }
            // ASSERT
            Assert.That(game.GameState.PlayersStates, Is.Not.Null);
            Assert.That(game.GameState.PlayersStates.ContainsKey(new MockUser(player1)));

            var playerState = game.GameState.PlayersStates[new MockUser(player1)];
            Assert.That(playerState.Hand, Is.Not.Null);
            Assert.That(playerState.Deck, Is.Not.Null);
            Assert.That(playerState.Hand.Count, Is.EqualTo(10));
            Assert.That(playerState.Deck.Cards.Count, Is.EqualTo(44));
        }

        [Test]
        public void TestDistributeJoker()
        {
            // ARRANGE
            const string gm = "EXPECTED_GAMEMASTER";
            const string player1 = "PLAYER1";
            const int player1Edge = 3;

            var gameState = new GameState
            {
                GameMaster = new MockUser(gm)
            };

            var playerADeck = new PlayerState(new Deck(new[] {
                new Card(Card.ValueEnum._2, Card.ColorEnum.Club),
                new Card(Card.ValueEnum.JokerB, Card.ColorEnum.Joker),
                new Card(Card.ValueEnum._3, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._2, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum._3, Card.ColorEnum.Spade),
                new Card(Card.ValueEnum.Jack, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum.Jack, Card.ColorEnum.Spade),
                new Card(Card.ValueEnum.Queen, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum.Queen, Card.ColorEnum.Spade),
                new Card(Card.ValueEnum.JokerA, Card.ColorEnum.Joker)
            }));
            playerADeck.Edge = player1Edge;

            gameState.PlayersStates[new MockUser(player1)] = playerADeck;
            IGame game = new Game(gameState);

            // ACT
            var jokerDrawn = game.Distribute(new MockUser(player1), 1);
            Assert.Throws<Exception>(() => game.Discard(new MockUser(player1), new[] { 0 }));
            var handcase1 = game.ShowHand(new MockUser(player1)).ToArray();
            Assert.That(jokerDrawn, Is.EqualTo(false));
            Assert.That(handcase1.Count(), Is.EqualTo(1));
            Assert.That(handcase1.Contains(new Card(Card.ValueEnum._2, Card.ColorEnum.Club)));
            jokerDrawn = game.Distribute(new MockUser(player1), 1);
            Assert.That(jokerDrawn, Is.EqualTo(true));
            game.Discard(new MockUser(player1), new[] { 0 });
            Assert.Throws<Exception>(() => game.Discard(new MockUser(player1), new[] { 0 }));
            jokerDrawn = game.Refill(new MockUser(player1));
            Assert.That(!handcase1.Contains(new Card(Card.ValueEnum.JokerA, Card.ColorEnum.Joker)));
            do
            {
                jokerDrawn = game.Refill(new MockUser(player1));
            }
            while (jokerDrawn);
            var handcase2 = game.ShowHand(new MockUser(player1)).ToArray();
            Assert.That(!handcase2.Contains(new Card(Card.ValueEnum.JokerA, Card.ColorEnum.Joker)));
            Assert.That(handcase2.Count(), Is.EqualTo(player1Edge));
        }

        [Test]
        public void TestDistributeJokerAndFungusNetwork()
        {
            const string gm = "EXPECTED_GAMEMASTER";
            const string player2 = "PLAYER2";
            const int player2Edge = 1;
            var gameState = new GameState
            {
                GameMaster = new MockUser(gm)
            };

            var playerBDeck = new PlayerState(new Deck(new[] {
                new Card(Card.ValueEnum._2, Card.ColorEnum.Club),
                new Card(Card.ValueEnum.JokerA, Card.ColorEnum.Joker),
                new Card(Card.ValueEnum._3, Card.ColorEnum.Club),
            }));
            playerBDeck.Edge = player2Edge;
            IGame game = new Game(gameState);

            gameState.PlayersStates[new MockUser(player2)] = playerBDeck;
            var jokerDrawn = game.Distribute(new MockUser(player2), 1);
            Assert.That(jokerDrawn, Is.EqualTo(false));
            jokerDrawn = game.Distribute(new MockUser(player2), 1);
            Assert.That(jokerDrawn, Is.EqualTo(true));
            game.Discard(new MockUser(player2), new[] { 0 });
            int nRetry = 20;
            while (game.Refill(new MockUser(player2)) && nRetry-- >= 0) { }
            var handcase2 = game.ShowHand(new MockUser(player2));
            Assert.That(handcase2.Contains(new Card(Card.ValueEnum._3, Card.ColorEnum.Club)), "Test Fungus network");
        }

        [Test]
        public void TestBasicPlayWithoutJoker()
        {
            // ARRANGE
            const string gm = "EXPECTED_GAMEMASTER";
            const string player1 = "PLAYER1";
            var gameState = new GameState
            {
                GameMaster = new MockUser(gm)
            };

            var playerADeck = new PlayerState(new Deck(new[] {
                new Card(Card.ValueEnum._2, Card.ColorEnum.Club),
                new Card(Card.ValueEnum.Jack, Card.ColorEnum.Club),
                new Card(Card.ValueEnum._2, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum.Queen, Card.ColorEnum.Spade),
                new Card(Card.ValueEnum.King, Card.ColorEnum.Spade),
                new Card(Card.ValueEnum._8, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._3, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum._7, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._6, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._2, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._10, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum.King, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._6, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum._1, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum.Queen, Card.ColorEnum.Club),
                new Card(Card.ValueEnum.Jack, Card.ColorEnum.Diamond),
            }));
            var nbCard = playerADeck.Deck.Cards.Count;
            gameState.PlayersStates[new MockUser(player1)] = playerADeck;
            IGame game = new Game(gameState);

            // ACT
            var val = game.PlayDeck(new MockUser(player1), Card.ColorEnum.Diamond);
            Assert.That(val.Item1, Is.EqualTo(2));
            var playerState = gameState.PlayersStates[new MockUser(player1)];
            Assert.That(playerState.Discard.Count, Is.EqualTo(1));
            Assert.That(playerState.Deck.Cards.Count, Is.EqualTo(--nbCard));

            val = game.PlayDeck(new MockUser(player1), Card.ColorEnum.Diamond);
            Assert.That(val.Item1, Is.EqualTo(3));

            val = game.PlayDeck(new MockUser(player1), Card.ColorEnum.Diamond);
            Assert.That(val.Item1, Is.EqualTo(5), "Only one figure reroll");

            val = game.PlayDeck(new MockUser(player1), Card.ColorEnum.Diamond);
            Assert.That(val.Item1, Is.EqualTo(8), "Should take the max of 8 diamond and 3 heart");

            val = game.PlayDeck(new MockUser(player1), Card.ColorEnum.Diamond);
            Assert.That(val.Item1, Is.EqualTo(13), "Sum");

            val = game.PlayDeck(new MockUser(player1), Card.ColorEnum.Diamond);
            Assert.That(val.Item1, Is.EqualTo(10), "Max");

            val = game.PlayDeck(new MockUser(player1), Card.ColorEnum.Diamond);
            Assert.That(val.Item1, Is.EqualTo(10), "King + 6 + 1");

            val = game.PlayDeck(new MockUser(player1), Card.ColorEnum.Diamond);
            Assert.That(val.Item1, Is.EqualTo(3), "Queen + Jack");
        }

        [Test]
        public void TestBasicPlayWithoker()
        {
            // ARRANGE
            const string gm = "EXPECTED_GAMEMASTER";
            const string player1 = "PLAYER1";
            const string player2 = "PLAYER2";
            const string player3 = "PLAYER3";
            var gameState = new GameState
            {
                GameMaster = new MockUser(gm)
            };

            var playerADeck = new PlayerState(new Deck(new[] {
                new Card(Card.ValueEnum._2, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum.JokerA, Card.ColorEnum.Joker),
                new Card(Card.ValueEnum.Jack, Card.ColorEnum.Club),
                new Card(Card.ValueEnum.Queen, Card.ColorEnum.Spade),
                new Card(Card.ValueEnum.King, Card.ColorEnum.Spade),
                new Card(Card.ValueEnum._8, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._3, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum._7, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._6, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._2, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._10, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum.King, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._6, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum._1, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum.Queen, Card.ColorEnum.Club),
                new Card(Card.ValueEnum.Jack, Card.ColorEnum.Diamond),
            }));
            playerADeck.Edge = 5;

            var playerBDeck = new PlayerState(new Deck(new[] {
                new Card(Card.ValueEnum._2, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum.JokerA, Card.ColorEnum.Joker),
                new Card(Card.ValueEnum.Jack, Card.ColorEnum.Diamond),
            }));
            playerBDeck.Edge = 1;

            var playerCDeck = new PlayerState(new Deck(new[] {
                new Card(Card.ValueEnum.Jack, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum.JokerA, Card.ColorEnum.Joker),
                new Card(Card.ValueEnum._2, Card.ColorEnum.Heart),
            }));
            playerCDeck.Edge = 1;

            var nbCard = playerADeck.Deck.Cards.Count;
            gameState.PlayersStates[new MockUser(player1)] = playerADeck;
            gameState.PlayersStates[new MockUser(player2)] = playerBDeck;
            gameState.PlayersStates[new MockUser(player3)] = playerCDeck;
            IGame game = new Game(gameState);

            // ACT
            var jokerDrawn = game.Distribute(new MockUser(player1), 1);
            Assert.Throws<Exception>(() => game.Refill(new MockUser(player1)));
            var val = game.PlayDeck(new MockUser(player1), Card.ColorEnum.Heart);
            Assert.That(val.Item1, Is.EqualTo(-1), "Critical FAILURE");
            game.Discard(new MockUser(player1), new[] { 0 });
            game.Refill(new MockUser(player1));
            val = game.PlayDeck(new MockUser(player2), Card.ColorEnum.Heart);
            Assert.That(val.Item1, Is.EqualTo(-1), "Critical FAILURE");
            val = game.PlayDeck(new MockUser(player3), Card.ColorEnum.Diamond);
            Assert.That(val.Item1, Is.EqualTo(-1), "Critical FAILURE");
        }

        [Test]
        public void TestPlayDeckWithDiscard()
        {// ARRANGE
            const string gm = "EXPECTED_GAMEMASTER";
            const string player1 = "PLAYER1";
            var gameState = new GameState
            {
                GameMaster = new MockUser(gm)
            };

            var playerADeck = new PlayerState(
                new Deck(
                    new[] {
                        new Card(Card.ValueEnum._7, Card.ColorEnum.Heart),
                        new Card(Card.ValueEnum._6, Card.ColorEnum.Diamond),
                        new Card(Card.ValueEnum._2, Card.ColorEnum.Heart),
                        new Card(Card.ValueEnum._10, Card.ColorEnum.Heart),
                    }),
                new[]{
                    new Card(Card.ValueEnum._2, Card.ColorEnum.Club),
                    new Card(Card.ValueEnum.Jack, Card.ColorEnum.Club),
                    new Card(Card.ValueEnum.Queen, Card.ColorEnum.Spade),
                    new Card(Card.ValueEnum._8, Card.ColorEnum.Diamond),
                    new Card(Card.ValueEnum._7, Card.ColorEnum.Diamond),
                    new Card(Card.ValueEnum._2, Card.ColorEnum.Diamond),
                    new Card(Card.ValueEnum._1, Card.ColorEnum.Diamond),
                    new Card(Card.ValueEnum.Queen, Card.ColorEnum.Club),
                });
            var nbCardInHand = playerADeck.Hand.Count;
            gameState.PlayersStates[new MockUser(player1)] = playerADeck;
            var game = new Game(gameState);
            var val = game.PlayDeckWithDiscard(new MockUser(player1), Card.ColorEnum.Diamond, 0);
            Assert.That(val.Item1, Is.EqualTo(13), "Sum");
            Assert.That(gameState.PlayersStates[new MockUser(player1)].Hand.Count, Is.EqualTo(--nbCardInHand));
            val = game.PlayDeckWithDiscard(new MockUser(player1), Card.ColorEnum.Diamond, 0);
            Assert.That(val.Item1, Is.EqualTo(10), "Max");
            Assert.That(gameState.PlayersStates[new MockUser(player1)].Hand.Count, Is.EqualTo(--nbCardInHand));
        }

        [Test]
        public void TestHandPlayWithoutJoker()
        {
            // ARRANGE
            const string gm = "EXPECTED_GAMEMASTER";
            const string player1 = "PLAYER1";
            var gameState = new GameState
            {
                GameMaster = new MockUser(gm)
            };

            var playerADeck = new PlayerState(new Deck(new[] {
                new Card(Card.ValueEnum._2, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum.King, Card.ColorEnum.Spade),
                new Card(Card.ValueEnum._3, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum._6, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._10, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum.King, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._6, Card.ColorEnum.Heart),
                new Card(Card.ValueEnum.Jack, Card.ColorEnum.Diamond),
            }),
            new[]{
                new Card(Card.ValueEnum._2, Card.ColorEnum.Club),
                new Card(Card.ValueEnum.Jack, Card.ColorEnum.Club),
                new Card(Card.ValueEnum.Queen, Card.ColorEnum.Spade),
                new Card(Card.ValueEnum._8, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._7, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._2, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum._1, Card.ColorEnum.Diamond),
                new Card(Card.ValueEnum.Queen, Card.ColorEnum.Club),
            });
            var nbCard = playerADeck.Deck.Cards.Count;
            gameState.PlayersStates[new MockUser(player1)] = playerADeck;
            IGame game = new Game(gameState);

            // ACT
            var val = game.PlayHand(new MockUser(player1), Card.ColorEnum.Diamond, 0);
            Assert.That(val.Item1, Is.EqualTo(2));
            var playerState = gameState.PlayersStates[new MockUser(player1)];
            Assert.That(playerState.Discard.Count, Is.EqualTo(1));
            Assert.That(playerState.Deck.Cards.Count, Is.EqualTo(nbCard));

            val = game.PlayHand(new MockUser(player1), Card.ColorEnum.Diamond, 0);
            Assert.That(val.Item1, Is.EqualTo(3));

            val = game.PlayHand(new MockUser(player1), Card.ColorEnum.Diamond, 0);
            Assert.That(val.Item1, Is.EqualTo(5), "Only one figure reroll");

            val = game.PlayHand(new MockUser(player1), Card.ColorEnum.Diamond, 0);
            Assert.That(val.Item1, Is.EqualTo(8), "Should take the max of 8 diamond and 3 heart");

            val = game.PlayHand(new MockUser(player1), Card.ColorEnum.Diamond, 0);
            Assert.That(val.Item1, Is.EqualTo(13), "Sum");

            val = game.PlayHand(new MockUser(player1), Card.ColorEnum.Diamond, 0);
            Assert.That(val.Item1, Is.EqualTo(10), "Max");

            val = game.PlayHand(new MockUser(player1), Card.ColorEnum.Diamond, 0);
            Assert.That(val.Item1, Is.EqualTo(10), "King + 6 + 1");

            val = game.PlayHand(new MockUser(player1), Card.ColorEnum.Diamond, 0);
            Assert.That(val.Item1, Is.EqualTo(3), "Queen + Jack");
        }
    }
}