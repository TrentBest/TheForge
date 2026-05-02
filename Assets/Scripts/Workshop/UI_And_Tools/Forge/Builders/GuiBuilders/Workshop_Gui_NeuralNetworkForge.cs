using Hermit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.FSM_API;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Core;
using Workshop.UI_And_Tools.Forge.Hermit.Core;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{


   

    public class Workshop_Gui_NeuralNetworkForge : IGuiProvider
    {
        public string Title => "Neural Network Forge";

        public FSMHandle NN_Status { get; private set; }

        private string _processGroup;
        private NeuralNetworkForgeContext _context;
        private LiveModelPreviewBuilder _previewBuilder;

        // UI Containers
        private VisualElement _rightPanel;
        private VisualElement _inspectorPanel;

        private List<LineRenderer> _visualSynapses = new List<LineRenderer>();
        private List<Renderer> _visualNeurons = new List<Renderer>();
        private List<Transform> _visualActivationBars = new List<Transform>();
        private Color[] _regionThemeColors = new Color[] { Color.cyan, new Color(0.8f, 0.2f, 1f), Color.yellow, Color.green };

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public VisualElement CreateGui(GuiContext ctx)
        {
            _processGroup = "NN_Forge_" + Guid.NewGuid().ToString().Substring(0, 6);
            _context = new NeuralNetworkForgeContext();
            SeedInitialData();

            // 1. SQUARED UP PREVIEW
            GameObject networkModel = GenerateProceduralNetworkModel(_context);
            _previewBuilder = new LiveModelPreviewBuilder(networkModel)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.05f))
                // REMOVED Auto-Orbit. Squared up.
                .WithZoom(8f)
                .WithMouseControl(true);

            var rootContainer = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row } };

            // 2. THE OVERLAY PATTERN
            var previewContainer = _previewBuilder.CreateGui(ctx);
            previewContainer.style.flexGrow = 1;
            previewContainer.style.flexBasis = new StyleLength(new Length(65, LengthUnit.Percent));

            // Add the floating Input buttons right over the 3D nodes
            previewContainer.Add(BuildInWorldInputButtons());

            rootContainer.Add(previewContainer);

            _rightPanel = new VisualElement { style = { width = 350, backgroundColor = new Color(0.12f, 0.12f, 0.12f), borderLeftWidth = 2, borderLeftColor = Color.black } };
            rootContainer.Add(_rightPanel);

            BuildToolbar();
            BuildExecutionAndInputsPanel();
            SetupSimulationFSM();

            rootContainer.schedule.Execute(() => {
                FSM_API.Interaction.Update(_processGroup);
                UpdateLightningVisuals();
            }).Every(16);

            rootContainer.RegisterCallback<DetachFromPanelEvent>(evt => {
                _previewBuilder?.Dispose();
                FSM_API.Interaction.DestroyInstance(NN_Status);
            });

            return rootContainer;
        }
        private VisualElement BuildInWorldInputButtons()
        {
            var overlay = new VisualElement
            {
                style = { position = Position.Absolute, left = 20, top = 20, bottom = 20, width = 120, justifyContent = Justify.Center }
            };

            foreach (var input in _context.Inputs)
            {
                var btn = new Button();

                Action updateBtn = () => {
                    btn.text = $"{input.InputName}\n[{(input.CurrentValue > 0 ? "ACTUATED" : "OFF")}]";
                    btn.style.backgroundColor = input.CurrentValue > 0 ? new Color(0.1f, 0.6f, 0.8f) : new Color(0.2f, 0.2f, 0.2f);
                };

                btn.clicked += () => {
                    input.CurrentValue = input.CurrentValue == 0 ? input.MaxValue : 0; // Toggle Full Voltage

                    // INJECT LIVE SIGNAL INTO THE BRAIN
                    if (_context.LiveBrain != null && _context.LiveBrain.Cortex.TryGetValue("Sense_" + input.InputName, out var liveNode))
                    {
                        liveNode.ReceiveSignal(input.CurrentValue);
                    }

                    updateBtn();
                };

                btn.style.height = 60;
                btn.style.marginBottom = 15;
                btn.style.color = Color.white;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                updateBtn();

                overlay.Add(btn);
            }
            return overlay;
        }
        // --- UI BUILDING METHODS ---

        private void BuildToolbar()
        {
            var toolbar = new VisualElement { style = { flexDirection = FlexDirection.Row, height = 30, backgroundColor = new Color(0.2f, 0.2f, 0.2f) } };

            toolbar.Add(new Button(() => ShowPanel("CRUD")) { text = "Files", style = { flexGrow = 1 } });
            toolbar.Add(new Button(() => ShowPanel("Architecture")) { text = "Regions", style = { flexGrow = 1 } });
            toolbar.Add(new Button(() => ShowPanel("Execute")) { text = "Execute", style = { flexGrow = 1 } });

            _rightPanel.Add(toolbar);

            // Container for dynamic panel content
            _inspectorPanel = new VisualElement { style = { flexGrow = 1, paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10 } };
            _rightPanel.Add(_inspectorPanel);
        }

        private void ShowPanel(string panelName)
        {
            _inspectorPanel.Clear();
            var title = new Label(panelName) { style = { fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15, color = Color.white } };
            _inspectorPanel.Add(title);

            switch (panelName)
            {
                case "CRUD":
                    BuildCrudPanel();
                    break;
                case "Architecture":
                    BuildArchitecturePanel();
                    break;
                case "Execute":
                    BuildExecutionAndInputsPanel();
                    break;
            }
        }

        private void BuildCrudPanel()
        {
            _inspectorPanel.Add(new TextField("Network Name") { value = _context.Name });
            _inspectorPanel.Add(new Button(() => Debug.Log("Saving...")) { text = "Save Network Definition" });
            _inspectorPanel.Add(new Button(() => Debug.Log("Loading...")) { text = "Load Existing..." });
            _inspectorPanel.Add(new Button(() => Debug.Log("Clearing...")) { text = "Clear / New", style = { backgroundColor = new Color(0.4f, 0.1f, 0.1f) } });
        }

        private void BuildArchitecturePanel()
        {
            _inspectorPanel.Add(new Label("Neural Regions (Layers/Clusters)") { style = { color = Color.gray, marginBottom = 5 } });

            var listView = new ListView(_context.Regions, 25, MakeRegionItem, BindRegionItem)
            {
                style = { flexGrow = 1, minHeight = 150, backgroundColor = new Color(0.1f, 0.1f, 0.1f) },
                showAlternatingRowBackgrounds = AlternatingRowBackground.All
            };

            listView.selectionChanged += (items) => {
                _context.SelectedObject = items.FirstOrDefault();
                RefreshNodeInspector();
            };

            _inspectorPanel.Add(listView);
            _inspectorPanel.Add(new Button(() => { _context.Regions.Add(new NeuralRegion { RegionName = "New Region", NodeCount = 1 }); listView.RefreshItems(); }) { text = "+ Add Region" });
        }

        private void BuildExecutionAndInputsPanel()
        {
            // 1. Chronos Controls
            var timeBox = new VisualElement { style = { paddingTop = 5, paddingRight = 5, paddingLeft = 5, paddingBottom = 5, backgroundColor = new Color(0.15f, 0.15f, 0.15f), marginBottom = 15 } };
            timeBox.Add(new Label("Simulation Flow") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            var controlsRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 5 } };
            controlsRow.Add(new Button(() => _context.IsPlaying = !_context.IsPlaying) { text = "Play/Pause", style = { flexGrow = 1 } });
            controlsRow.Add(new Button(() => { _context.IsPlaying = false; FSM_API.Interaction.Update(_processGroup); }) { text = "Step >", style = { flexGrow = 1 } });
            timeBox.Add(controlsRow);
            _inspectorPanel.Add(timeBox);

            // 2. Dynamic Inputs
            _inspectorPanel.Add(new Label("Live Inputs") { style = { fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, color = Color.cyan, marginBottom = 5 } });

            foreach (var input in _context.Inputs)
            {
                var slider = new Slider(input.InputName, input.MinValue, input.MaxValue) { value = input.CurrentValue };
                slider.RegisterValueChangedCallback(evt => {
                    input.CurrentValue = evt.newValue;
                    // TODO: Push this value directly into the corresponding FSM State/Node's context immediately!
                });
                _inspectorPanel.Add(slider);
            }
        }

        // --- UTILS & MOCKS ---

        private VisualElement MakeRegionItem() => new Label();
        private void BindRegionItem(VisualElement e, int i) => ((Label)e).text = $"{_context.Regions[i].RegionName} ({_context.Regions[i].NodeCount} Nodes)";

        private void RefreshNodeInspector()
        {
            // If the user clicks a region in the list, or clicks a 3D sphere (via raycast later), 
            // we populate a property editor here to change Activation Functions, Node Counts, etc.
        }

        private void SetupSimulationFSM()
        {
            if(!FSM_API.Interaction.Exists("NN_Simulation"))
            {
                FSM_API.Create.CreateFiniteStateMachine("NN_Simulation", processRate: 1, _processGroup)
                    .State("Simulating", null, (c) => {
                        var ctx = (NeuralNetworkForgeContext)c;

                        // Wait for Play mode OR a manual Step click
                        if (!ctx.IsPlaying && !ctx.StepRequested) // FIXED RETURN
                        ctx.StepRequested = false;

                        PropagateOneLayer(ctx);

                       
                    }, null)
                    .BuildDefinition();
            }
            NN_Status = FSM_API.Create.CreateInstance("NN_Simulation", _context, _processGroup);
        }

        private void PropagateOneLayer(NeuralNetworkForgeContext ctx)
        {
            ctx.FiringSynapses.Clear();

            // Reset flow if we reached the final output layer
            if (ctx.CurrentPropagationLayer >= ctx.Regions.Count - 1)
            {
                ctx.CurrentPropagationLayer = 0;
                return; // Wait for next step to fire from Region 0
            }

            int layerToFire = ctx.CurrentPropagationLayer;
            int currentStart = 0;
            for (int i = 0; i < layerToFire; i++) currentStart += ctx.Regions[i].NodeCount;
            int nextStart = currentStart + ctx.Regions[layerToFire].NodeCount;
            int nextEnd = nextStart + ctx.Regions[layerToFire + 1].NodeCount;

            // Evaluate Synapses for THIS GAP ONLY
            for (int i = 0; i < ctx.Synapses.Count; i++)
            {
                var syn = ctx.Synapses[i];
                if (syn.Item1 >= currentStart && syn.Item1 < nextStart && syn.Item2 >= nextStart && syn.Item2 < nextEnd)
                {
                    if (layerToFire == 0)
                    {
                        // Input Logic: Read direct from the toggled buttons!
                        int inputIdx = syn.Item1 % Mathf.Max(1, ctx.Inputs.Count);
                        if (ctx.Inputs[inputIdx].CurrentValue > 0) ctx.FiringSynapses.Add(i);
                    }
                    else
                    {
                        // Hidden Logic: Simulated weights (Random threshold for demo)
                        if (UnityEngine.Random.value > 0.4f) ctx.FiringSynapses.Add(i);
                    }
                }
            }

            ctx.CurrentPropagationLayer++;
        }

        private void SeedInitialData()
        {
            // 1. Try to find an active HermitBrain in the scene
            _context.LiveBrain = GameObject.FindAnyObjectByType<HermitBrain>();

            if (_context.LiveBrain == null || _context.LiveBrain.Cortex == null)
            {
                Debug.LogWarning("[NN Forge] No active HermitBrain found in scene. Waiting for instantiation...");
                return;
            }

            // 2. Map the live topology to the UI Regions
            // For visualization, we group nodes into layers based on their naming convention (Sense, Process, Motor)
            var inputNodes = _context.LiveBrain.Cortex.Values.Where(n => n.Id.StartsWith("Sense")).ToList();
            var processNodes = _context.LiveBrain.Cortex.Values.Where(n => n.Id.StartsWith("Process") || n.Id.StartsWith("Emotion")).ToList();
            var motorNodes = _context.LiveBrain.Cortex.Values.Where(n => n.Id.StartsWith("Motor")).ToList();

            _context.Regions.Add(new NeuralRegion { RegionName = "Sensory Cortex", NodeCount = inputNodes.Count, RegionColor = Color.cyan });
            _context.Regions.Add(new NeuralRegion { RegionName = "Processing Hub", NodeCount = processNodes.Count, RegionColor = Color.white });
            _context.Regions.Add(new NeuralRegion { RegionName = "Motor Output", NodeCount = motorNodes.Count, RegionColor = Color.red });

            // 3. Map UI buttons to the live Sense nodes
            foreach (var node in inputNodes)
            {
                _context.Inputs.Add(new NeuralInputDef
                {
                    InputName = node.Id.Replace("Sense_", ""),
                    MinValue = 0,
                    MaxValue = node.Threshold,
                    CurrentValue = 0
                });
            }
        }

        private GameObject GenerateProceduralNetworkModel(NeuralNetworkForgeContext ctx)
        {
            GameObject root = new GameObject("Network_Base");
            root.hideFlags = HideFlags.HideAndDontSave;

            float xSpacing = 5f;
            float ySpacing = 1.5f;

            // Use Unlit/Color to guarantee they glow and never disappear in Editor lighting
            Material unlitMatBase = new Material(Shader.Find("Unlit/Color"));
            List<Transform> allNodes = new List<Transform>();

            for (int r = 0; r < ctx.Regions.Count; r++)
            {
                NeuralRegion region = ctx.Regions[r];
                int nodeCount = region.NodeCount;
                Color themeColor = _regionThemeColors[r % _regionThemeColors.Length];

                float startY = -(nodeCount - 1) * ySpacing / 2f;
                float xPos = r * xSpacing - (ctx.Regions.Count * xSpacing / 2f);

                // 1. IN-WORLD GUI: The Glass Backboard (The "Panel")
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.name = $"InWorldGui_Panel_{region.RegionName}";
                panel.transform.SetParent(root.transform);
                panel.transform.position = new Vector3(xPos + 0.5f, 0, 0.5f); // Offset slightly
                panel.transform.localScale = new Vector3(3f, Mathf.Max(nodeCount * ySpacing + 2f, 4f), 0.1f);

                Material panelMat = new Material(Shader.Find("Standard"));
                panelMat.color = new Color(0.1f, 0.1f, 0.12f, 0.8f); // Dark translucent glass
                panel.GetComponent<Renderer>().sharedMaterial = panelMat;

                // 2. IN-WORLD GUI: The Header Strip
                GameObject header = GameObject.CreatePrimitive(PrimitiveType.Cube);
                header.transform.SetParent(panel.transform);
                header.transform.localPosition = new Vector3(0, 0.48f, -0.6f); // Top of the panel
                header.transform.localScale = new Vector3(1f, 0.04f, 0.2f);
                Material headerMat = new Material(unlitMatBase) { color = themeColor };
                header.GetComponent<Renderer>().sharedMaterial = headerMat;

                // 3. IN-WORLD GUI: The List Items (Neurons & Activation Bars)
                for (int n = 0; n < nodeCount; n++)
                {
                    Vector3 pos = new Vector3(xPos, startY + (n * ySpacing), 0);
                    ctx.NodePositions.Add(pos);

                    GameObject nodeObj;
                    if (r == 0) // INPUT
                    {
                        nodeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        nodeObj.transform.localScale = new Vector3(0.5f, 1.2f, 1.2f);
                    }
                    else if (r == ctx.Regions.Count - 1) // OUTPUT
                    {
                        nodeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        nodeObj.transform.rotation = Quaternion.Euler(45, 45, 45); // Diamond shape
                        nodeObj.transform.localScale = Vector3.one * 1f;
                    }
                    else // HIDDEN
                    {
                        nodeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        nodeObj.transform.localScale = Vector3.one * 0.8f;
                    }

                    nodeObj.transform.position = pos;
                    nodeObj.transform.SetParent(root.transform);

                    Material neuronMat = new Material(unlitMatBase) { color = themeColor * 0.3f };
                    var nRenderer = nodeObj.GetComponent<Renderer>();
                    nRenderer.sharedMaterial = neuronMat;
                    _visualNeurons.Add(nRenderer);
                    allNodes.Add(nodeObj.transform);

                    // The List Item Data (Horizontal Activation Bar)
                    GameObject actBar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    actBar.transform.SetParent(root.transform);
                    // Rotate cylinder to lie horizontally like a UI progress bar
                    actBar.transform.rotation = Quaternion.Euler(0, 0, 90);
                    actBar.transform.position = pos + new Vector3(1f, 0, 0); // Placed to the right of the node
                    actBar.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f); // Starts at 0 width

                    Material barMat = new Material(unlitMatBase) { color = themeColor };
                    actBar.GetComponent<Renderer>().sharedMaterial = barMat;
                    _visualActivationBars.Add(actBar.transform);
                }
            }

            // 4. SYNAPSES (The Lightning)
            Material lineMat = new Material(Shader.Find("Sprites/Default")); // Bright additive lines
            int synapseIndex = 0;
            int currentNode = 0;

            for (int l = 0; l < ctx.Regions.Count - 1; l++)
            {
                int nodesInCurrent = ctx.Regions[l].NodeCount;
                int nodesInNext = ctx.Regions[l + 1].NodeCount;

                for (int i = 0; i < nodesInCurrent; i++)
                {
                    for (int j = 0; j < nodesInNext; j++)
                    {
                        int fromIdx = currentNode + i;
                        int toIdx = currentNode + nodesInCurrent + j;
                        ctx.Synapses.Add(new Tuple<int, int>(fromIdx, toIdx));

                        var lineObj = new GameObject($"Synapse_{synapseIndex}");
                        lineObj.transform.SetParent(root.transform);
                        var lr = lineObj.AddComponent<LineRenderer>();
                        lr.positionCount = 2;
                        lr.SetPosition(0, allNodes[fromIdx].position);
                        lr.SetPosition(1, allNodes[toIdx].position);
                        lr.material = lineMat;

                        _visualSynapses.Add(lr);
                        synapseIndex++;
                    }
                }
                currentNode += nodesInCurrent;
            }

            return root;
        }

        // --- UPDATE THE ANIMATION TICK ---
        private void UpdateLightningVisuals()
        {
            if (_context == null || _context.LiveBrain == null) return;

            // Flatten the live nodes into a list matching the 3D generated order (Sense -> Process -> Motor)
            var allLiveNodes = new List<NeuralNode>();
            //allLiveNodes.AddRange(_context.LiveBrain.Cortex.Values.Where(n => n.Id.StartsWith("Sense")));
            //allLiveNodes.AddRange(_context.LiveBrain.Cortex.Values.Where(n => n.Id.StartsWith("Process") || n.Id.StartsWith("Emotion")));
            //allLiveNodes.AddRange(_context.LiveBrain.Cortex.Values.Where(n => n.Id.StartsWith("Motor")));

            // 1. Animate Neurons & Activation Bars based on REAL Data
            for (int i = 0; i < _visualNeurons.Count && i < allLiveNodes.Count; i++)
            {
                if (_visualNeurons[i] == null || _visualActivationBars[i] == null) continue;

                var liveNode = allLiveNodes[i];

                // Math: How close is this node to firing? (0.0 to 1.0)
                float chargePercentage = Mathf.Clamp01(liveNode.Activation / liveNode.Threshold);

                // Scale the 3D UI bar
                _visualActivationBars[i].localScale = new Vector3(0.1f, chargePercentage * 0.8f, 0.1f);

                // Brighten the neuron sphere as it charges
                var mat = _visualNeurons[i].sharedMaterial;
                mat.color = Color.Lerp(new Color(0.1f, 0.1f, 0.1f), Color.white, chargePercentage);
            }

            // 2. Animate Synapses based on Firing State
            // (Assumes you mapped ctx.Synapses to align with liveNode.OutputConnections)
            for (int i = 0; i < _visualSynapses.Count; i++)
            {
                if (_visualSynapses[i] == null) continue;

                // For true visualization, you check if the originating node of this synapse has Activation >= Threshold
                int fromNodeIdx = _context.Synapses[i].Item1;
                bool isFiring = allLiveNodes[fromNodeIdx].Activation >= allLiveNodes[fromNodeIdx].Threshold;

                if (isFiring)
                {
                    _visualSynapses[i].startColor = Color.white;
                    _visualSynapses[i].endColor = Color.cyan;
                    _visualSynapses[i].widthMultiplier = 0.08f;
                }
                else
                {
                    _visualSynapses[i].startColor = new Color(0, 0.1f, 0.3f, 0.2f);
                    _visualSynapses[i].endColor = new Color(0, 0.05f, 0.1f, 0.1f);
                    _visualSynapses[i].widthMultiplier = 0.015f;
                }
            }
        }

        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}