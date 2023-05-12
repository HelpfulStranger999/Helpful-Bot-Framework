using System;
using System.Collections.Generic;
using System.Linq;

namespace Helpful.Framework.Services
{
    /// <summary>Builds a collection of snack messages and phrases for <see cref="SnacksService{TConfig, TGuild, TUser, TCommandContext, TEnum}"/></summary>
    public class SnackMessageBuilder<TEnum> where TEnum : Enum
    {
        /// <summary>Default instance of <see cref="SnackMessageBuilder{TEnum}"/></summary>
        public static SnackMessageBuilder<TEnum> Instance { get; } = new SnackMessageBuilder<TEnum>().WithDefaults();

        /// <summary>A map of the snack type to the snacks name.</summary>
        public Dictionary<TEnum, string> Names { get; set; } = new Dictionary<TEnum, string>();
        /// <summary>A map of the snack type to the face for the snack.</summary>
        public Dictionary<TEnum, string> Faces { get; set; } = new Dictionary<TEnum, string>();

        /// <summary>A map of the snack type to an array of arrival messages.</summary>
        public Dictionary<TEnum, string[]> ArrivalMessages { get; set; } = new Dictionary<TEnum, string[]>();
        /// <summary>A map of the snack type to an array of departure messages.</summary>
        public Dictionary<TEnum, string[]> DepartureMessages { get; set; } = new Dictionary<TEnum, string[]>();
        /// <summary>A map of the snack type to an array of give messages.</summary>
        public Dictionary<TEnum, string[]> GiveMessages { get; set; } = new Dictionary<TEnum, string[]>();
        /// <summary>A map of the snack type to an array of greedy messages.</summary>
        public Dictionary<TEnum, string[]> GreedMessages { get; set; } = new Dictionary<TEnum, string[]>();
        /// <summary>A map of the snack type to an array of rude messages.</summary>
        public Dictionary<TEnum, string[]> RudeMessages { get; set; } = new Dictionary<TEnum, string[]>();
        /// <summary>A map of the snack type to an array of departure with no people messages.</summary>
        public Dictionary<TEnum, string[]> NoPeopleMessages { get; set; } = new Dictionary<TEnum, string[]>();
        /// <summary>A map of the snack type to an array of last second before departure messages.</summary>
        public Dictionary<TEnum, string[]> LastSecondMessages { get; set; } = new Dictionary<TEnum, string[]>();

        /// <summary>A map of the snack type to an array of agreement phrases</summary>
        public Dictionary<TEnum, string[]> AgreePhrases { get; set; } = new Dictionary<TEnum, string[]>();
        /// <summary>A map of the snack type to an array of rude phrases</summary>
        public Dictionary<TEnum, string[]> RudePhrases { get; set; } = new Dictionary<TEnum, string[]>();
        /// <summary>A map of the snack type to an array of greedy phrases</summary>
        public Dictionary<TEnum, string[]> GreedPhrases { get; set; } = new Dictionary<TEnum, string[]>();

        /// <summary>Sets default messages and phrases</summary>
        public SnackMessageBuilder<TEnum> WithDefaults()
        {
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

                AgreePhrases.Add(type, _agree_phrases);
                GreedPhrases.Add(type, _greed_phrases);
                RudePhrases.Add(type, _rude_phrases);
            }

            return this;
        }

        // Message

        private static readonly string[] _arrival = new string[]
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

        private static readonly string[] _departure = new string[]
        {
            "I'm out of snacks! I'll be back with more soon.",
            "I'm out of snacks :( I'll be back soon with more!",
            "Aight, I gotta head out! I'll be back with more, don worry :3",
            "Alright, I gotta get back to my errands. I'll see you guys soon!"
        };

        private static readonly string[] _give = new string[]
        {
            "Here ya go, {1:#}, here's {0:D} {2:#}!",
            "Alright here ya go, {1:#}, {0:D} {2:#} for you!",
            "Yeah! Here you go, {1:#}! {0:D} {2:#}!",
            "Of course {1:#}! Here's {0:D} {2:#}!",
            "Ok {1:#}, here's {0:D} {2:#} for you. Anyone else want some?",
            "Alllright, {0:D} {2:#} for {1:#}!",
            "Hold your horses {1:#}! Alright, {0:D} {2:#} for you :)"
        };

        private static readonly string[] _rude = new string[]
        {
            "Wow, you're rude. Have one {1:#}, {0:#}."
        };

        private static readonly string[] _greed = new string[]
        {
            "Don't be greedy now! you already got some {1:#} {0:#}!",
            "You already got your {1:#} {0:#}!",
            "Come on {0:#}, you already got your {1:#}! We gotta make sure there's some for errbody!"
        };

        private static readonly string[] _nopeople = new string[]
        {
            "I guess nobody wants snacks... more for me!",
            "Guess nobody's here.. I'll just head out then",
            "I don't see anybody.. <.< ... >.> ... All the snacks for me!!",
            "I guess nobody wants snacks huh.. Well, I'll come back later",
            "I guess i'll just come back later.."
        };

        private static readonly string[] _lastsec = new string[]
        {
            "Fine fine, {0:#}, I'll give you {1:D} of my on-the-road {2:#}.. Cya!",
            "Oh! {0:#}, you caught me right before I left! Alright, i'll give you {1:D} of my own {2:#}"
        };

        // Phrases

        private static readonly string[] _agree_phrases = new string[]
        {
            "holds out hand",
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
            "me want"
        };

        private static readonly string[] _greed_phrases = new string[]
        {
            "holds out hand",
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
            "me want",
            "more pl",
            "i have some more",
            "i want more",
            "i have another",
            "i have more",
            "more snack"
        };

        private static readonly string[] _rude_phrases = new string[]
        {
            "hand over"
        };
    }
}
