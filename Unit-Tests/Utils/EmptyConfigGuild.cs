using Helpful.Framework.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Unit_Tests.Utils
{
    internal class EmptyConfigGuild : IConfigGuild, ISpawnerGuild
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public double Frequency { get; set; } = 0.04;
        public ulong Duration { get; set; } = 30_000;
        public IList<ulong> CreatureChannels { get; set; } = new List<ulong>();
    }
}
