using Helpful.Framework.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Unit_Tests.Utils
{
    internal class EmptyConfigUser : IConfigUser, IReputation, ISpawnerUser, ISnacksUser<EmptySnackSet>
    {
        public ulong Reputation { get; set; }

        public ulong Id { get; set; }
        public ulong Creatures { get; set; }
        public IDictionary<EmptySnackSet, ulong> Snacks { get; } = new Dictionary<EmptySnackSet, ulong>();
    }
}
