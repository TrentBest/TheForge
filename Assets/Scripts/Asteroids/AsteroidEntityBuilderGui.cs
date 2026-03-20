#if UNITY_EDITOR
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Asteroids.Editor
{
    internal class AsteroidEntityBuilderGui : IGuiProvider
    {
        public string Title => "ASTEROID TEMPLATE EDITOR";

        private GuiContext _lastCtx;
        private CRUD_Builder<AsteroidContext> _crudInterface;

        // Mock static database for templates until you have a central Asteroid Registry
        private static List<AsteroidContext> _templateDatabase = new List<AsteroidContext>
        {
            new AsteroidContext { Name = "Large Cratered", SizeTier = 3, PointValue = 100, RotationSpeed = 45f },
            new AsteroidContext { Name = "Medium Fragment", SizeTier = 2, PointValue = 250, RotationSpeed = 90f },
            new AsteroidContext { Name = "Small Shard", SizeTier = 1, PointValue = 500, RotationSpeed = 180f }
        };

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _crudInterface = new CRUD_Builder<AsteroidContext>(
                title: "ASTEROID TEMPLATES DB",

                dataSource: () => _templateDatabase,

                getDisplayName: (asteroid) => string.IsNullOrEmpty(asteroid.Name) ? "Unnamed Asteroid" : asteroid.Name,

                // Group by size tier so you easily find your templates
                getGroupCategory: (asteroid) => $"Size Tier {asteroid.SizeTier}",

                buildEditorForm: (asteroid) =>
                {
                    var form = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.FlexStart } };

                    // --- LEFT SIDE: PROCEDURAL VECTOR PREVIEW ---
                    var previewCol = new VisualElement { style = { width = 180, marginRight = 20, alignItems = Align.Center } };

                    var vectorPreview = new AsteroidVectorPreview(asteroid)
                    {
                        style = {
                            width = 160, height = 160,
                            backgroundColor = new Color(0.05f, 0.05f, 0.08f), // Deep space bg
                            borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2,
                            borderTopColor = Color.cyan, borderBottomColor = Color.cyan, borderLeftColor = Color.cyan, borderRightColor = Color.cyan,
                            marginBottom = 10
                        }
                    };

                    previewCol.Add(vectorPreview);
                    previewCol.Add(new Label("VECTOR PREVIEW") { style = { color = Color.gray, fontSize = 10, marginBottom = 2, unityFontStyleAndWeight = FontStyle.Bold } });
                    form.Add(previewCol);

                    // --- RIGHT SIDE: METADATA FIELDS ---
                    var fieldsCol = new VisualElement { style = { flexGrow = 1 } };

                    var nameField = new TextField("Template Name") { value = asteroid.Name, style = { marginBottom = 10 } };
                    nameField.Q<Label>().style.minWidth = 150; nameField.Q<Label>().style.color = Color.gray;
                    nameField.RegisterValueChangedCallback(e => {
                        asteroid.Name = e.newValue;
                        vectorPreview.MarkDirtyRepaint(); // Force redraw if name changes (changes random seed)
                    });
                    fieldsCol.Add(nameField);

                    var tierField = new IntegerField("Size Tier (1-3)") { value = asteroid.SizeTier, style = { marginBottom = 10 } };
                    tierField.Q<Label>().style.minWidth = 150; tierField.Q<Label>().style.color = Color.gray;
                    tierField.RegisterValueChangedCallback(e => {
                        asteroid.SizeTier = Mathf.Clamp(e.newValue, 1, 3);
                        vectorPreview.MarkDirtyRepaint(); // Redraw size
                    });
                    fieldsCol.Add(tierField);

                    var pointsField = new IntegerField("Point Value") { value = asteroid.PointValue, style = { marginBottom = 10 } };
                    pointsField.Q<Label>().style.minWidth = 150; pointsField.Q<Label>().style.color = Color.gray;
                    pointsField.RegisterValueChangedCallback(e => asteroid.PointValue = e.newValue);
                    fieldsCol.Add(pointsField);

                    var rotSpeedField = new FloatField("Base Rotation Speed") { value = asteroid.RotationSpeed, style = { marginBottom = 20 } };
                    rotSpeedField.Q<Label>().style.minWidth = 150; rotSpeedField.Q<Label>().style.color = Color.gray;
                    rotSpeedField.RegisterValueChangedCallback(e => asteroid.RotationSpeed = e.newValue);
                    fieldsCol.Add(rotSpeedField);

                    form.Add(fieldsCol);
                    return form;
                },

                onSave: (asteroid) => {
                    if (!_templateDatabase.Contains(asteroid))
                    {
                        _templateDatabase.Add(asteroid);
                    }
                    // TODO: Replace with your actual persistence logic (JSON/ScriptableObject/Registry)
                    Debug.Log($"[Asteroids DB] Saved template: {asteroid.Name}");
                },

                onDelete: (asteroid) => {
                    _templateDatabase.Remove(asteroid);
                    Debug.Log($"[Asteroids DB] Deleted template: {asteroid.Name}");
                },

                getSubtitle: (asteroid) => $"Points: {asteroid.PointValue} | Rot: {asteroid.RotationSpeed}/s"
            );

            return _crudInterface.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }

    // --- PROCEDURAL ASTEROID RENDERER ---
    public class AsteroidVectorPreview : VisualElement
    {
        private AsteroidContext _context;

        public AsteroidVectorPreview(AsteroidContext context)
        {
            _context = context;
            generateVisualContent += OnGenerateVisualContent;
        }

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            float width = layout.width;
            float height = layout.height;
            if (width <= 0 || height <= 0) return;

            var painter = ctx.painter2D;
            Vector2 center = new Vector2(width / 2f, height / 2f);

            // Radius scales based on Size Tier
            float baseRadius = 20f * _context.SizeTier;

            painter.strokeColor = Color.white;
            painter.fillColor = Color.black;
            painter.lineWidth = 2f;
            painter.BeginPath();

            // We use the Name's HashCode as a random seed.
            // This ensures the asteroid always looks identical when re-selecting it in the list,
            // giving the illusion of a uniquely saved shape!
            UnityEngine.Random.InitState(_context.Name?.GetHashCode() ?? 0);

            int points = 8 + (_context.SizeTier * 2); // Larger asteroids get more jagged points
            for (int i = 0; i <= points; i++)
            {
                float angle = (i * 360f / points) * Mathf.Deg2Rad;
                float rOffset = baseRadius + UnityEngine.Random.Range(-baseRadius * 0.25f, baseRadius * 0.25f);
                Vector2 pt = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * rOffset;

                if (i == 0) painter.MoveTo(pt);
                else painter.LineTo(pt);
            }

            painter.Fill();
            painter.Stroke();

            // Restore the random state clock so we don't accidentally freeze other Unity randomization
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        }
    }
}
#endif