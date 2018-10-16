namespace Helpful.Framework.Config
{
    /// <summary>Represents a snacks channel configuration</summary>
    public interface ISnacksChannelConfig
    {
        /// <summary>The channel ID</summary>
        ulong Id { get; }
        /// <summary>Upper-bound of how much to give out</summary>
        ulong Amount { get; set; }
        /// <summary>How long the event should last approximately</summary>
        ulong Duration { get; set; }
        /// <summary>How much the <see cref="Duration"/> should vary</summary>
        ulong DurationVariance { get; set; }
        /// <summary>How many messages must be sent by humans before the event begins</summary>
        ulong MessagesRequired { get; set; }
        /// <summary>How long after the <see cref="MessagesRequired"/> precondition the event should begin</summary>
        ulong Delay { get; set; }
        /// <summary>How much the <see cref="Duration"/> should vary</summary>
        ulong DelayVariance { get; set; }
        /// <summary>The base number of snacks the early bird pot should hold</summary>
        ulong EarlyBirdPot { get; set; }
        /// <summary>How much the <see cref="EarlyBirdPot"/> should vary</summary>
        ulong EarlyBirdPotVariance { get; set; }
        /// <summary>How many users qualify for the <see cref="EarlyBirdPot"/></summary>
        ulong EarlyBirdPotSize { get; set; }
        /// <summary>Whether bots can get an early bird pot bonus</summary>
        bool BotEarlyBirdBot { get; set; }
    }
}
