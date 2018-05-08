using Helpful.Framework.Config;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Helpful.Framework.Services
{
    /// <summary>A snack event manager for <see cref="SnacksService{TConfig, TGuild, TUser, TCommandContext, TEnum}"/></summary>
    public class SnackEventManager<TEnum>
        where TEnum : Enum
    {
        // Queued
        /// <summary>How many messages have passed since the last event</summary>
        public ulong Messages { get; set; } = 0;

        // Running

        /// <summary>Whether the event has begun. <see cref="IsActive"/></summary>
        /// <remarks>This returns true when the event has begun. But it is active only after the 
        /// <see cref="ISnacksChannelConfig.Delay"/> and has arrived.</remarks>
        public bool HasBegun { get; set; } = false;
        /// <summary>Whether the event is active. <see cref="HasBegun"/></summary>
        public bool IsActive { get; set; } = false;

        /// <summary>The type of snack the event is currently running as</summary>
        public TEnum Snack { get; set; } = default;

        /// <summary>How much of the pot is left.</summary>
        public ulong Pot { get; set; } = 0;

        /// <summary>How many people have been given snacks so far</summary>
        public List<ulong> Users { get; } = new List<ulong>();

        /// <summary>The timer for ending the event</summary>
        public Timer EndTimer { get; set; }

        /// <summary>Resets the event manager after a recent event.</summary>
        public void Reset()
        {
            Messages = 0;
            Pot = 0;
            HasBegun = false;
            IsActive = false;
            Snack = default;
            Users.Clear();
        }
    }
}
