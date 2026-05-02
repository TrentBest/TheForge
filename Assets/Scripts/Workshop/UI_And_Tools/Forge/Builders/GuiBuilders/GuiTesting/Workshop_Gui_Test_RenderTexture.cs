using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.GuiTesting
{
    public class Workshop_Gui_Test_TexturePreview : IGuiProvider
    {
        private RenderTexture _renderTexture;
        private TexturePreviewBuilder _testPreview;
        private List<Texture> _availableTextures;

        public string Title { get; private set; }

        public Workshop_Gui_Test_TexturePreview()
        {
            // 1. Scan for all textures in the Resources/Textures folder
            _availableTextures = Resources.LoadAll<Texture>("Textures").ToList();

            if (_availableTextures.Count == 0)
            {
                Debug.LogError("[Workshop Test] No textures found in Resources/Textures/");
                Title = "TEST: Error - No Textures";
                return;
            }

            // 2. Initial Setup with the first found texture
            var initial = _availableTextures[0];
            _renderTexture = new RenderTexture(initial.width, initial.height, 0);
            Graphics.Blit(initial, _renderTexture);

            _testPreview = new TexturePreviewBuilder(_renderTexture);
            Title = "TEST: Texture Browser";
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new VisualElement { style = { flexGrow = 1 } };

            // --- 1. Top Toolbar for Selection ---
            var toolbar = new VisualElement
            {
                style = {
                    flexDirection = FlexDirection.Row,
                    paddingBottom = 5,
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                    borderBottomWidth = 1,
                    borderBottomColor = Color.black
                }
            };

            var dropdown = new DropdownField("Select Texture",
                _availableTextures.Select(t => t.name).ToList(), 0);

            dropdown.style.flexGrow = 1;
            dropdown.RegisterValueChangedCallback(evt => OnTextureSelected(evt.newValue));

            toolbar.Add(dropdown);
            root.Add(toolbar);

            // --- 2. The Interactive Preview ---
            // Use the public preview's GUI
            var previewContent = _testPreview.CreateGui(ctx);
            previewContent.style.flexGrow = 1;
            root.Add(previewContent);

            return root;
        }

        private void OnTextureSelected(string textureName)
        {
            var tex = _availableTextures.FirstOrDefault(t => t.name == textureName);
            if (tex != null)
            {
                // Ensure the RT matches the new texture dimensions if they changed
                if (_renderTexture.width != tex.width || _renderTexture.height != tex.height)
                {
                    _renderTexture.Release();
                    _renderTexture.width = tex.width;
                    _renderTexture.height = tex.height;
                    _renderTexture.Create();
                }

                // Blit the new pixel data into the same RT being watched by the Builder
                Graphics.Blit(tex, _renderTexture);

                // Signal the context that we've changed data
                _testPreview.GetContext().IsDirty = true;
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}