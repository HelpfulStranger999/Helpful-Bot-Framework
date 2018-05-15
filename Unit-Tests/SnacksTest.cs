using Helpful.Framework.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Unit_Tests.Utils;

namespace Unit_Tests
{
    [TestClass]
    public class SnacksTests
    {
        internal const ulong Channel = 381889909113225237ul;
        internal SnacksService<EmptyConfig, EmptyConfigGuild, EmptyConfigUser, EmptyContext, EmptySnackSet> Service { get; }
            = new SnacksService<EmptyConfig, EmptyConfigGuild, EmptyConfigUser, EmptyContext, EmptySnackSet>();

        [TestMethod]
        public void CanStartTest()
        {
            Assert.IsTrue(Service.CanStartEvent(Channel));
        }

        [TestMethod]
        public void IsActiveTest()
        {
            Assert.IsFalse(Service.IsActive(Channel));
        }

        [TestMethod]
        public void IsBegunTest()
        {
            Assert.IsFalse(Service.IsStarted(Channel));
        }

    }
}
