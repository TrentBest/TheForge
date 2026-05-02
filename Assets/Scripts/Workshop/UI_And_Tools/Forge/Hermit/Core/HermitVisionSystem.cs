using UnityEngine;
using UnityEngine.Rendering;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{
    /// <summary>
    /// Pure Static Vision Shunt. 
    /// Captures 256x256 Stereo data and pushes it to the unmanaged Warehouse.
    /// </summary>
    public static class HermitVisionSystem
    {
        private static readonly Vector2Int Res = new Vector2Int(256, 256);
        private static RenderTexture _lRT, _rRT;
        private static Camera _slaveCam;
        private static byte[] _buffer;
        private static bool _init;

        public static void Initialize()
        {
            if (_init) return;

            _lRT = new RenderTexture(Res.x, Res.y, 24, RenderTextureFormat.ARGB32);
            _rRT = new RenderTexture(Res.x, Res.y, 24, RenderTextureFormat.ARGB32);

            // Create the hidden eyes
            var go = new GameObject("HERMIT_EYES_VIGNETTE");
            go.hideFlags = HideFlags.HideAndDontSave;
            _slaveCam = go.AddComponent<Camera>();
            _slaveCam.enabled = false;
            _slaveCam.fieldOfView = 90f;
            _slaveCam.cullingMask = 1 << LayerMask.NameToLayer("Ontology");

            _buffer = new byte[Res.x * Res.y * 4 * 2];
            _init = true;
        }

        public static void Tick(ref HermitChassisData data)
        {
            if (!_init) return;

            Vector3 offset = data.Rotation * Vector3.right * 0.032f;

            // Manual Render Sequence
            _slaveCam.transform.SetPositionAndRotation(data.Position - offset, data.Rotation);
            _slaveCam.targetTexture = _lRT;
            _slaveCam.Render();

            _slaveCam.transform.position = data.Position + offset;
            _slaveCam.targetTexture = _rRT;
            _slaveCam.Render();

            CaptureToBus();
        }

        private static void CaptureToBus()
        {
            // Async extraction to avoid stalling the Sovereign Heartbeat
            AsyncGPUReadback.Request(_lRT, 0, reqL => {
                if (reqL.hasError) return;
                System.Buffer.BlockCopy(reqL.GetData<byte>().ToArray(), 0, _buffer, 0, 65536 * 4);

                AsyncGPUReadback.Request(_rRT, 0, reqR => {
                    if (reqR.hasError) return;
                    System.Buffer.BlockCopy(reqR.GetData<byte>().ToArray(), 0, _buffer, 65536 * 4, 65536 * 4);

                    // Transmit to NNs via the SingularityDataBus
                    SingularityDataBus.Instance.SendLocal("Neural_Vision_In", _buffer);
                });
            });
        }

        /// <summary>
        /// Explicit cleanup for the SingularityBootloader shutdown sequence.
        /// </summary>
        public static void Dispose()
        {
            if (!_init) return;

            if (_lRT != null) { _lRT.Release(); Object.DestroyImmediate(_lRT); }
            if (_rRT != null) { _rRT.Release(); Object.DestroyImmediate(_rRT); }
            if (_slaveCam != null) Object.DestroyImmediate(_slaveCam.gameObject);

            _buffer = null;
            _init = false;
        }
    }
}