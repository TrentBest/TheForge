using System.Text;
using UnityEngine;
using Unity;

namespace Assets.Scripts.MicroPackages
{
    public interface IProvider
    {
        int Id { get; }

        ProviderType ProviderType { get; }
    }
}
