using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using StarGambit.Game;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace StarGambit.System
{
    internal class DiscordClient : IDiscordClient
    {

        private DiscordSocketClient client;
        private CommandService commands;

        public IServiceProvider ServiceProvider { get; }

        public DiscordClient()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacModule());
            var container = builder.Build();
            ServiceProvider = new AutofacServiceProvider(container);
            this.client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug
            });
            this.commands = new CommandService();
        }

        public async Task RunBotAsync()
        {
            client.Log += Log;

            client.Ready += () =>
            {
                Console.WriteLine("I am ready");
                return Task.CompletedTask;
            };

            await InstallCommandAsync().ConfigureAwait(false);
            await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("STAR_GAMBIT_TOKEN", EnvironmentVariableTarget.User)).ConfigureAwait(false);
            await client.StartAsync().ConfigureAwait(false);

            await Task.Delay(-1);
        }

        public async Task InstallCommandAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider).ConfigureAwait(false);
        }

        private async Task HandleCommandAsync(SocketMessage pMessage)
        {
            var message = pMessage as SocketUserMessage;
            if (message == null) return;

            var argPos = 0;

            if (!message.HasCharPrefix('!', ref argPos)) return;

            var context = new SocketCommandContext(client, message);

            var result = await commands.ExecuteAsync(context, argPos, ServiceProvider).ConfigureAwait(false);

            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }
    }
}
