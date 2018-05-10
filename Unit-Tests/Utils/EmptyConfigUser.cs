using Helpful.Framework.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Unit_Tests.Utils
{
    internal class EmptyConfigUser : IConfigUser, IReputation
    {
        public ulong Reputation { get; set; }

        public ulong Id { get; set; }
    }
}
