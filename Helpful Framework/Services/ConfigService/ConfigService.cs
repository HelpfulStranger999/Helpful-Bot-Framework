using Discord.WebSocket;
using Helpful.Framework.Config;
using System.Linq;
using System.Threading.Tasks;

namespace Helpful.Framework.Services
{
    /// <summary>Provides a default service for automatically loading guilds to the config</summary>
    public sealed class ConfigService<TConfig, TGuild, TUser> : IService<TConfig, TGuild, TUser>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild
        where TUser : class, IConfigUser
    {
        private BaseSocketClient Client { get; }
        private TConfig Config { get; }

        /// <summary>Instantiates a new <see cref="ConfigService{TConfig, TGuild, TUser}"/></summary>
        public ConfigService(FrameworkBot<TConfig, TGuild, TUser> bot)
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
                await Config.Create(guild);
            }

            if (newGuilds.LongCount() > 0) await Config.WriteAsync(DatabaseType.Guild);
        }

        private async Task GuildJoin(SocketGuild guild)
        {
            await Config.Create(guild);
            await Config.WriteAsync(DatabaseType.Guild);
        }

        /// <inheritdoc />
        public bool CanDisconnect(FrameworkBot<TConfig, TGuild, TUser> bot) => true;

        /// <inheritdoc />
        public async Task Disconnect(FrameworkBot<TConfig, TGuild, TUser> bot)
        {
            await Config.Disconnect();
        }
    }
}
