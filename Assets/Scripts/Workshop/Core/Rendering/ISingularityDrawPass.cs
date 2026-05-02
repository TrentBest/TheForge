// File: Assets/Scripts/Workshop/MicroPackages/Providers/ISingularityDrawPass.cs
using UnityEngine;
using UnityEngine.Rendering;
using Workshop.Core.Memory;
using Workshop.Systems.MicroPackages;

namespace Workshop.MicroPackages.Providers
{
    // Define the contract so the pipeline can interface with it
    public interface ISingularityDrawPass : IProvider
    {
        int RenderQueue { get; }
        void ExecuteDraw(CommandBuffer cmd, DataWarehouse warehouse, Matrix4x4 viewMatrix, Matrix4x4 projMatrix);
        void Dispose();
    }
}