using Discord.Net.Rest;
using Discord.Net.Udp;
using Discord.Net.WebSockets;
using HelpfulUtilities.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace Helpful.Framework.Json
{
    internal class ConfigContractResolver : DefaultContractResolver
    {
        public static readonly ConfigContractResolver Instance = new ConfigContractResolver();

        public ConfigContractResolver()
        {
            IgnoreSerializableInterface = true;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            if (typeof(MulticastDelegate).IsAssignableFrom(member.DeclaringType)) return null;
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            
            if ((property.PropertyType.Extends(typeof(WebSocketProvider)) && property.PropertyName == "WebSocketProvider")
                || (property.PropertyType.Extends(typeof(UdpSocketProvider)) && property.PropertyName == "UdpSocketProvider")
                || (property.PropertyType.Extends(typeof(RestClientProvider)) && property.PropertyName == "RestClientProvider"))
            {
                property.Ignored = true;
            }

            return property;
        }
    }
}
