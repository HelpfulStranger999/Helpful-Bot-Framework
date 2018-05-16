using Discord;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Helpful.Framework.Services
{
    /// <summary>A creature manager for <see cref="CreatureSpawnerService{TConfig, TGuild, TUser, TCommandContext}"/></summary>
    public class CreatureManager
    {
        /// <summary>The message containing the creature</summary>
        public IUserMessage Creature { get; set; } = null;
        /// <summary>The timer for creatures despawning naturally</summary>
        public Timer Despawner { get; set; }

        public async Task Despawn()
        {
            if (Creature != null)
            {
                await Creature.DeleteAsync().ConfigureAwait(false);
                Creature = null;
            }

            Despawner.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
