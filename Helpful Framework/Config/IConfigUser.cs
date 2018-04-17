using System;
using System.Collections.Generic;
using System.Text;

namespace Helpful.Framework.Config
{
    /// <summary>Represents a configuration user</summary>
    public interface IConfigUser
    {
        /// <summary>The ID of this user</summary>
        ulong Id { get; set; }
    }
}
