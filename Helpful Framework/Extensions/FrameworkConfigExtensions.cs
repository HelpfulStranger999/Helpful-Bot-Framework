using Newtonsoft.Json;
using System;

namespace Helpful.Framework.Json
{
    /// <summary>Provides extensions method for <see cref="FrameworkBotConfig"/></summary>
    public static class FrameworkConfigExtensions
    {
        /// <summary>Serializes this config into JSON with an optional factory for setting serializer settings.</summary>
        public static string Serialize(this FrameworkBotConfig config, Action<JsonSerializerSettings> factory = null)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = ConfigContractResolver.Instance
            };

            factory?.Invoke(settings);
            return JsonConvert.SerializeObject(config, settings);
        }
    }
}
