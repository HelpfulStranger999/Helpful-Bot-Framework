using Helpful.Framework.Json;
using Newtonsoft.Json;
using System.Linq;
using Xunit;

namespace Helpful.Framework.Tests
{
    public class ConfigTests
    {
        [Fact]
        public void Serializer()
        {
            var config = new FrameworkBotConfig();
            var json = config.Serialize(settings =>
            {
                settings.Formatting = Formatting.Indented;
            });

            Assert.InRange(json.Split('\n').Count(), 0, 1000);
        }
    }
}
