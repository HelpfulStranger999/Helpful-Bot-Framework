using System.Collections.Generic;

namespace Helpful.Framework.Config
{
    /// <summary>An extended representation of <see cref="IConfigGuild"/></summary>
    public interface IInviteGuild : IConfigGuild
    {
        /// <summary>Provides of mapping of user ID to users invited</summary>
        IDictionary<ulong, ulong> Invites { get; }
    }
}
