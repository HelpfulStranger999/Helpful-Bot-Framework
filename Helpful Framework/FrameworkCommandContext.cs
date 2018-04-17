using Discord;
using Discord.Commands;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Helpful.Framework
{
    /// <summary>Provides an extended command context base.</summary>
    public abstract class FrameworkCommandContext<TClient> : ICommandContext
        where TClient : IDiscordClient
    {
        /// <summary>Your instance of <see cref="IDiscordClient"/></summary>
        public TClient Client { get; }

        /// <summary>The guild this command was run in.</summary>
        public IGuild Guild { get; }

        /// <summary>The text channel this command was run in.</summary>
        public ITextChannel TextChannel { get; }

        /// <summary>The channel this command was run in.</summary>
        public IMessageChannel Channel { get; }

        /// <summary>The user this command was run by.</summary>
        public IUser User { get; }

        /// <summary>The guild user this command was run by.</summary>
        public IGuildUser GuildUser { get; }

        /// <summary>The message of this command.</summary>
        public IUserMessage Message { get; }

        /// <summary>Constructs a new instance of this module base.</summary>
        public FrameworkCommandContext(IServiceProvider services, IUserMessage message)
        {
            Client = services.GetService<TClient>();
            Channel = message.Channel;
            TextChannel = message.Channel as ITextChannel;
            Guild = TextChannel?.Guild;
            User = message.Author;
            GuildUser = User as IGuildUser;
            Message = message;
        }

        IDiscordClient ICommandContext.Client => Client;
    }
}
