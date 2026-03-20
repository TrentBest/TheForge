using System.Text;
using UnityEngine;
using Unity;

namespace TheSingularityWorkshop.MicroPackages
{
    public interface IProvider
    {
        int Id { get; }

        ProviderType ProviderType { get; }
    }
}
