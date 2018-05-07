namespace Helpful.Framework.Config
{
    /// <summary>An extended representation of <see cref="IConfigUser"/></summary>
    public interface ISpawnerUser : IConfigUser
    {
        /// <summary>How many creatures this user has</summary>
        ulong Creatures { get; set; }
    }
}
