using Helpful.Framework.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Helpful.Framework.Config
{
    /// <summary>Represents a configuration manager</summary>
    public interface IConfig<TGuild, TUser> : IService
        where TGuild : IConfigGuild where TUser : IConfigUser
    {
        /// <summary>Provides a mapping of guild ID to the corresponding config object.</summary>
        IDictionary<ulong, TGuild> Guilds { get; set; }
        /// <summary>Provides a mapping of user ID to the corresponding config object.</summary>
        IDictionary<ulong, TUser> Users { get; set; }

        /// <summary>Asynchronously connects to the database</summary>
        Task Connect();
        /// <summary>Asynchronously disconnects from the database</summary>
        Task Disconnect();
        /// <summary>Asynchronously disconnects and reconnects to the database</summary>
        Task Reconnect();

        /// <summary>Asynchronously writes to the database</summary>
        /// <param name="type">The database type to write</param>
        Task<ResultOperation> WriteAsync(DatabaseType type);
    }
}
