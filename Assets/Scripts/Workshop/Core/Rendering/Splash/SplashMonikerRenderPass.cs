// File: Assets/Scripts/Workshop/Core/Rendering/Splash/SplashMonikerRenderPass.cs
using UnityEngine;
using UnityEngine.Rendering;
using Workshop.Core.Memory;
using Workshop.MicroPackages.Providers;
using Workshop.Systems.MicroPackages;

namespace Workshop.Core.Rendering.Splash
{
    public class SplashMonikerRenderPass : ISingularityDrawPass
    {
        public ProviderType ProviderType => ProviderType.RenderPass;
        public int RenderQueue => 1000;

        // Satisfy the interface
        public int Id => 0;

        private Mesh[] _alphabetMeshes;
        private Material[] _renderStyles;
        private MaterialPropertyBlock _mpb;

        public SplashMonikerRenderPass(Mesh[] alphabet, Material[] styles)
        {
            _alphabetMeshes = alphabet ?? new Mesh[0];
            _renderStyles = styles ?? new Material[0];
            _mpb = new MaterialPropertyBlock();
        }

        public void ExecuteDraw(CommandBuffer cmd, DataWarehouse warehouse, Matrix4x4 viewMatrix, Matrix4x4 projMatrix)
        {
            var letters = warehouse.GetShelf<SplashLetterEntity>();
            if (letters == null) return;

            // Simple, standard manifestation loop
            for (int i = 0; i < 22; i++)
            {
                if (!letters.IsActive(i)) continue;
                ref var letter = ref letters.GetRef(i);

                Matrix4x4 trs = Matrix4x4.TRS(letter.Position, letter.Rotation, letter.Scale);
                var value = _alphabetMeshes.Length - 1;
                if(value < 0)
                {
                    //boo!  Now what? 
                    return;
                }
                int meshIdx = Mathf.Clamp(letter.MeshId, 0, value);
                int styleIdx = Mathf.Clamp(letter.MaterialId, 0, _renderStyles.Length - 1);

                if (_alphabetMeshes.Length > meshIdx && _alphabetMeshes[meshIdx] != null &&
                    _renderStyles.Length > styleIdx && _renderStyles[styleIdx] != null)
                {
                    // Pass the FactionColor to the shader via PropertyBlock
                    _mpb.Clear();
                    _mpb.SetColor("_Color", letter.FactionColor);

                    _mpb.SetFloat("_Metallic", 0.85f);
                    _mpb.SetFloat("_Glossiness", 0.75f);

                    // Safe emission math: Give it a baseline glow, and add to it as it scales
                    float pulse = Mathf.Abs(letter.Scale.x - 1.618f);
                    Color pulseEmission = letter.FactionColor * (0.8f + (pulse * 2.0f));
                    _mpb.SetColor("_EmissionColor", pulseEmission);

                    cmd.DrawMesh(_alphabetMeshes[meshIdx], trs, _renderStyles[styleIdx], 0, 0, _mpb);
                }
            }
        }

        public void Dispose()
        {
            // Nothing unmanaged to release yet
        }
    }
}