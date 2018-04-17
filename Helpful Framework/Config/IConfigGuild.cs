using System;
using System.Collections.Generic;
using System.Text;

namespace Helpful.Framework.Config
{
    /// <summary>Represents a configuration guild</summary>
    public interface IConfigGuild
    {
        /// <summary>The ID of this guild</summary>
        ulong Id { get; set; }
    }
}
