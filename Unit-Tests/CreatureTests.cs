using Helpful.Framework.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Unit_Tests.Utils;

namespace Unit_Tests
{
    [TestClass]
    public class CreateureTests
    {
        internal const ulong Channel = 348489498079920141ul;
        internal CreatureSpawnerService<EmptyConfig, EmptyConfigGuild, EmptyConfigUser, EmptyContext> Service =
            new CreatureSpawnerService<EmptyConfig, EmptyConfigGuild, EmptyConfigUser, EmptyContext>();

        [TestMethod]
        public void AnyLooseTest()
        {
            Assert.IsFalse(Service.AnyLoose(Channel));
        }

        [TestMethod]
        public void CanSpawnTest()
        {
            Assert.IsTrue(Service.CanSpawn(Channel));
        }

    }
}
