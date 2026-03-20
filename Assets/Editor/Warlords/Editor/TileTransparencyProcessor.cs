using UnityEngine;
using UnityEditor;
using System.IO;

namespace TheSingularityWorkshop.Warlords.Editor
{
    public class TileTransparencyProcessor : EditorWindow
    {
        private Color _colorToReplace = Color.white;
        private float _tolerance = 0.1f;
        private Texture2D _targetTexture;

        [MenuItem("TheSingularityWorkshop/Warlords/Tile Transparency Tool")]
        public static void ShowWindow() => GetWindow<TileTransparencyProcessor>("Tile Processor");

        private void OnGUI()
        {
            GUILayout.Label("Warlords Sprite Alpha Tool", EditorStyles.boldLabel);
            _targetTexture = (Texture2D)EditorField("Texture to Process", _targetTexture);
            _colorToReplace = EditorGUILayout.ColorField("Color to Remove", _colorToReplace);
            _tolerance = EditorGUILayout.Slider("Tolerance", _tolerance, 0f, 1f);

            if (GUILayout.Button("Process & Overwrite Asset") && _targetTexture != null)
            {
                ProcessTexture(_targetTexture);
            }
        }

        private void ProcessTexture(Texture2D tex)
        {
            // 1. Prepare the texture for reading
            string path = AssetDatabase.GetAssetPath(tex);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);

            bool wasReadable = importer.isReadable;
            importer.isReadable = true;
            importer.SaveAndReimport();

            // 2. Clone and Edit
            Texture2D newTex = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            Color[] pixels = tex.GetPixels();

            int groundHeight = 256;
            int totalHeight = tex.height; // 384

            for (int y = 0; y < totalHeight; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    int index = y * tex.width + x;

                    // ONLY process if we are above the 256x256 base
                    if (y >= groundHeight)
                    {
                        if (IsColorMatch(pixels[index], _colorToReplace))
                        {
                            pixels[index] = new Color(0, 0, 0, 0); // Transparent
                        }
                    }
                }
            }

            newTex.SetPixels(pixels);
            newTex.Apply();

            // 3. Save it back to disk
            byte[] bytes = newTex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            // 4. Reset Importer Settings
            importer.isReadable = wasReadable;
            importer.alphaIsTransparency = true; // Ensure Unity knows to use the alpha
            importer.SaveAndReimport();

            AssetDatabase.Refresh();
            Debug.Log($"Processed {tex.name}. The 'Sky' area is now transparent!");
        }

        private bool IsColorMatch(Color c1, Color c2)
        {
            return Mathf.Abs(c1.r - c2.r) < _tolerance &&
                   Mathf.Abs(c1.g - c2.g) < _tolerance &&
                   Mathf.Abs(c1.b - c2.b) < _tolerance;
        }

        private UnityEngine.Object EditorField(string label, UnityEngine.Object obj) =>
            EditorGUILayout.ObjectField(label, obj, typeof(Texture2D), false);
    }
}