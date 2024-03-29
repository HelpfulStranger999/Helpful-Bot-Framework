﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
    public class CreatureSpawnerService<TConfig, TGuild, TUser, TCommandContext> : IService<TConfig, TGuild, TUser, TCommandContext>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild, ISpawnerGuild
        where TUser : class, IConfigUser, ISpawnerUser
        where TCommandContext : class, ICommandContext
    {
        /// <summary>Provides a default field generator for <see cref="GenerateLeaderboard(TConfig, FrameworkBot{TConfig, TGuild, TUser, TCommandContext}, SocketGuild, LeaderboardScale, EmbedBuilder, int, Func{TUser, SocketUser, string}, Func{EmbedBuilder, EmbedBuilder})"/></summary>
        protected static readonly Func<TUser, SocketUser, string> DefaultLeaderboardFieldFunction = (configUser, socketUser) =>
        {
            return $"{configUser.Creatures} creatures";
        };

        /// <summary>Sets the message sent when a creature despawns naturally.</summary>
        /// <remarks>Highly recommended you set this to your specific creature</remarks>
        public static string DespawnMessage { get; set; } = $"The creature wandered away again";

        /// <summary>Whether this service is shutting down.</summary>
        protected bool Disconnecting => Bot != null;
        /// <summary>The <see cref="FrameworkBot{TConfig, TGuild, TUser, TCommandContext}"/> for this service. Non-null only when shutting down.</summary>
        protected FrameworkBot<TConfig, TGuild, TUser, TCommandContext> Bot { get; set; } = null;
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

        /// <summary>Returns whether a creature should be spawned in the specified channel</summary>
        public virtual bool ShouldSpawn(TGuild guild, ulong channelID)
        {
            if (!CanSpawn(channelID)) return false;
            if (!guild.CreatureChannels.Contains(channelID)) return false;
            return Random.NextDouble() <= guild.Frequency;
        }

        /// <summary>Spawns a creature with the specified message and the specified guild.</summary>
        public bool Spawn(ISpawnerGuild guild, IUserMessage message)
        {
            if (!CanSpawn(message.Channel.Id)) return false;
            var manager = Spawner.GetOrAdd(message.Channel.Id, new CreatureManager());

            manager.Creature = message;
            manager.Despawner = new Timer(async _ =>
            {
                await message.Channel.SendMessageAsync(DespawnMessage).ConfigureAwait(false);
                await Despawn(message.Channel.Id).ConfigureAwait(false);
            }, null, TimeSpan.FromMilliseconds(guild.Duration), Timeout.InfiniteTimeSpan);

            return true;
        }

        /// <summary>Despawns the creature in the channel.</summary>
        public Task<bool> Despawn(ulong channelId) => Despawn(channelId, null, null);
        /// <summary>Despawns the creature in the channel as the specified user.</summary>
        public async Task<bool> Despawn(ulong channelId, TConfig config, TUser user)
        {
            if (!AnyLoose(channelId)) return false;
            await Spawner[channelId].Despawn();

            if (user != null)
            {
                user.Creatures++;
                await config.WriteAsync(DatabaseType.User).ConfigureAwait(false);
            }

            if (Disconnecting && CanDisconnect(Bot))
            {
                Bot.DisconnectList[GetType()] = true;
            }

            return user != null;
        }

        /// <summary>Generates a leaderboard for the creatures.</summary>
        /// <param name="config">The configuration to use</param>
        /// <param name="bot">The framework bot to use</param>
        /// <param name="guild">The guild the command was run in. Defaults to null.</param>
        /// <param name="scale">The <see cref="LeaderboardScale"/> to use. Defaults to <see cref="LeaderboardScale.Global"/></param>
        /// <param name="builder">The EmbedBuild to use. Defaults to a new one.</param>
        /// <param name="size">The size of the leaderboard. Defaults to 10.</param>
        /// <param name="fieldFunc">A function taking a <typeparamref name="TConfig"/> and a <see cref="SocketUser"/> and
        /// returning the field value of the positions on the leaderboard.</param>
        /// <param name="formatEmbedFunc">A function taking and returning an embed builder for the purpose of customizing it.</param>
        /// <returns>The embed populated with the leaderboard</returns>
        public EmbedBuilder GenerateLeaderboard(TConfig config, FrameworkBot<TConfig, TGuild, TUser, TCommandContext> bot, SocketGuild guild = null,
            LeaderboardScale scale = LeaderboardScale.Global, EmbedBuilder builder = null, int size = 10,
            Func<TUser, SocketUser, string> fieldFunc = null, Func<EmbedBuilder, EmbedBuilder> formatEmbedFunc = null)
        {
            builder ??= new EmbedBuilder();
            fieldFunc ??= ((configUser, _) => $"{configUser.Creatures} creatures");
            formatEmbedFunc ??= (embed => embed);

            var orderedLeaderboard = config.Users.Values.OrderByDescending(u => u.Creatures);
            var leaderboard = (scale == LeaderboardScale.Server && guild != null) ?
                    orderedLeaderboard.Where(user => guild.GetUser(user.Id) != null).ToArray() :
                    orderedLeaderboard.ToArray();

            for (var position = 1; position <= Math.Min(leaderboard.Length, size); position++)
            {
                var cuser = leaderboard[position - 1];
                var user = guild?.GetUser(cuser.Id) ?? bot.SocketClient.GetUser(cuser.Id);
                builder.AddField(new EmbedFieldBuilder
                {
                    Name = $"#{position} {((user as IGuildUser)?.Nickname ?? user?.Username) ?? "Unknown User"}",
                    Value = fieldFunc(cuser, user),
                    IsInline = false
                });
            }

            return formatEmbedFunc(builder);
        }

        /// <inheritdoc />
        public bool CanDisconnect(FrameworkBot<TConfig, TGuild, TUser, TCommandContext> bot)
        {
            Bot = bot;
            return Spawner.IsEmpty;
        }

        /// <inheritdoc />
        public Task Disconnect(FrameworkBot<TConfig, TGuild, TUser, TCommandContext> bot)
        {
            Spawner.Clear();
            return Task.CompletedTask;
        }
    }
}
