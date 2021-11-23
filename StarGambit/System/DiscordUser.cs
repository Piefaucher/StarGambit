using Discord.WebSocket;
using StarGambit.Game;
using System;

namespace StarGambit.System
{
    internal class DiscordUser : IPlayer
    {
        private readonly ulong Identifier;

        public DiscordUser(SocketUser socketUser)
        {
            Identifier = socketUser.Id;
            Name = socketUser.Mention;
        }

        public string Name { get; }
                
        private bool Equals(DiscordUser other)
        {
            return other.Identifier.Equals(Identifier);
        }

        public override bool Equals(object other)
        {
            if (other == null) return this==null;
            if (ReferenceEquals(other, this)) return true;
            var otherDiscordUser = other as DiscordUser;
            if (otherDiscordUser == null) return false;
            return Equals(otherDiscordUser);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Identifier);
        }
    }
}
