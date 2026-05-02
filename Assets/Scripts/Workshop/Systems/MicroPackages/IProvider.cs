using System.Collections.Generic;
using System.Text;
using Unity;
using UnityEngine;

namespace Workshop.Systems.MicroPackages
{
    public interface IProvider
    {
        int Id { get; }

        ProviderType ProviderType { get; }
    }

    /// <summary>
    /// A strongly-typed provider that fulfills the contract of the base IProvider.
    /// </summary>
    public interface IProvider<T> : IProvider
    {
        /// <summary>
        /// Yields the specific instances of type T provided by this package.
        /// </summary>
        IEnumerable<T> Provide();
    }
}
