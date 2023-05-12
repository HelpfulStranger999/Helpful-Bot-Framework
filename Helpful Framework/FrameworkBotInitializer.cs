using Discord.Commands;
using Discord.WebSocket;
using Helpful.Framework.Config;
using HelpfulUtilities.Discord.Commands.Extensions;
using HelpfulUtilities.Discord.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Helpful.Framework
{
    public abstract partial class FrameworkBot<TConfig, TGuild, TUser, TCommandContext>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild
        where TUser : class, IConfigUser
        where TCommandContext : class, ICommandContext
    {

        /// <summary>Default handling of ready events</summary>
        public async Task DefaultReadyHandler()
        {
            var guilds = SocketClient.Guilds.Where(g => !Configuration.Guilds.ContainsKey(g.Id));

            foreach (var guild in guilds)
                await Configuration.Create(guild).ConfigureAwait(false);

            if (guilds.Any())
                await Configuration.WriteAsync(DatabaseType.Guild).ConfigureAwait(false);
        }

        /// <summary>Default handling of messages</summary>
        public async Task DefaultMessageReceivedHandler(SocketMessage msg)
        {
            if (msg.Author.IsWebhook || msg.Author.Id == SocketClient.CurrentUser.Id) { return; }

            if (msg is SocketUserMessage message)
            {
                var pos = 0;
                var prefix = BotConfig.Prefix;
                var context = CreateContext(message);

                if (message.Channel is SocketTextChannel)
                    prefix = Configuration.Guilds[message.GetGuild().Id].Prefix;

                if (message.HasPrefix(prefix, SocketClient, ref pos))
                {
                    await HandleResult(context, await CommandService.ExecuteAsync(context, pos, ServiceProvider)).ConfigureAwait(false);
                }
                else
                {
                    foreach (var result in await ListenerService.ExecuteAsync(context, ServiceProvider))
                    {
                        await HandleResult(context, result, false).ConfigureAwait(false);
                    }
                }
            }
        }
        /// <summary>Builds the service provider.</summary>
        public virtual IServiceProvider BuildServices(IServiceCollection collection, params object[] dependencies)
        {
            collection ??= new ServiceCollection();

            foreach (var dependency in dependencies)
            {
                collection.AddSingleton(dependency.GetType(), dependency);
            }

            return collection.BuildServiceProvider();
        }

        /// <summary>Registers logs to the various services.</summary>
        /// <remarks>Does nothing by default.</remarks>
        public virtual void RegisterLogs() { }

        /// <summary>Registers type readers to the command service</summary>
        /// <remarks>If you override and use <see cref="TypeReaders"/>, be sure to either call this base method or manually load from there!</remarks>
        public virtual void RegisterTypeReaders()
        {
            foreach (var pair in TypeReaders)
            {
                CommandService.AddTypeReader(pair.Key, pair.Value);
            }
        }
    }
}
