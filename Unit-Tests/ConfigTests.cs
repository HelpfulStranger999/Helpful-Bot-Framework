using Helpful.Framework;
using Helpful.Framework.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Linq;

namespace Unit_Tests
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void SerializerTest()
        {
            var config = new FrameworkBotConfig();
            var json = config.Serialize(settings =>
            {
                settings.Formatting = Formatting.Indented;
            });

            Assert.IsTrue(json.Split('\n').Count() <= 1000);
        }
    }
}
