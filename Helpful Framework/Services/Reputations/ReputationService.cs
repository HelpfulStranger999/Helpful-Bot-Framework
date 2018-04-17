using Helpful.Framework.Config;
using System;
using System.Linq;
using System.Threading.Tasks;
using Reputations = System.Collections.Generic.Dictionary<ulong, System.DateTimeOffset[]>;

namespace Helpful.Framework.Services
{
    /// <summary>Provides a default service for reputation commands</summary>
    public class ReputationService<TConfig, TGuild, TUser> : IService<TConfig, TGuild, TUser>
        where TConfig : class, IConfig<TGuild, TUser>
        where TGuild : class, IConfigGuild
        where TUser : class, IConfigUser, IReputation
    {
        /// <summary>The configuration object to use</summary>
        protected TConfig Config { get; }

        /// <summary>The cooldown between reputations</summary>
        protected TimeSpan Cooldown { get; } = TimeSpan.FromDays(1);
        /// <summary>The number of reputation points available per cooldown period</summary>
        protected int RepsCount { get; } = 1;
        /// <summary>A mapping of user id and next reputation</summary>
        protected Reputations Reputations { get; } = new Reputations();

        /// <summary>Instantiates a new reputation service</summary>
        /// <param name="config">The implementation of <see cref="IConfig{IGuild, IUser}"/></param>
        /// <param name="cooldown">The cooldown between reputations. Default to one day.</param>
        /// <param name="reps">The number of reputation points available per user per cooldown period</param>
        public ReputationService(TConfig config, TimeSpan cooldown = default(TimeSpan), int reps = 1)
        {
            Config = config;
            RepsCount = reps;
            Cooldown = cooldown == default(TimeSpan) ? 
                TimeSpan.FromDays(1) : cooldown;
        }

        /// <summary>Returns whether this user can give a reputation point</summary>
        public bool CanRep(TUser user) => CanRep(user.Id);
        /// <summary>Returns whether this user can give a reputation point</summary>
        public bool CanRep(ulong id) => AvailableReputation(id) >= 1;

        /// <summary>Returns the next time when this user can give a reputation point</summary>
        public DateTimeOffset NextReputation(TUser user) => NextReputation(user.Id);
        /// <summary>Returns the next time when this user can give a reputation point</summary>
        public DateTimeOffset NextReputation(ulong id)
        {
            return GetOrCreateUser(id).OrderBy(time => time.ToUnixTimeMilliseconds()).First();
        }

        /// <summary>Returns the number of currently available reputation points.</summary>
        public int AvailableReputation(TUser user) => AvailableReputation(user.Id);
        /// <summary>Returns the number of currently available reputation points.</summary>
        public int AvailableReputation(ulong id)
        {
            var now = DateTimeOffset.Now;
            return GetOrCreateUser(id).Count(time => time <= now);
        }

        /// <summary>Gives</summary>
        /// <returns></returns>
        public async Task<ReputationResult> RepUser(ulong user, ulong userRepped)
        {
            if (CanRep(user))
            {
                Config.Users[userRepped].Reputation++;
                await Config.WriteAsync(DatabaseType.User);
                var nextRep = DateTimeOffset.Now.Add(Cooldown);
                UpdateUser(user, nextRep);
                return ReputationResult.FromSuccess(nextRep);
            }

            return ReputationResult.FromError(NextReputation(user));
        }

        /// <summary>Updates the cooldown for the specified user</summary>
        protected internal void UpdateUser(ulong id, DateTimeOffset reputation)
        {
            var reps = Reputations[id];
            for (var i = 0; i < reps.Count(); i++)
            {
                var rep = reps[i];
                if (reputation >= rep)
                {
                    Reputations[id][i] = reputation;
                    return;
                }
            }
        }

        /// <summary>Returns the cooldowns for the specified user</summary>
        protected internal DateTimeOffset[] GetOrCreateUser(ulong id)
        {
            if (Reputations.ContainsKey(id))
            {
                return Reputations[id];
            }

            Reputations.Add(id, GenerateBaseTimes());
            return Reputations[id];
        }

        /// <summary>Returns the base times for next reputations</summary>
        protected internal DateTimeOffset[] GenerateBaseTimes()
        {
            var times = new DateTimeOffset[RepsCount];
            for (var i = 0; i < RepsCount; i++)
                times[i] = DateTimeOffset.Now;
            return times;
        }

        /// <inheritdoc />
        public bool CanDisconnect(FrameworkBot<TConfig, TGuild, TUser> bot) => true;
        /// <inheritdoc />
        public Task Disconnect(FrameworkBot<TConfig, TGuild, TUser> bot) => Task.CompletedTask;
    }
}
