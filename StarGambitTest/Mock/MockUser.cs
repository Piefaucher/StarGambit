using StarGambit.Game;
using System;

namespace StarGambit.Test.Mock
{
    class MockUser : IPlayer
    {
        public string Name { get; }

        public MockUser(string name)
        {
            Name = name;
        }

        private bool Equals(MockUser other)
        {
            return other.Name.Equals(Name);
        }

        public override bool Equals(object other)
        {
            if (other == null) return this == null;
            if (ReferenceEquals(other, this)) return true;
            var otherDiscordUser = other as MockUser;
            if (otherDiscordUser == null) return false;
            return Equals(otherDiscordUser);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}
