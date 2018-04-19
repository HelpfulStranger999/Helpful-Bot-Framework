using System.Collections.Generic;

namespace Helpful.Framework.Config
{
    /// <summary>An extended representation of <see cref="IConfigGuild"/></summary>
    public interface ISpawnerGuild : IConfigGuild
    {
        /// <summary>How often a creature should wander by</summary>
        double Frequency { get; set; }
        /// <summary>How long each creature should last before wandering away</summary>
        ulong Duration { get; set; }
        /// <summary>A list of channel IDs creatures can spawn in</summary>
        IList<ulong> Channels { get; set; }
    }
}
