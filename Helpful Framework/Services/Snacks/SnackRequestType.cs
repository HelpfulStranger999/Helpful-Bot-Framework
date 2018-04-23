namespace Helpful.Framework.Services
{
    /// <summary>Specifies what type of snack request a message is</summary>
    public enum SnackRequestType
    {
        /// <summary>The message was not a snack request.</summary>
        None = 0,
        /// <summary>The message was a normal snack request.</summary>
        Request = 1,
        /// <summary>The message was a rude snack request.</summary>
        Rude = 2,
        /// <summary>The message was a greedy snack request.</summary>
        Greedy = 3,
        /// <summary>The message could not be parsed to a snack request type</summary>
        Unknown = 4
    }
}
