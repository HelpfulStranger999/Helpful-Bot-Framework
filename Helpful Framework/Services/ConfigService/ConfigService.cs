using Discord.Commands;
using Discord.WebSocket;
using Helpful.Framework.Config;
using System.Linq;
using System.Threading.Tasks;

namespace Helpful.Framework.Services
{
    /// <summary>Provides a default service for automatically loading guilds to the config</summary>
    public sealed class ConfigService<TConfig, TGuild, TUser, TCommandContext> : IService<TConfig, TGuild, TUser, TCommandContext>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild
        where TUser : class, IConfigUser
        where TCommandContext : class, ICommandContext
    {
        private BaseSocketClient Client { get; }
        private TConfig Config { get; }

        /// <summary>Instantiates a new <see cref="ConfigService{TConfig, TGuild, TUser, TCommandContext}"/></summary>
        public ConfigService(FrameworkBot<TConfig, TGuild, TUser, TCommandContext> bot)
        {
            Client = bot.SocketClient;
            Config = bot.Configuration;

            bot.SocketClient.JoinedGuild += GuildJoin;
            bot.Ready(Ready);
        }

        private async Task Ready()
        {
            var newGuilds = Client.Guilds.Where(g => !Config.Guilds.ContainsKey(g.Id));
            foreach (var guild in newGuilds)
            {
                await Config.Create(guild).ConfigureAwait(false);
            }

            if (newGuilds.LongCount() > 0) await Config.WriteAsync(DatabaseType.Guild).ConfigureAwait(false);
        }

        private async Task GuildJoin(SocketGuild guild)
        {
            await Config.Create(guild).ConfigureAwait(false);
            await Config.WriteAsync(DatabaseType.Guild).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public bool CanDisconnect(FrameworkBot<TConfig, TGuild, TUser, TCommandContext> bot) => true;

        /// <inheritdoc />
        public async Task Disconnect(FrameworkBot<TConfig, TGuild, TUser, TCommandContext> bot)
        {
            await Config.Disconnect().ConfigureAwait(false);
        }
    }
}
