using Helpful.Framework.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Unit_Tests.Utils;

namespace Unit_Tests
{
    [TestClass]
    public class ReputationTests
    {
        internal ReputationService<EmptyConfig, EmptyConfigGuild, EmptyConfigUser, EmptyContext> Service { get; } 
            = new ReputationService<EmptyConfig, EmptyConfigGuild, EmptyConfigUser, EmptyContext>(null, reps: 1_000_000);

        [TestMethod]
        public void CanRepTest()
        {
            Assert.IsTrue(Service.CanRep(0));
        }

    }
}
