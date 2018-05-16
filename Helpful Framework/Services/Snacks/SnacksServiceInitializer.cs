using Discord;
using Discord.Commands;
using Helpful.Framework.Config;
using HelpfulUtilities;
using HelpfulUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Helpful.Framework.Services
{
    public partial class SnacksService<TConfig, TGuild, TUser, TCommandContext, TEnum> : IService<TConfig, TGuild, TUser, TCommandContext>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild, ISnacksGuild
        where TUser : class, IConfigUser, ISnacksUser<TEnum>
        where TCommandContext : class, ICommandContext
        where TEnum : Enum
    {
        /// <summary>A map of the snack type to the snacks name.</summary>
        protected Dictionary<TEnum, string> Names { get; }
        /// <summary>A map of the snack type to the face for the snack.</summary>
        protected Dictionary<TEnum, string> Faces { get; }

        /// <summary>A map of the snack type to an array of arrival messages.</summary>
        protected Dictionary<TEnum, string[]> ArrivalMessages { get; }
        /// <summary>A map of the snack type to an array of departure messages.</summary>
        protected Dictionary<TEnum, string[]> DepartureMessages { get; }
        /// <summary>A map of the snack type to an array of give messages.</summary>
        protected Dictionary<TEnum, string[]> GiveMessages { get; }
        /// <summary>A map of the snack type to an array of greedy messages.</summary>
        protected Dictionary<TEnum, string[]> GreedMessages { get; }
        /// <summary>A map of the snack type to an array of rude messages.</summary>
        protected Dictionary<TEnum, string[]> RudeMessages { get; }
        /// <summary>A map of the snack type to an array of departure with no people messages.</summary>
        protected Dictionary<TEnum, string[]> NoPeopleMessages { get; }
        /// <summary>A map of the snack type to an array of last second before departure messages.</summary>
        protected Dictionary<TEnum, string[]> LastSecondMessages { get; }

        /// <summary>An array of agreement phrases</summary>
        protected Dictionary<TEnum, string[]> AgreePhrases { get; }
        /// <summary>An array of rude phrases</summary>
        protected Dictionary<TEnum, string[]> RudePhrases { get; }
        /// <summary>An array of greed phrases</summary>
        protected Dictionary<TEnum, string[]> GreedPhrases { get; }

        /// <summary>Instantiates a new SnackService with optional <see cref="SnackMessageBuilder{TEnum}"/></summary>
        public SnacksService(SnackMessageBuilder<TEnum> snackMessages = null)
        {
            snackMessages = snackMessages ?? SnackMessageBuilder<TEnum>.Instance;

            Names = snackMessages.Names;
            Faces = snackMessages.Faces;

            ArrivalMessages = snackMessages.ArrivalMessages;
            DepartureMessages = snackMessages.DepartureMessages;
            GiveMessages = snackMessages.GiveMessages;
            GreedMessages = snackMessages.GreedMessages;
            RudeMessages = snackMessages.RudeMessages;
            NoPeopleMessages = snackMessages.NoPeopleMessages;
            LastSecondMessages = snackMessages.LastSecondMessages;

            AgreePhrases = snackMessages.AgreePhrases;
            RudePhrases = snackMessages.RudePhrases;
            GreedPhrases = snackMessages.GreedPhrases;
        }

        /// <summary>Returns a random snack as defined by <typeparamref name="TEnum"/></summary>
        public TEnum Snack() => EnumUtils.Random<TEnum>(typeof(TEnum));

        /// <summary>Returns a random arrival message based off snack type</summary>
        public string Arrival(TEnum snack) => $"{Faces[snack]} {ArrivalMessages[snack].Random()}";

        /// <summary>Returns a random departure message based off snack type</summary>
        public string Departure(TEnum snack) => $"{Faces[snack]} {DepartureMessages[snack].Random()}";

        /// <summary>Returns a random departure with no people based off snack type</summary>
        public string NoPeople(TEnum snack) => $"{Faces[snack]} {NoPeopleMessages[snack].Random()}";

        /// <summary>Returns a random give message based off snack type, username, and amount of snacks</summary>
        public string Give(TEnum snack, string user, ulong amount)
            => string.Format($"{Faces[snack]} {GiveMessages[snack].Random()}", amount, user, Names[snack]);

        /// <summary>Returns a random rude message based off snack type and username</summary>
        public string Greed(TEnum snack, string user)
            => string.Format($"{Faces[snack]} {GreedMessages[snack].Random()}", user, Names[snack]);

        /// <summary>Returns a random rude message based off snack type and username</summary>
        public string Rude(TEnum snack, string user)
            => string.Format($"{Faces[snack]} {RudeMessages[snack].Random()}", user, Names[snack]);

        /// <summary>Generates a random delay based on the <see cref="ISnacksChannelConfig"/> passed.</summary>
        public virtual ulong GenerateDelay(ISnacksChannelConfig config)
        {
            return Random.Next(Math.Max(config.Delay - config.DelayVariance, 1),
                config.Delay + config.DelayVariance + 1);
        }

        /// <summary>Generates a random duration based on the <see cref="ISnacksChannelConfig"/> passed.</summary>
        public virtual ulong GenerateDuration(ISnacksChannelConfig config)
        {
            return Random.Next(Math.Max(config.Duration - config.DurationVariance, 1),
                config.Duration + config.DurationVariance + 1);
        }

        /// <summary>Generates a random amount based on the <see cref="ISnacksChannelConfig"/> 
        /// and <see cref="SnackEventManager{TEnum}"/> passed.</summary>
        public virtual ulong GenerateAmount(ISnacksChannelConfig config, SnackEventManager<TEnum> manager)
        {
            var amount = Random.Next(1, config.Amount + 1);
            var snackers = manager.Users.LongCount();

            if (snackers < (long)config.EarlyBirdPotSize)
            {
                var addition = (ulong)Math.Floor((double)manager.Pot / 2);
                amount += addition;
                manager.Pot -= addition;
            }
            else if (snackers == (long)config.EarlyBirdPotSize)
            {
                amount += manager.Pot;
                manager.Pot = 0;
            }

            return amount;
        }

        /// <summary>Generates a random pot size based on the <see cref="ISnacksChannelConfig"/> 
        /// and <see cref="ITextChannel"/> passed</summary>
        protected virtual async Task<ulong> GeneratePotSizeAsync(ISnacksChannelConfig config, ITextChannel channel)
        {
            var pot = Random.Next(Math.Max(config.EarlyBirdPot - config.EarlyBirdPotVariance, 1),
                config.EarlyBirdPot + config.EarlyBirdPotVariance + 1);

            var users = (await channel.GetUsersAsync().FlattenAsync().ConfigureAwait(false)).Where(user =>
            {
                return !user.IsBot && !user.IsWebhook
                        && user.Status != UserStatus.Offline
                        && user.Status != UserStatus.Invisible;
            });

            return (ulong)users.LongCount() * pot;
        }
    }
}
