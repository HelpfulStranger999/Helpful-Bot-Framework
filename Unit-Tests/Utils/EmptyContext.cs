using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Unit_Tests.Utils
{
    internal class EmptyContext : ICommandContext
    {
        public IDiscordClient Client => null;
        public IGuild Guild => null;
        public IMessageChannel Channel => null;
        public IUser User => null;
        public IUserMessage Message => null;
    }
}
