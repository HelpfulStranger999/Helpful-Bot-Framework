using System;

namespace Helpful.Framework.Services
{
    /// <summary>The result of a reputation attempt</summary>
    public sealed class ReputationResult
    {
        /// <summary>When the user can rep next.</summary>
        public DateTimeOffset NextReputation { get; }
        /// <summary>Whether the user successfully repped another user.</summary>
        public bool IsSuccess { get; }

        private ReputationResult(bool success, DateTimeOffset nextRep)
        {
            IsSuccess = success;
            NextReputation = nextRep;
        }

        /// <summary>Generates a new successful reputation result</summary>
        public static ReputationResult FromSuccess(DateTimeOffset nextReputation)
            => new ReputationResult(true, nextReputation);

        /// <summary>Generates a new unsuccessful reputation result</summary>
        public static ReputationResult FromError(DateTimeOffset nextReputation)
            => new ReputationResult(false, nextReputation);
    }
}
