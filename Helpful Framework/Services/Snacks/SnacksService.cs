using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Helpful.Framework.Config;
using HelpfulUtilities;
using HelpfulUtilities.Discord.Extensions;
using HelpfulUtilities.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Helpful.Framework.Services
{
    /// <summary>Provides a default service for snacktime.</summary>
    /// <remarks><typeparamref name="TEnum"/> must be an enum. Uses default value of enum.</remarks>
    public partial class SnacksService<TConfig, TGuild, TUser, TEnum> : IService<TConfig, TGuild, TUser>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild, ISnacksGuild
        where TUser : class, IConfigUser, ISnacksUser<TEnum>
        where TEnum : struct, IComparable, IConvertible, IFormattable
    {
        /// <summary>Whether this service is shutting down.</summary>
        protected bool Disconnecting => Bot != null;
        /// <summary>The <see cref="FrameworkBot{TConfig, TGuild, TUser}"/> for this service. Non-null only when shutting down.</summary>
        protected FrameworkBot<TConfig, TGuild, TUser> Bot { get; set; } = null;
        /// <summary>A random number generator for generating values.</summary>
        protected AdvancedRandom Random { get; set; } = new AdvancedRandom();

        /// <summary>A mapping of channel ID to <see cref="SnackEventManager{TEnum}"/></summary>
        protected ConcurrentDictionary<ulong, SnackEventManager<TEnum>> Managers { get; } = new ConcurrentDictionary<ulong, SnackEventManager<TEnum>>();

        /// <summary>Attempts to begin a snack event with the specified config in the specified context.</summary>
        public async Task<bool> TryStartEvent(TConfig config, ICommandContext context)
        {
            if (context.Guild == null || 
                !config.Guilds[context.Guild.Id].Snacks.ContainsKey(context.Channel.Id)
                || !CanStartEvent(context.Channel.Id))
            {
                return false;
            }

            var channel = context.Channel as ITextChannel;
            var snacksConfig = config.Guilds[context.Guild.Id].Snacks[context.Channel.Id];
            var manager = Managers.GetOrAdd(channel.Id, new SnackEventManager<TEnum>());

            if (snacksConfig.MessagesRequired <= ++manager.Messages && CanStartEvent(channel.Id))
            {
                manager.HasBegun = true;
                await Task.Run(async () =>
                {
                    await Operations.DelayAsync(GenerateDelay(snacksConfig)).ConfigureAwait(false);
                    await StartEvent(channel, snacksConfig, Snack()).ConfigureAwait(false);
                }).ConfigureAwait(false);
                return true;
            }

            return false;
        }
        
        /// <summary>Begins a snack event in the specified channel with the specified config and the specified snack type.</summary>
        public async Task StartEvent(ITextChannel channel, ISnacksChannelConfig config, TEnum snack)
        {
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
            var snack = Managers[channel.Id].Snack;
            await channel.SendMessageAsync(Managers[channel.Id].Users.Any() ?
                Departure(snack) : NoPeople(snack)).ConfigureAwait(false);

            Managers[channel.Id].Reset();
            if (CanDisconnect(Bot))
            {
                Bot.DisconnectList[GetType()] = false;
            }
        }

        /// <summary>Returns if an event can be started in the specified channel</summary>
        public bool CanStartEvent(ulong channelId)
        {
            return !Disconnecting && IsActive(channelId);
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
            return default(TEnum);
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
                    var amount = GenerateAmount(snacksConfig, manager);
                    config.Users[message.Author.Id].Snacks[GetSnack(message.Channel.Id)] += amount;
                    await config.WriteAsync(DatabaseType.User).ConfigureAwait(false);
                    return (type, amount);
                case SnackRequestType.Rude:
                    config.Users[message.Author.Id].Snacks[GetSnack(message.Channel.Id)] += 1;
                    await config.WriteAsync(DatabaseType.User).ConfigureAwait(false);
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
        public EmbedBuilder GenerateLeaderboard(TConfig config, FrameworkBot<TConfig, TGuild, TUser> bot, SocketGuild guild = null,
            LeaderboardScale scale = LeaderboardScale.Global, EmbedBuilder builder = null, int size = 10,
            Func<TUser, SocketUser, string> fieldFunc = null, Func<EmbedBuilder, EmbedBuilder> formatEmbedFunc = null)
        {
            builder = builder ?? new EmbedBuilder();
            fieldFunc = fieldFunc ?? ((configUser, _) => $"{(ulong)configUser.Snacks.Sum(s => (double)s.Value)} snacks");
            formatEmbedFunc = formatEmbedFunc ?? (embed => embed);

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
                    Name = $"#{position} {(user as IGuildUser)?.Nickname ?? user.Username}",
                    Value = fieldFunc(cuser, user),
                    IsInline = false
                });
            }

            return formatEmbedFunc(builder);
        }

        /// <inheritdoc />
        public bool CanDisconnect(FrameworkBot<TConfig, TGuild, TUser> bot)
        {
            Bot = bot;
            return Managers.LongCount(m => m.Value.IsActive) <= 0;
        }

        /// <inheritdoc />
        public async Task Disconnect(FrameworkBot<TConfig, TGuild, TUser> bot)
        {
            foreach (var channel in Managers.Keys)
            {
                var msgChannel = bot.SocketClient.GetChannel(channel) as ITextChannel;
                await StopEvent(msgChannel).ConfigureAwait(false);
            }
        }
    }
}
