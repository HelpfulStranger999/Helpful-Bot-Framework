using Discord;
using Discord.WebSocket;
using Helpful.Framework.Config;
using HelpfulUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Helpful.Framework.Services
{
    /// <summary>Provides a default service for seeing what invite a user joined with</summary>
    public class InviteService<TConfig, TGuild, TUser> : IService<TConfig, TGuild, TUser>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild, IInviteGuild
        where TUser : class, IConfigUser
    {
        /// <summary>The framework bot in use</summary>
        protected FrameworkBot<TConfig, TGuild, TUser> Bot { get; }
        /// <summary>Provides a list of all invites' metadata</summary>
        protected HashSet<IInviteMetadata> Invites { get; } = new HashSet<IInviteMetadata>();

        /// <summary>Instantiates a new <see cref="InviteService{TConfig, TGuild, TUser}"/></summary>
        public InviteService(FrameworkBot<TConfig, TGuild, TUser> client)
        {
            Bot = client;
            client.SocketClient.UserJoined += OnMemberJoin;
            client.Ready(LoadInvites);
        }

        private async Task LoadInvites()
        {
            foreach (var guild in Bot.SocketClient.Guilds.Where(g => g.CurrentUser.GuildPermissions.ManageGuild))
            {
                Invites.AddRange(await guild.GetInvitesAsync().ConfigureAwait(false));
            }
        }
        
        private async Task OnMemberJoin(SocketGuildUser user)
        {
            var cachedInvites = Invites.Where(i => i.GuildId == user.Guild.Id);
            var invites = await user.Guild.GetInvitesAsync().ConfigureAwait(false);
            foreach (var invite in invites)
            {
                var oldInvite = cachedInvites.FirstOrDefault(x => x.Code == invite.Code);
                if (oldInvite == null && invite.Uses == 1)
                {
                    Invites.Add(invite);
                    Bot.Configuration.Guilds[user.Guild.Id].Invites[invite.Inviter.Id]++;
                    await Bot.Configuration.WriteAsync(DatabaseType.Guild).ConfigureAwait(false);
                }
                else if (oldInvite.Uses < invite.Uses)
                {
                    Invites.Remove(oldInvite);
                    Invites.Add(invite);

                    Bot.Configuration.Guilds[user.Guild.Id].Invites[invite.Inviter.Id]++;
                    await Bot.Configuration.WriteAsync(DatabaseType.Guild).ConfigureAwait(false);
                }
                else
                {
                    continue;
                }
            }
        }

        /// <inheritdoc />
        public bool CanDisconnect(FrameworkBot<TConfig, TGuild, TUser> bot) => true;
        /// <inheritdoc />
        public async Task Disconnect(FrameworkBot<TConfig, TGuild, TUser> bot)
        {
            await Bot.Configuration.WriteAsync(DatabaseType.Guild).ConfigureAwait(false);
        }
    }
}
