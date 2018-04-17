using Discord;
using Helpful.Framework.Config;
using HelpfulUtilities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spawner = System.Collections.Concurrent.ConcurrentDictionary<ulong, Helpful.Framework.Services.CreatureManager>;

namespace Helpful.Framework.Services
{
    /// <summary>Provides a default service for spawning creatures</summary>
    public class CreatureSpawnerService<TConfig, TGuild, TUser> : IService<TConfig, TGuild, TUser>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild, ISpawnerGuild
        where TUser : class, IConfigUser, ISpawnerUser
    {
        /// <summary>Sets the message sent when a creature despawns naturally.</summary>
        /// <remarks>Highly recommended you set this to your specific creature</remarks>
        public static string DespawnMessage { get; set; } = $"The creature wandered away again";

        /// <summary>Whether this service is shutting down.</summary>
        protected bool Disconnecting => Bot != null;
        /// <summary>The <see cref="FrameworkBot{TConfig, TGuild, TUser}"/> for this service. Non-null only when shutting down.</summary>
        protected FrameworkBot<TConfig, TGuild, TUser> Bot { get; set; } = null;
        /// <summary>A random number generator for generating values.</summary>
        protected AdvancedRandom Random { get; set; } = new AdvancedRandom();
        /// <summary>A mapping of channel ID to creature manager</summary>
        protected Spawner Spawner { get; } = new Spawner();

        /// <summary>Returns whether any creatures can spawn in the specified channel</summary>
        public bool CanSpawn(ulong channelID) => !Disconnecting && !AnyLoose(channelID);
        /// <summary>Returns whether there are any loose creatures in the specified channel</summary>
        public bool AnyLoose(ulong channelID) => GetCreature(channelID) != null;
        /// <summary>Returns the message containing the creature in the specified channel</summary>
        public IUserMessage GetCreature(ulong channelID) => Spawner.GetOrAdd(channelID, new CreatureManager()).Creature;

        /// <summary>Spawns a creature with the specified message and the specified guild.</summary>
        public bool Spawn(ISpawnerGuild guild, IUserMessage message)
        {
            if (!CanSpawn(message.Channel.Id)) return false;
            var manager = Spawner.GetOrAdd(message.Channel.Id, new CreatureManager());

            manager.Creature = message;
            manager.Despawner = new Timer(async _ =>
            {
                await message.Channel.SendMessageAsync(DespawnMessage);
                await Despawn(message.Channel.Id);
            }, null, TimeSpan.FromMilliseconds(guild.Duration), Timeout.InfiniteTimeSpan);

            return true;
        }

        /// <summary>Despawns the creature in the channel.</summary>
        public Task<bool> Despawn(ulong channelId) => Despawn(channelId, null, null);
        /// <summary>Despawns the creature in the channel as the specified user.</summary>
        public async Task<bool> Despawn(ulong channelId, TConfig config, TUser user)
        {
            if (!AnyLoose(channelId)) return false;
            await Spawner[channelId].Creature.DeleteAsync();
            Spawner[channelId].Despawner.Change(Timeout.Infinite, Timeout.Infinite);

            if (user != null)
            {
                user.Creatures++;
                await config.WriteAsync(DatabaseType.User);
            }

            if (Disconnecting && CanDisconnect(Bot))
            {
                Bot.DisconnectList[GetType()] = true;
            }

            return user != null;
        }

        /// <inheritdoc />
        public bool CanDisconnect(FrameworkBot<TConfig, TGuild, TUser> bot)
        {
            Bot = bot;
            return Spawner.LongCount() <= 0;
        }

        /// <inheritdoc />
        public Task Disconnect(FrameworkBot<TConfig, TGuild, TUser> bot)
        {
            Spawner.Clear();
            return Task.CompletedTask;
        }
    }
}
