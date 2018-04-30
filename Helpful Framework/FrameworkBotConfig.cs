using Discord.Commands;
using Discord.WebSocket;

namespace Helpful.Framework
{
    /// <summary>A bot configuration for <see cref="FrameworkBot{TConfig, TGuild, TUser}"/></summary>
    public class FrameworkBotConfig : DiscordSocketConfig
    {
        /// <summary>The token of the bot</summary>
        public string Token { get; set; }
        /// <summary>Whether the bot is running in beta mode</summary>
        public bool RunningAsBeta { get; set; } = false;
        /// <summary>Whether the bot should be sharded</summary>
        public bool ShouldShard { get; set; } = false;
        /// <summary>The default prefix of the bot</summary>
        public string Prefix { get; set; } = "!";
        /// <summary>The <see cref="CommandServiceConfig"/> to use</summary>
        public CommandServiceConfig CommandServiceConfig { get; set; } = new CommandServiceConfig();
        /// <summary>An optional path to the database.</summary>
        public string Database { get; set; } = null;
    }
}
