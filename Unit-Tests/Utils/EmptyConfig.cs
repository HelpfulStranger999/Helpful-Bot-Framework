using Discord;
using Helpful.Framework.Config;
using Helpful.Framework.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Unit_Tests.Utils
{
    internal class EmptyConfig : IConfig<EmptyConfigGuild, EmptyConfigUser>
    {
        public ConcurrentDictionary<ulong, EmptyConfigGuild> Guilds { get; set; } = new ConcurrentDictionary<ulong, EmptyConfigGuild>();
        public ConcurrentDictionary<ulong, EmptyConfigUser> Users { get; set; } = new ConcurrentDictionary<ulong, EmptyConfigUser>();

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
            return Task.FromResult(ResultOperation.FromSuccess());
        }
    }
}
