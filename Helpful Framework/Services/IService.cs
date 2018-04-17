using Helpful.Framework.Config;
using System.Threading.Tasks;

namespace Helpful.Framework
{
    /// <summary>Basic interface services should extend</summary>
    public interface IService<TConfig, TGuild, TUser>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild
        where TUser : class, IConfigUser
    {
        /// <summary>Returns whether the service is ready to disconnect at that moment.</summary>
        /// <param name="bot">The bot instance for the necessary operations.</param>
        bool CanDisconnect(FrameworkBot<TConfig, TGuild, TUser> bot);
        
        /// <summary>Asynchronously disconnects the service.</summary>
        /// <param name="bot">The bot instance for any necessary operations</param>
        Task Disconnect(FrameworkBot<TConfig, TGuild, TUser> bot);
    }
}
