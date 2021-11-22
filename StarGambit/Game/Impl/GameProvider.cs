using System.Collections.Generic;

namespace StarGambit.Game.Impl
{
    internal class GameProvider : IGameProvider
    {
        private IDictionary<ulong, IGame> Games { get; }

        public GameProvider()
        {
            Games = new Dictionary<ulong, IGame>();
        }

        public IGame Provide(ulong channelIdentifier, IPlayer gameMaster)
        {
            if (!Games.TryGetValue(channelIdentifier, out var result))
            {
                result = new Game(gameMaster);
                Games[channelIdentifier] = result;                
            }
            return result;
        }
    }
}
