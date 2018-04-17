using System.Collections.Generic;

namespace Helpful.Framework.Config
{
    /// <summary>An extended representation of <see cref="IConfigGuild"/></summary>
    public interface ISnacksGuild : IConfigGuild
    {
        /// <summary>A mapping of channel ID to <see cref="ISnacksChannelConfig"/></summary>
        IDictionary<ulong, ISnacksChannelConfig> Snacks { get; }
    }
}
