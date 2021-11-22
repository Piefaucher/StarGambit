using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StarGambit
{
    public class TestCommands : ModuleBase<SocketCommandContext>
    {
        [Command("test ping")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong").ConfigureAwait(false);
            return;
        }

        [Command("test avatar")]
        public async Task AvatarAsync(ushort size = 512)
        {
            await ReplyAsync(CDN.GetUserAvatarUrl(Context.User.Id, Context.User.AvatarId, size, ImageFormat.Auto)).ConfigureAwait(false);
        }

        [Command("test react")]
        public async Task ReactAsync(string pMessage, string pEmoji)
        {
            var message = await Context.Channel.SendMessageAsync(pMessage);
            var emoji = new Emoji(pEmoji);
            await message.AddReactionAsync(emoji);
        } 
    }
}
