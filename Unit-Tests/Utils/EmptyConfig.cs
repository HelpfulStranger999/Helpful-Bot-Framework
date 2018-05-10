using Discord;
using Helpful.Framework.Config;
using Helpful.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Unit_Tests.Utils
{
    internal class EmptyConfig : IConfig<EmptyConfigGuild, EmptyConfigUser>
    {
        public IDictionary<ulong, EmptyConfigGuild> Guilds { get; set; } = new Dictionary<ulong, EmptyConfigGuild>();
        public IDictionary<ulong, EmptyConfigUser> Users { get; set; } = new Dictionary<ulong, EmptyConfigUser>();

        public Task Connect()
        {
            throw new NotImplementedException();
        }

        public Task<EmptyConfigGuild> Create(IGuild guild)
        {
            throw new NotImplementedException();
        }

        public Task<EmptyConfigUser> Create(IUser user)
        {
            throw new NotImplementedException();
        }

        public Task Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task Reconnect()
        {
            throw new NotImplementedException();
        }

        public Task<ResultOperation> WriteAsync(DatabaseType type)
        {
            throw new NotImplementedException();
        }
    }
}
