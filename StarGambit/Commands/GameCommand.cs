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

        [Command("game start")]
        public async Task GameStart()
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            await ReplyAsync($"La partie va commencer avec le maître du jeu : {game.GameMaster.Name}").ConfigureAwait(false);
        }

        [Command("game infos")]
        public async Task GameInfo()
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            var gameMaster = game.GameMaster;
            var gameInfo = game.GeneratePlayersInfos();
            await ReplyAsync($"Information de la partie :\r\n{CreateGameInformations(gameMaster, gameInfo)}").ConfigureAwait(false);
        }

        [Command("game add", false)]
        public async Task GameAdd([Remainder] string _ = null)
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            var mentionnedUsers = Context.Message.MentionedUsers.Select(su => new DiscordUser(su));
            var addedPlayers = game.AddUsers(mentionnedUsers);
            await ReplyAsync($"Joueurs ajoutés : {string.Join(", ",addedPlayers.Select(p=>p.Name))}").ConfigureAwait(false);
        }

        [Command("game distribute", false)]
        public async Task GameAdd(int nbCard, [Remainder] string _ = null)
        {
            var game = GameProvider.Provide(Context.Channel.Id, new DiscordUser(Context.User));
            var mentionnedUsers = Context.Message.MentionedUsers.Select(su => new DiscordUser(su));
            foreach (var user in mentionnedUsers)
            {
                game.Distribute(user, nbCard);
                await ReplyAsync(ShowHand(game, user)).ConfigureAwait(false);

            }
        }

        private static string ShowHand(IGame game, Game.IPlayer user)
        {
            string message = $"Main de {user.Name}\r\n";
            message += CreateHandInfo(game.ShowHand(user));
            return message;
        }

        private static string CreateGameInformations(Game.IPlayer gameMaster, IEnumerable<PlayerInfo> playerInformations)
        {
            return $"Maître du jeu : {gameMaster.Name}\r\n"
                + string.Join("\r\n", playerInformations.Select(CreatePlayerInfo));
        }

        private static string CreatePlayerInfo(PlayerInfo playerInfo)
        {
            return $"Joueur {playerInfo.User.Name}, Taille de deck : {playerInfo.NumberCardInDeck}, Défausse : {playerInfo.NumberCardInDiscard}\r\n"
                + CreateHandInfo(playerInfo.CardsInHand);
        }

        private static string CreateHandInfo(IEnumerable<Card> cards)
        {
            var count = 0;
            return string.Join("\r\n", cards.Select(c => $"{count++} => {c}"));
        }
    }
}
