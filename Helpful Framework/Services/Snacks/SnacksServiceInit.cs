using Discord;
using Helpful.Framework.Config;
using HelpfulUtilities;
using HelpfulUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Helpful.Framework.Services
{
    public partial class SnacksService<TConfig, TGuild, TUser, TEnum> : IService<TConfig, TGuild, TUser>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild, ISnacksGuild
        where TUser : class, IConfigUser, ISnacksUser<TEnum>
        where TEnum : struct, IComparable, IConvertible, IFormattable
    {
        /// <summary>A map of the snack type to the snacks name.</summary>
        public static Dictionary<TEnum, string> Names { get; set; }
        /// <summary>A map of the snack type to the face for the snack.</summary>
        public static Dictionary<TEnum, string> Faces { get; set; }

        /// <summary>A map of the snack type to an array of arrival messages.</summary>
        public static Dictionary<TEnum, string[]> ArrivalMessages { get; set; }
        /// <summary>A map of the snack type to an array of departure messages.</summary>
        public static Dictionary<TEnum, string[]> DepartureMessages { get; set; }
        /// <summary>A map of the snack type to an array of give messages.</summary>
        public static Dictionary<TEnum, string[]> GiveMessages { get; set; }
        /// <summary>A map of the snack type to an array of greedy messages.</summary>
        public static Dictionary<TEnum, string[]> GreedMessages { get; set; }
        /// <summary>A map of the snack type to an array of rude messages.</summary>
        public static Dictionary<TEnum, string[]> RudeMessages { get; set; }
        /// <summary>A map of the snack type to an array of departure with no people messages.</summary>
        public static Dictionary<TEnum, string[]> NoPeopleMessages { get; set; }
        /// <summary>A map of the snack type to an array of last second before departure messages.</summary>
        public static Dictionary<TEnum, string[]> LastSecondMessages { get; set; }

        /// <summary>An array of agreement phrases</summary>
        public static string[] AgreePhrases { get; set; }
        /// <summary>An array of rude phrases</summary>
        public static string[] RudePhrases { get; set; }
        /// <summary>An array of greed phrases</summary>
        public static string[] GreedPhrases { get; set; }

        static SnacksService()
        {
            Names = new Dictionary<TEnum, string>();
            Faces = new Dictionary<TEnum, string>();

            ArrivalMessages = new Dictionary<TEnum, string[]>();
            DepartureMessages = new Dictionary<TEnum, string[]>();
            GiveMessages = new Dictionary<TEnum, string[]>();
            RudeMessages = new Dictionary<TEnum, string[]>();
            NoPeopleMessages = new Dictionary<TEnum, string[]>();
            LastSecondMessages = new Dictionary<TEnum, string[]>();

            var _arrival = new string[]
            {
                "It's snack time!",
                "I'm back with s'more snacks! Who wants!?",
                "I'm back errbody! Who wants some snacks!?",
                "Woo man those errands are crazy! Anyways, anybody want some snacks?",
                "I got snacks! If nobody wants em, I'm gonna eat em all!!",
                "Hey, I'm back! Anybody in the mood for some snacks?!",
                "Heyyaaayayyyaya! I say Hey, I got snacks!",
                "Heyyaaayayyyaya! I say Hey, What's goin on?... I uh.. I got snacks.",
                "If anybody has reason why these snacks and my belly should not be wed, speak now or forever hold your peace!",
                "Got another snack delivery guys!",
                "Did somebody say snacks?!?! o/",
                "Choo Choo! it's the pb train! Come on over guys!",
                "Snacks are here! Dig in! Who wants a plate?",
                "Pstt.. I got the snacks you were lookin for. <.<",
                "I hope you guys are hungry! Cause i'm loaded to the brim with snacks!!!",
                "I was hungry on the way over so I kinda started without you guys :3 Who wants snacks!?!",
                "Beep beep! I got a snack delivery comin in! Who wants snacks!",
                "Guess what time it is?! It's snacktime!! Who wants?!",
                "Hey check out this sweet stach o' snacks I found! Who wants a cut?",
                "Who's ready to gobble down some snacks!?",
                "So who's gonna help me eat all these snacks? :3"
            };

            var _departure = new string[]
            {
                "I'm out of snacks! I'll be back with more soon.",
                "I'm out of snacks :( I'll be back soon with more!",
                "Aight, I gotta head out! I'll be back with more, don worry :3",
                "Alright, I gotta get back to my errands. I'll see you guys soon!"
            };

            var _give = new string[]
            {
                "Here ya go, {1:#}, here's {0:D} {2:#}!",
                "Alright here ya go, {1:#}, {0:D} {2:#} for you!",
                "Yeah! Here you go, {1:#}! {0:D} {2:#}!",
                "Of course {1:#}! Here's {0:D} {2:#}!",
                "Ok {1:#}, here's {0:D} {2:#} for you. Anyone else want some?",
                "Alllright, {0:D} {2:#} for {1:#}!",
                "Hold your horses {1:#}! Alright, {0:D} {2:#} for you :)"
            };

            var _rude = new string[]
            {
                "Wow, you're rude. Have one {1:#}, {0:#}."
            };

            var _greed = new string[]
            {
                "Don't be greedy now! you already got some {1:#} {0:#}!",
                "You already got your {1:#} {0:#}!",
                "Come on {0:#}, you already got your {1:#}! We gotta make sure there's some for errbody!"
            };

            var _nopeople = new string[]
            {
                "I guess nobody wants snacks... more for me!`",
                "Guess nobody's here.. I'll just head out then`",
                "I don't see anybody.. <.< ... >.> ... All the snacks for me!!",
                "I guess nobody wants snacks huh.. Well, I'll come back later",
                "I guess i'll just come back later.."
            };

            var _lastsec = new string[]
            {
                "Fine fine, {0:#}, I'll give you {1:D} of my on-the-road {2:#}.. Cya!",
                "Oh! {0:#}, you caught me right before I left! Alright, i'll give you {1:D} of my own {2:#}"
            };

            foreach (var type in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
            {
                Names.Add(type, Enum.GetName(typeof(TEnum), type));
                Faces.Add(type, "(^=˃ᆺ˂)");

                ArrivalMessages.Add(type, _arrival);
                DepartureMessages.Add(type, _departure);
                GiveMessages.Add(type, _give);
                RudeMessages.Add(type, _rude);
                GreedMessages.Add(type, _greed);
                NoPeopleMessages.Add(type, _nopeople);
                LastSecondMessages.Add(type, _lastsec);
            }

            var _phrases = new List<string>();
            _phrases.AddRange("holds out hand",
                "im ready",
                "i'm ready",
                "hit me up",
                "hand over",
                "hand me",
                "kindly",
                "i want",
                "i'll have",
                "ill have",
                "yes",
                "pls",
                "plz",
                "please",
                "por favor",
                "can i",
                "i'd like",
                "i would",
                "may i",
                "in my mouth",
                "in my belly",
                "snack me",
                "gimme",
                "give me",
                "i'll take",
                "ill take",
                "i am",
                "about me",
                "me too",
                "of course",
                "me want");

            AgreePhrases = _phrases.ToArray();

            _phrases.AddRange("more pl",
                "i have some more",
                "i want more",
                "i have another",
                "i have more",
                "more snack");

            GreedPhrases = _phrases.ToArray();
            RudePhrases = new string[] { "hand over" };
        }

        /// <summary>Returns a random snack as defined by <typeparamref name="TEnum"/></summary>
        public static TEnum Snack() => EnumUtils.Random<TEnum>(typeof(TEnum));
        /// <summary>Returns a random arrival message based off snack type</summary>
        public static string Arrival(TEnum snack) => $"{Faces[snack]} {ArrivalMessages[snack].Random()}";
        /// <summary>Returns a random departure message based off snack type</summary>
        public static string Departure(TEnum snack) => $"{Faces[snack]} {DepartureMessages[snack].Random()}";
        /// <summary>Returns a random departure with no people based off snack type</summary>
        public static string NoPeople(TEnum snack) => $"{Faces[snack]} {NoPeopleMessages[snack].Random()}";
        /// <summary>Returns a random give message based off snack type, username, and amount of snacks</summary>
        public static string Give(TEnum snack, string user, ulong amount)
            => string.Format($"{Faces[snack]} {GiveMessages[snack].Random()}", amount, user, Names[snack]);
        /// <summary>Returns a random rude message based off snack type and username</summary>
        public static string Greed(TEnum snack, string user)
            => string.Format($"{Faces[snack]} {GreedMessages[snack].Random()}", user, Names[snack]);
        /// <summary>Returns a random rude message based off snack type and username</summary>
        public static string Rude(TEnum snack, string user)
            => string.Format($"{Faces[snack]} {RudeMessages[snack].Random()}", user, Names[snack]);

        /// <summary>Generates a random delay based on the <see cref="ISnacksChannelConfig"/> passed.</summary>
        public virtual ulong GenerateDelay(ISnacksChannelConfig config)
        {
            var variance = Random.Next(0, config.DelayVariance + 1);
            return Operations.PlusMinus(config.Delay, variance);
        }

        /// <summary>Generates a random duration based on the <see cref="ISnacksChannelConfig"/> passed.</summary>
        public virtual ulong GenerateDuration(ISnacksChannelConfig config)
        {
            var variance = Random.Next(0, config.Duration + 1);
            return Operations.PlusMinus(config.Duration, variance);
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
            var pot = config.EarlyBirdPot;
            var variance = Random.Next(0, config.EarlyBirdPotVariance);
            var users = (await channel.GetUsersAsync().FlattenAsync()).Where(user =>
            {
                return !user.IsBot && !user.IsWebhook
                        && user.Status != UserStatus.Offline
                        && user.Status != UserStatus.Invisible;
            });

            var earlyBirdPot = Operations.PlusMinus(pot, variance);

            return (ulong)users.LongCount() * earlyBirdPot;
        }
    }
}
