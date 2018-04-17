using Discord;
using Discord.Commands;
using Helpful.Framework.Config;
using HelpfulUtilities;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Helpful.Framework.Services
{
    /// <summary>Provides a default service for snacktime.</summary>
    /// <remarks><typeparamref name="TEnum"/> must be an enum. <typeparamref name="TGuild"/> must </remarks>
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
                    await Operations.DelayAsync(GenerateDelay(snacksConfig));
                    await StartEvent(channel, snacksConfig, Snack());
                });
                return true;
            }

            return false;
        }
        
        /// <summary>Begins a snack event in the specified channel with the specified config and the specified snack type.</summary>
        public async Task StartEvent(ITextChannel channel, ISnacksChannelConfig config, TEnum snack)
        {
            var manager = Managers.GetOrAdd(channel.Id, new SnackEventManager<TEnum>());
            manager.Pot = await GeneratePotSizeAsync(config, channel);
            manager.IsActive = true;
            manager.Snack = snack;

            manager.EndTimer = new Timer(async _ =>
            {
                await StopEvent(channel);
            }, null, TimeSpan.FromMilliseconds(GenerateDuration(config)), Timeout.InfiniteTimeSpan);

            await channel.SendMessageAsync(Arrival(snack));
        }

        /// <summary>Stops the snack event running in the specified channel.</summary>
        public async Task StopEvent(ITextChannel channel)
        {
            var snack = Managers[channel.Id].Snack;
            await channel.SendMessageAsync(Managers[channel.Id].Users.Any() ?
                Departure(snack) : NoPeople(snack));

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
                await StopEvent(msgChannel);
            }
        }
    }
}
