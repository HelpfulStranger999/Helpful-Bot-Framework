namespace Helpful.Framework.Config
{
    /// <summary>An extended representation of <see cref="IConfigUser"/></summary>
    public interface IReputation : IConfigUser
    {
        /// <summary>How many reputation points this user has</summary>
        ulong Reputation { get; set; }
    }
}
