using System;
using System.Collections.Generic;

namespace Helpful.Framework.Config
{
    /// <summary>An extended representation of <see cref="IConfigUser"/></summary>
    /// <typeparam name="TEnum">An enum defining the snack types</typeparam>
    public interface ISnacksUser<TEnum> : IConfigUser where TEnum : struct, IConvertible, IComparable, IFormattable
    {
        /// <summary>Maps each of the snack types defined in <typeparamref name="TEnum"/> to an ulong.</summary>
        IDictionary<TEnum, ulong> Snacks { get; set; }
    }
}
