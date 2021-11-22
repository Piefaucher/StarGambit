using Autofac;
using StarGambit.Game;
using StarGambit.Game.Impl;

namespace StarGambit
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder moduleBuilder)
        {
            // Game elements
            moduleBuilder.RegisterType<GameProvider>().As<IGameProvider>().SingleInstance();            
        }
    }
}
