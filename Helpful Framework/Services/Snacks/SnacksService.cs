﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Helpful.Framework.Config;
using HelpfulUtilities;
using HelpfulUtilities.Discord.Extensions;
using HelpfulUtilities.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Helpful.Framework.Services
{
    /// <summary>Provides a default service for snacktime.</summary>
    /// <remarks><typeparamref name="TEnum"/> must be an enum. Uses default value of enum.</remarks>
    public partial class SnacksService<TConfig, TGuild, TUser, TCommandContext, TEnum> : IService<TConfig, TGuild, TUser, TCommandContext>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild, ISnacksGuild
        where TUser : class, IConfigUser, ISnacksUser<TEnum>
        where TCommandContext : class, ICommandContext
        where TEnum : Enum
    {
        /// <summary>Whether this service is shutting down.</summary>
        protected bool Disconnecting => Bot != null;
        /// <summary>The <see cref="FrameworkBot{TConfig, TGuild, TUser, TCommandContext}"/> for this service. Non-null only when shutting down.</summary>
        protected FrameworkBot<TConfig, TGuild, TUser, TCommandContext> Bot { get; set; } = null;
        /// <summary>A random number generator for generating values.</summary>
        protected AdvancedRandom Random { get; set; } = new AdvancedRandom();

        /// <summary>A mapping of channel ID to <see cref="SnackEventManager{TEnum}"/></summary>
        protected ConcurrentDictionary<ulong, SnackEventManager<TEnum>> Managers { get; } = new ConcurrentDictionary<ulong, SnackEventManager<TEnum>>();

        /// <summary>Attempts to begin a snack event with the specified config in the specified context.</summary>
        public bool TryStartEvent(TConfig config, ICommandContext context)
        {
            if (context.Guild == null) return false;
            if (!config.Guilds[context.Guild.Id].Snacks.ContainsKey(context.Channel.Id)) return false;
            if (!CanStartEvent(context.Channel.Id)) return false;

            var channel = context.Channel as ITextChannel;
            var snacksConfig = config.Guilds[context.Guild.Id].Snacks[context.Channel.Id];
            var manager = Managers.GetOrAdd(channel.Id, new SnackEventManager<TEnum>());

            if (snacksConfig.MessagesRequired <= ++manager.Messages && CanStartEvent(channel.Id))
            {
                manager.HasBegun = true;
                manager.StartTimer = new Timer(async _ =>
                {
                    await StartEvent(channel, snacksConfig, Snack()).ConfigureAwait(false);
                }, null, TimeSpan.FromMilliseconds(GenerateDelay(snacksConfig)), Timeout.InfiniteTimeSpan);

                return true;
            }

            return false;
        }

        /// <summary>Begins a snack event in the specified channel with the specified config and the specified snack type.</summary>
        public async Task StartEvent(ITextChannel channel, ISnacksChannelConfig config, TEnum snack)
        {
            if (IsActive(channel.Id)) return;
            var manager = Managers.GetOrAdd(channel.Id, new SnackEventManager<TEnum>());
            manager.Pot = await GeneratePotSizeAsync(config, channel).ConfigureAwait(false);
            manager.IsActive = true;
            manager.Snack = snack;

            manager.EndTimer = new Timer(async _ =>
            {
                await StopEvent(channel).ConfigureAwait(false);
            }, null, TimeSpan.FromMilliseconds(GenerateDuration(config)), Timeout.InfiniteTimeSpan);

            await channel.SendMessageAsync(Arrival(snack)).ConfigureAwait(false);
        }

        /// <summary>Stops the snack event running in the specified channel.</summary>
        public async Task StopEvent(ITextChannel channel)
        {
            if (!Managers.TryGetValue(channel.Id, out var manager)) return;
            await channel.SendMessageAsync(manager.Users.Count > 0 ?
                Departure(manager.Snack) : NoPeople(manager.Snack)).ConfigureAwait(false);

            manager.Reset();
            if (Disconnecting && CanDisconnect(Bot))
            {
                Bot.DisconnectList[GetType()] = false;
            }
        }

        /// <summary>Returns if an event can be started in the specified channel</summary>
        public bool CanStartEvent(ulong channelId)
        {
            return !Disconnecting && !IsStarted(channelId) && !IsActive(channelId);
        }

        /// <summary>Returns if an event has started in the specified channel</summary>
        public bool IsStarted(ulong channelId)
        {
            if (Managers.TryGetValue(channelId, out var manager))
                return manager.HasBegun;
            return false;
        }

        /// <summary>Returns if an event is active in the specified channel</summary>
        public bool IsActive(ulong channelId)
        {
            if (Managers.TryGetValue(channelId, out var manager))
                return manager.IsActive;
            return false;
        }

        /// <summary>Gets the snack type for the event in the specified channel</summary>
        public TEnum GetSnack(ulong channelId)
        {
            if (Managers.TryGetValue(channelId, out var manager))
                return manager.Snack;
            return default;
        }

        /// <summary>Gets the list of users already snacked in the specified channel</summary>
        public List<ulong> GetUsers(ulong channelId)
        {
            if (Managers.TryGetValue(channelId, out var manager))
                return manager.Users;
            return new List<ulong>();
        }

        /// <summary>Extending on <see cref="HandleSnackRequestAsync(IUserMessage, TConfig)"/> to send appropriate messages</summary>
        public async Task<(SnackRequestType Type, ulong Amount)> HandleMessageAsync(IUserMessage message, TConfig config)
        {
            var type = await HandleSnackRequestAsync(message, config).ConfigureAwait(false);
            Managers.TryGetValue(message.Channel.Id, out var manager);
            var user = (message.Author as IGuildUser)?.GetEffectiveName() ?? message.Author.Username;

            switch (type.Type)
            {
                case SnackRequestType.Request:
                    await Task.Delay(new Random().Next(0, 7)).ConfigureAwait(false);
                    await message.Channel.SendMessageAsync(Give(manager.Snack, user, type.Amount)).ConfigureAwait(false);
                    break;
                case SnackRequestType.Greedy:
                    await Task.Delay(new Random().Next(0, 7)).ConfigureAwait(false);
                    await message.Channel.SendMessageAsync(Greed(manager.Snack, user)).ConfigureAwait(false);
                    break;
                case SnackRequestType.Rude:
                    await Task.Delay(new Random().Next(0, 7)).ConfigureAwait(false);
                    await message.Channel.SendMessageAsync(Rude(manager.Snack, user)).ConfigureAwait(false);
                    break;
            }
            return type;
        }

        /// <summary>Handles a snack request, parsing the type of request and adding the appropriate amount of snacks to the user.</summary>
        public async Task<(SnackRequestType Type, ulong Amount)> HandleSnackRequestAsync(IUserMessage message, TConfig config)
        {
            var type = ParseSnackRequestType(message);
            Managers.TryGetValue(message.Channel.Id, out var manager);

            switch (type)
            {
                case SnackRequestType.Request:
                    var snacksConfig = config.Guilds[message.GetGuild().Id].Snacks[message.Channel.Id];
                    var amount = GenerateAmount(snacksConfig, manager, message.Author.IsBot);
                    if (!config.Users.TryGetValue(message.Author.Id, out var user))
                        user = await config.Create(message.Author).ConfigureAwait(false);

                    var snack = GetSnack(message.Channel.Id);
                    if (user.Snacks.ContainsKey(snack))
                        user.Snacks[snack] += amount;
                    else
                        user.Snacks[snack] = amount;

                    await config.WriteAsync(DatabaseType.User).ConfigureAwait(false);

                    manager.Users.Add(message.Author.Id);
                    return (type, amount);
                case SnackRequestType.Rude:
                    config.Users[message.Author.Id].Snacks[GetSnack(message.Channel.Id)] += 1;
                    await config.WriteAsync(DatabaseType.User).ConfigureAwait(false);

                    manager.Users.Add(message.Author.Id);
                    return (type, 1);
                default:
                    return (type, 0);
            }
        }

        /// <summary>Parses and returns the <see cref="SnackRequestType"/> based on the specified message.</summary>
        public SnackRequestType ParseSnackRequestType(IUserMessage message)
        {
            if (!Managers.TryGetValue(message.Channel.Id, out var manager) || !manager.IsActive)
                return SnackRequestType.None;

            if (manager.Users.Contains(message.Author.Id))
            {
                if (GreedPhrases[manager.Snack].Any(message.Content.ContainsIgnoreCase))
                    return SnackRequestType.Greedy;
            }
            else
            {
                if (RudePhrases[manager.Snack].Any(message.Content.ContainsIgnoreCase))
                    return SnackRequestType.Rude;
                else if (AgreePhrases[manager.Snack].Any(message.Content.ContainsIgnoreCase))
                    return SnackRequestType.Request;
            }

            return SnackRequestType.None;
        }

        /// <summary>Generates a leaderboard for the snacks.</summary>
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
            fieldFunc ??= ((configUser, _) => $"{(ulong)configUser.Snacks.Sum(s => (double)s.Value)} snacks");
            formatEmbedFunc ??= (embed => embed);

            var orderedLeaderboard = config.Users.Values.OrderByDescending(u => u.Snacks.Sum(s => (double)s.Value));
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
            return !Managers.Values.Any(m => m.IsActive);
        }

        /// <inheritdoc />
        public async Task Disconnect(FrameworkBot<TConfig, TGuild, TUser, TCommandContext> bot)
        {
            Bot = bot;
            foreach (var pair in Managers)
            {
                if (pair.Value.IsActive)
                {
                    var msgChannel = bot.SocketClient.GetChannel(pair.Key) as ITextChannel;
                    await StopEvent(msgChannel).ConfigureAwait(false);
                }
            }
        }
    }
}
