using System;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.IO
{
    public interface IForgeFileBrowser
    {
        void OpenImageFile(Action<Texture2D> onTextureLoaded, Action onCanceled = null);
    }

    // The temporary band-aid for your current Image Editor
    public class EditorNativeFileBrowser : IForgeFileBrowser
    {
        public void OpenImageFile(Action<Texture2D> onTextureLoaded, Action onCanceled = null)
        {
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
            if (string.IsNullOrEmpty(path))
            {
                onCanceled?.Invoke();
                return;
            }

            byte[] fileData = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); // Auto-resizes the texture dimensions
            onTextureLoaded?.Invoke(tex);
#else
            Debug.LogError("[Forge] Native OS file browser is not available at runtime. Awaiting the Singularity File Browser module!");
            onCanceled?.Invoke();
#endif
        }
    }
}