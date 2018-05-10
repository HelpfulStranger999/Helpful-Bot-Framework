using Helpful.Framework.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Unit_Tests.Utils
{
    internal class EmptyConfigGuild : IConfigGuild
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
    }
}
