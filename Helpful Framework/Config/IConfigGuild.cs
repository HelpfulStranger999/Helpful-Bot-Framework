namespace Helpful.Framework.Config
{
    /// <summary>Represents a configuration guild</summary>
    public interface IConfigGuild
    {
        /// <summary>The ID of this guild</summary>
        ulong Id { get; }
        /// <summary>The prefix of this guild</summary>
        string Prefix { get; set; }
    }
}
