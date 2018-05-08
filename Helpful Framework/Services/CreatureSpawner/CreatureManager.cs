using Discord;
using System.Threading;

namespace Helpful.Framework.Services
{
    /// <summary>A creature manager for <see cref="CreatureSpawnerService{TConfig, TGuild, TUser, TCommandContext}"/></summary>
    public class CreatureManager
    {
        /// <summary>The message containing the creature</summary>
        public IUserMessage Creature { get; set; } = null;
        /// <summary>The timer for creatures despawning naturally</summary>
        public Timer Despawner { get; set; }
    }
}
