using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.Architecture
{
    /// <summary>
    /// Represents a classification node for Digital Universe MicroPackages.
    /// </summary>
    public class OntologyNode
    {
        public string Term;
        public string Definition;

        // MicroPackage Architectural Metadata
        public string PackageDesignation; // e.g., "Core Engine", "Content Add-on", "Ruleset"
        public string ExecutionLayer;     // e.g., "Simulation Loop", "Render Pipeline", "Logic"
        public string Examples;

        public List<OntologyNode> Children = new List<OntologyNode>();

        public OntologyNode AddChild(OntologyNode child)
        {
            Children.Add(child);
            return this;
        }
    }

    public class Workshop_Gui_OntologyCore : IGuiProvider
    {
        public string Title => "Digital Universe Ontology";

        private OntologyNode _rootNode;
        private OntologyNode _inspectedNode = null;

        // UI Containers
        private VisualElement _rootContainer;
        private VisualElement _inspectorContainer;

        // Holographic Palette
        private readonly Color _holoBackground = new Color(0.02f, 0.02f, 0.05f, 0.95f);
        private readonly Color _holoAccent = new Color(0.7f, 0.2f, 1f, 1f);
        private readonly Color _holoText = new Color(0.8f, 0.8f, 1f, 1f);
        private readonly Color _holoDim = new Color(0.4f, 0.1f, 0.6f, 0.4f);

        public Workshop_Gui_OntologyCore()
        {
            _rootNode = UniverseOntologyData.GetPackageTaxonomy();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _rootContainer = new VisualElement { style = { flexGrow = 1, backgroundColor = _holoBackground } };
            RebuildUI();
            return _rootContainer;
        }

        private void RebuildUI()
        {
            _rootContainer.Clear();

            var mainLayoutBuilder = new GraphicalUserInterfaceBuilder("OntologyHoloLayout")
                .WithAutoGrow()
                .WithFlexLayout(FlexDirection.Row);

            // --- LEFT PANEL: THE HOLOGRAPHIC TREE ---
            var treePanelBuilder = new GraphicalUserInterfaceBuilder("TreeContainer")
                .WithFlexGrow(5)
                .WithPadding(20)
                .WithScrollable(true)
                .WithBorderRightWidth(2)
                .WithBorderRightColor(new Color(_holoAccent.r, _holoAccent.g, _holoAccent.b, 0.3f));

            var topBar = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 20, borderBottomWidth = 1, borderBottomColor = _holoAccent, paddingBottom = 5 } };

            var headerLabel = new Label("MICROPACKAGE ONTOLOGY :: ARCHITECTURE MAP")
            {
                style = { fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, color = _holoAccent, letterSpacing = 2 }
            };
            topBar.Add(headerLabel);
            treePanelBuilder.AddChild(topBar);

            // FIX: Explicitly align the tree content to the top to prevent Unity from centering it
            VisualElement treeRoot = new VisualElement { style = { justifyContent = Justify.FlexStart, flexGrow = 1 } };
            BuildTreeUI(_rootNode, treeRoot, 0);
            treePanelBuilder.AddChild(treeRoot);

            mainLayoutBuilder.AddChild(treePanelBuilder);

            // --- RIGHT PANEL: DATA INSPECTOR ---
            var inspectorPanelBuilder = new GraphicalUserInterfaceBuilder("InspectorWrapper")
                .WithWidth(400) // Widened slightly for the new data fields
                .WithPadding(20);

            inspectorPanelBuilder.AddChild(context =>
            {
                _inspectorContainer = new VisualElement { style = { flexGrow = 1, justifyContent = Justify.FlexStart } };
                return _inspectorContainer;
            });

            mainLayoutBuilder.AddChild(inspectorPanelBuilder);
            _rootContainer.Add(mainLayoutBuilder.Build());

            // FIX: Only default to the root node on the VERY FIRST load.
            if (_inspectedNode == null)
            {
                _inspectedNode = _rootNode;
            }
            RefreshInspector();
        }

        private void BuildTreeUI(OntologyNode node, VisualElement parentEl, int depth)
        {
            var nodeRow = new VisualElement
            {
                style = {
                    flexDirection = FlexDirection.Row,
                    paddingLeft = depth * 25,
                    marginTop = 4, marginBottom = 4,
                    alignItems = Align.Center
                }
            };

            if (depth > 0)
            {
                var connector = new VisualElement { style = { width = 15, height = 2, backgroundColor = _holoDim, marginRight = 5 } };
                nodeRow.Add(connector);
            }

            var nodeContainer = new VisualElement
            {
                style = {
                    backgroundColor = new Color(0, 0, 0, 0.5f),
                    borderLeftWidth = 3, borderLeftColor = _holoDim,
                    borderTopWidth = 1, borderBottomWidth = 1, borderRightWidth = 1,
                    borderTopColor = new Color(_holoDim.r, _holoDim.g, _holoDim.b, 0.1f),
                    borderBottomColor = new Color(_holoDim.r, _holoDim.g, _holoDim.b, 0.1f),
                    borderRightColor = new Color(_holoDim.r, _holoDim.g, _holoDim.b, 0.1f),
                    paddingTop = 6, paddingBottom = 6, paddingLeft = 12, paddingRight = 12,
                    borderTopRightRadius = 4, borderBottomRightRadius = 4
                }
            };

            var nodeLabel = new Label(node.Term) { style = { color = _holoText, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold } };
            nodeContainer.Add(nodeLabel);

            // Interaction Effects
            nodeContainer.RegisterCallback<MouseEnterEvent>(e => {
                nodeContainer.style.backgroundColor = new Color(_holoAccent.r, _holoAccent.g, _holoAccent.b, 0.2f);
                nodeContainer.style.borderLeftColor = _holoAccent;
                nodeContainer.style.scale = new StyleScale(new Vector2(1.02f, 1.02f));
            });

            nodeContainer.RegisterCallback<MouseLeaveEvent>(e => {
                if (_inspectedNode != node)
                {
                    nodeContainer.style.backgroundColor = new Color(0, 0, 0, 0.5f);
                    nodeContainer.style.borderLeftColor = _holoDim;
                }
                nodeContainer.style.scale = new StyleScale(new Vector2(1f, 1f));
            });

            // FIX: Safely assign the node and redraw to update highlights
            nodeContainer.RegisterCallback<ClickEvent>(e => {
                _inspectedNode = node;
                RebuildUI();
            });

            if (_inspectedNode == node)
            {
                nodeContainer.style.backgroundColor = new Color(_holoAccent.r, _holoAccent.g, _holoAccent.b, 0.3f);
                nodeContainer.style.borderLeftColor = _holoAccent;
                nodeLabel.style.color = Color.white;
            }

            nodeRow.Add(nodeContainer);
            parentEl.Add(nodeRow);

            if (node.Children.Count > 0)
            {
                var childrenContainer = new VisualElement { style = { borderLeftWidth = 1, borderLeftColor = new Color(_holoDim.r, _holoDim.g, _holoDim.b, 0.3f), marginLeft = (depth * 25) + 7 } };
                foreach (var child in node.Children)
                {
                    BuildTreeUI(child, childrenContainer, 0);
                }
                parentEl.Add(childrenContainer);
            }
        }

        private void RefreshInspector()
        {
            if (_inspectorContainer == null || _inspectedNode == null) return;
            _inspectorContainer.Clear();

            var scanline = new VisualElement { style = { height = 2, backgroundColor = _holoAccent, opacity = 0.5f, marginBottom = 10 } };
            _inspectorContainer.Add(scanline);

            var heroCard = new VisualElement
            {
                style = {
                    backgroundColor = new Color(_holoAccent.r, _holoAccent.g, _holoAccent.b, 0.1f),
                    borderTopWidth = 1, borderBottomWidth = 1,
                    borderTopColor = _holoAccent, borderBottomColor = _holoAccent,
                    paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15,
                    marginBottom = 20
                }
            };

            heroCard.Add(new Label("MICROPACKAGE TARGET:") { style = { fontSize = 10, color = _holoText, opacity = 0.5f, letterSpacing = 1 } });
            heroCard.Add(new Label($"[{_inspectedNode.Term.ToUpper()}]") { style = { fontSize = 22, unityFontStyleAndWeight = FontStyle.Bold, color = Color.white, marginTop = 2 } });

            _inspectorContainer.Add(heroCard);

            var contentBox = new VisualElement { style = { paddingLeft = 5 } };

            // Core Definition
            contentBox.Add(new Label("CONCEPTUAL DOMAIN") { style = { color = _holoAccent, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11, marginBottom = 5, letterSpacing = 1 } });
            contentBox.Add(new Label(_inspectedNode.Definition) { style = { color = _holoText, whiteSpace = WhiteSpace.Normal, fontSize = 13, marginBottom = 20, opacity = 0.9f } });

            // Architecture Metadata
            if (!string.IsNullOrEmpty(_inspectedNode.PackageDesignation))
            {
                var metaBox = new VisualElement { style = { backgroundColor = new Color(0, 0, 0, 0.4f), paddingBottom = 10, paddingTop = 10, paddingLeft = 10, paddingRight = 10, marginBottom = 20, borderLeftWidth = 2, borderLeftColor = _holoDim } };
                metaBox.Add(new Label("ARCHITECTURE PARAMETERS") { style = { color = _holoDim, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 10, marginBottom = 8, letterSpacing = 1 } });

                metaBox.Add(new Label($"Designation: <color=#{ColorUtility.ToHtmlStringRGBA(_holoText)}>{_inspectedNode.PackageDesignation}</color>") { style = { color = _holoAccent, fontSize = 12, marginBottom = 4 } });
                metaBox.Add(new Label($"Execution Layer: <color=#{ColorUtility.ToHtmlStringRGBA(_holoText)}>{_inspectedNode.ExecutionLayer}</color>") { style = { color = _holoAccent, fontSize = 12 } });
                contentBox.Add(metaBox);
            }

            if (!string.IsNullOrEmpty(_inspectedNode.Examples))
            {
                contentBox.Add(new Label("PACKAGE INSTANTIATIONS") { style = { color = _holoAccent, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11, marginBottom = 5, letterSpacing = 1 } });
                contentBox.Add(new Label(_inspectedNode.Examples) { style = { color = _holoText, whiteSpace = WhiteSpace.Normal, fontSize = 13, opacity = 0.8f, unityFontStyleAndWeight = FontStyle.Italic } });
            }

            _inspectorContainer.Add(contentBox);

            if (_inspectedNode.Children.Count > 0)
            {
                var childrenBox = new VisualElement { style = { marginTop = 30, borderTopWidth = 1, borderTopColor = _holoDim, paddingTop = 15 } };
                childrenBox.Add(new Label("SUB-DOMAINS:") { style = { color = _holoText, opacity = 0.6f, marginBottom = 10, fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold } });

                foreach (var child in _inspectedNode.Children)
                {
                    childrenBox.Add(new Label($"> {child.Term}") { style = { color = _holoAccent, opacity = 0.8f, marginBottom = 2, fontSize = 12 } });
                }
                _inspectorContainer.Add(childrenBox);
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }

    /// <summary>
    /// Static data defining the Digital Universe Package Ontology
    /// </summary>
    public static class UniverseOntologyData
    {
        public static OntologyNode GetPackageTaxonomy()
        {
            var root = new OntologyNode
            {
                Term = "Digital Entity",
                Definition = "Any self-contained construct, concept, or system within the digital universe. The absolute root of all MicroPackages.",
                PackageDesignation = "Universal Root",
                ExecutionLayer = "Global Scope",
                Examples = "Any valid MicroPackage."
            };

            // 1. REALM (Level 1 - Already in your code)
            var realBranch = new OntologyNode
            {
                Term = "Simulated Reality (Physis)",
                Definition = "Entities and systems bound by physical rules.",
                PackageDesignation = "Diegetic Construct",
                ExecutionLayer = "Simulation Engine"
            };
            root.AddChild(realBranch);

            // 2. SUPER-DOMAIN (Level 2)
            var materialSuperDomain = new OntologyNode
            {
                Term = "Material Entity",
                Definition = "Objects with physical presence.",
                PackageDesignation = "Asset/Prop Package"
            };
            realBranch.AddChild(materialSuperDomain);

            // 3. DOMAIN (Level 3)
            var biosphereDomain = new OntologyNode
            {
                Term = "Biosphere",
                Definition = "All living organisms and biological systems.",
                ExecutionLayer = "Simulation Loop"
            };
            materialSuperDomain.AddChild(biosphereDomain);

            // 4. KINGDOM (Level 4)
            var animaliaKingdom = new OntologyNode
            {
                Term = "Animalia",
                Definition = "Multicellular organisms that consume organic material."
            };
            biosphereDomain.AddChild(animaliaKingdom);

            // 5. PHYLUM (Level 5)
            var chordataPhylum = new OntologyNode { Term = "Chordata", Definition = "Animals with a notochord/backbone." };
            animaliaKingdom.AddChild(chordataPhylum);

            // 6. CLASS (Level 6)
            var mammaliaClass = new OntologyNode { Term = "Mammalia", Definition = "Warm-blooded, hair-bearing animals." };
            chordataPhylum.AddChild(mammaliaClass);

            // 7. ORDER (Level 7)
            var carnivoraOrder = new OntologyNode { Term = "Carnivora", Definition = "Obligate meat-eaters." };
            mammaliaClass.AddChild(carnivoraOrder);

            // 8. GENUS (Level 8)
            var canisGenus = new OntologyNode { Term = "Canis", Definition = "Dogs, wolves, coyotes, and jackals." };
            carnivoraOrder.AddChild(canisGenus);

            // 9. FORM / SPECIES (Level 9)
            var lupusForm = new OntologyNode
            {
                Term = "Canis Lupus (Wolf)",
                Definition = "Apex predator, pack-hunter.",
                Examples = "Dire Wolf Variant, Timber Wolf"
            };
            canisGenus.AddChild(lupusForm);

            return root;
        }
    }
}