namespace StarGambit.Game
{
    public interface IGameProvider
    {
        public IGame Provide(ulong channelIdentifier, IPlayer gameMaster);
    }
}
