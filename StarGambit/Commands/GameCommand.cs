using Discord;
using Discord.Commands;
using StarGambit.Game;
using StarGambit.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarGambit.Commands
{
    public class GameCommand : ModuleBase<SocketCommandContext>
    {
        public GameCommand(IGameProvider gameProvider)
        {
            GameProvider = gameProvider;
        }

        public IGameProvider GameProvider { get; }
        public IServiceProvider Services { get; }

        [Command("init")]
        public async Task GameStart()
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            await ReplyAsync($"La partie va commencer avec le maître du jeu : {game.GameMaster.Name}").ConfigureAwait(false);
        }

        [Command("infos")]
        public async Task GameInfo()
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            var gameMaster = game.GameMaster;
            var gameInfo = game.GeneratePlayersInfos();
            await ReplyAsync($"Information de la partie :\r\n{CreateGameInformations(gameMaster, gameInfo)}").ConfigureAwait(false);
        }

        [Command("add", false)]
        public async Task GameAdd([Remainder] string _ = null)
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            var mentionnedUsers = Context.Message.MentionedUsers.Select(su => new DiscordUser(su));
            var addedPlayers = game.AddUsers(mentionnedUsers);
            await ReplyAsync($"Joueurs ajoutés : {string.Join(", ", addedPlayers.Select(p => p.Name))}").ConfigureAwait(false);
        }

        [Command("set edge")]
        public async Task GameAdd(string _, int edge)
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            var user = Context.Message.MentionedUsers.First();
            var player = new DiscordUser(user);
            game.SetEdge(player, edge);
            await ReplyAsync($"Le joueur {player.Name} a désormais un edge de {edge}.").ConfigureAwait(false);
            await Refill(game, player, true).ConfigureAwait(false);
        }

        [Command("draw")]
        public async Task Draw(int nbCard, string _)
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            var user = Context.Message.MentionedUsers.Select(su => new DiscordUser(su)).Single();
            var jokerdrawn = game.Distribute(user, nbCard);
            string message = "";
            if (game.Distribute(user, nbCard))
            {
                message = "Un joker a été tiré, vous pouvez retirez des cartes de votre main (commande !discard) et la remplir à nouveau (commande !refill)\r\n";
            }
            message += ShowHand(game, user);
            await ReplyAsync(message).ConfigureAwait(false);
        }

        [Command("discard")]
        public async Task Discard(int pos)
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            var player = new DiscordUser(Context.User);
            var cardsDiscarded = game.Discard(player, new[] { pos });
            var message = $"Vous avez défaussé les cartes suivantes : {string.Join(", ", cardsDiscarded.Select(c => c.ToString()).ToArray())}\r\n";
            message += ShowHand(game, player);
            await ReplyAsync().ConfigureAwait(false);
        }

        [Command("refill")]
        public async Task Refill()
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            DiscordUser player = new DiscordUser(Context.User);
            await Refill(game, player).ConfigureAwait(false);
        }

        [Command("play hand", false)]
        public async Task PlayHand(string color, int pos)
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            var user = new DiscordUser(Context.User);
            var playResult = game.PlayHand(user, ParseColor(color), pos);
            await ReplyAsync(ShowRollResult(game, user, playResult)).ConfigureAwait(false);
        }

        [Command("play discard", false)]
        public async Task PlayDiscard(string color, int pos)
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            var user = new DiscordUser(Context.User);
            var playResult = game.PlayDeckWithDiscard(user, ParseColor(color), pos);
            await ReplyAsync(ShowRollResult(game, user, playResult)).ConfigureAwait(false);
        }

        [Command("play deck", false)]
        public async Task PlayDeck(string color)
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            var user = new DiscordUser(Context.User);
            var playResut = game.PlayDeck(user, ParseColor(color));
            string message = ShowRollResult(game, user, playResut);
            await ReplyAsync(message).ConfigureAwait(false);
        }

        private async Task Refill(IGame game, DiscordUser player, bool force = false)
        {
            string message = "";
            if (game.Refill(player, force))
            {
                message = "Un joker a été tiré, vous pouvez retirez des cartes de votre main (commande !discard) et la remplir à nouveau (commande !refill)\r\n";
            }
            message += ShowHand(game, player);
            await ReplyAsync(message).ConfigureAwait(false);
        }

        private static string ShowRollResult(IGame game, DiscordUser user, Tuple<int, IEnumerable<Card>> playResut)
        {
            string message;
            if (playResut.Item1 == -1)
            {
                message = "FUMMMMBLE mais il faut voir le bon côté des choses\r\n";
                message += "Un joker a été tiré, vous pouvez retirez des cartes de votre main (commande !discard) et la remplir à nouveau (commande !refill)\r\n";
            }
            else
            {
                message = $"Résultat du jet {playResut.Item1}\r\n";
            }
            message += $"Les cartes suivantes ont tirés pour ce jet : {string.Join(", ", playResut.Item2.Select(c => c.ToString()))}\r\n";
            message += ShowHand(game, user);
            return message;
        }

        private static string ShowHand(IGame game, IPlayer user)
        {
            string message = $"Main de {user.Name}\r\n";
            message += CreateHandInfo(game.ShowHand(user));
            return message;
        }

        private static string CreateGameInformations(IPlayer gameMaster, IEnumerable<PlayerInfo> playerInformations)
        {
            var message = $"Maître du jeu : {gameMaster.Name}\r\n";
            message += string.Join("\r\n", playerInformations.Select(CreatePlayerInfo));
            return message;
        }

        private static string CreatePlayerInfo(PlayerInfo playerInfo)
        {
            var message = $"Joueur {playerInfo.User.Name}, Taille de deck : {playerInfo.NumberCardInDeck}, Défausse : {playerInfo.NumberCardInDiscard}\r\n";
            message += CreateHandInfo(playerInfo.CardsInHand);
            return message;
        }

        private static string CreateHandInfo(IEnumerable<Card> cards)
        {
            var count = 0;
            return string.Join("\r\n", cards.Select(c => $"{count++} => {c}"));
        }

        private static Card.ColorEnum ParseColor(string color)
        {
            if (JaroWinklerDistance.proximity(color, "carreau") > 0.8)
            {
                return Card.ColorEnum.Diamond;
            }
            if (JaroWinklerDistance.proximity(color, "pique") > 0.8)
            {
                return Card.ColorEnum.Spade;
            }
            if (JaroWinklerDistance.proximity(color, "trèfle") > 0.8)
            {
                return Card.ColorEnum.Club;
            }
            if (JaroWinklerDistance.proximity(color, "coeur") > 0.8)
            {
                return Card.ColorEnum.Heart;
            }
            throw new Exception("Sorry I couldn't understand the color :'(");
        }
    }
}
