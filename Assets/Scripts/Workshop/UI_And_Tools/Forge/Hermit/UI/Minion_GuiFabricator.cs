using System;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics; // Injected for ForgeLogger
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{
    // 1. EXECUTED: Removed MonoBehaviour. This is now a pure static brain.
    public static class Minion_GuiFabricator
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void BootMinion()
        {
            // 2. EXECUTED: We do not spawn a GameObject. We do not use DontDestroyOnLoad.
            // We simply inject our pure C# tick directly into the engine's render loop.
            Application.onBeforeRender -= TickFabricator; // Safety un-subscribe
            Application.onBeforeRender += TickFabricator;

            ForgeLogger.Log("Logic Awakened. (Zero GameObject Allocation)")
                .WithHeader("Minion Scribe")
                .WithColor("#00FFCC") // Neon Cyan for logic ignition
                .SendToUnity();
        }

        // 3. EXECUTED: Removed LateUpdate(). This is a raw C# delegate.
        private static void TickFabricator()
        {
            if (SingularityDataBus.Instance.ReceiveRaw("Minion_Labor_Pool", out byte[] rawBytes))
            {
                try
                {
                    string rawJson = Encoding.UTF8.GetString(rawBytes);
                    var msg = JsonUtility.FromJson<SingularityMessage>(rawJson);

                    if (msg != null && msg.OpCode == "FABRICATE_GUI")
                    {
                        ForgeLogger.Log($"Inbound Fabrication Directive: {msg.Payload}")
                            .WithHeader("Labor Pool")
                            .WithColor("#FFA500") // Amber for Swarm/Minion tasks
                            .SendToUnity();

                        Fabricate(msg.Payload);
                    }
                }
                catch (Exception ex)
                {
                    ForgeLogger.LogError($"Decode Error in Labor Pool: {ex.Message}")
                        .WithHeader("Minion")
                        .SendToUnity();
                }
            }
        }

        private static void Fabricate(string guiName)
        {
            ForgeLogger.Log($"Initiating Manifestation sequence for: {guiName}")
                .WithHeader("Fabricator")
                .WithColor("#FF00FF") // Magenta for manifestation
                .SendToUnity();

            var provider = TypeGuiProviderFactory.GetProviderByName(guiName);
            if (provider == null)
            {
                ForgeLogger.LogError($"Manifestation Failed: '{guiName}' is not a valid IGuiProvider.")
                    .WithHeader("Fabricator")
                    .SendToUnity();
                return;
            }

            var existing = GameObject.Find($"InWorld_{guiName}");
            if (existing != null)
            {
                ForgeLogger.Log($"Purging stale instance: InWorld_{guiName}")
                    .WithHeader("Fabricator")
                    .WithColor(Color.red)
                    .SendToUnity();
                GameObject.Destroy(existing);
            }

            var builder = new InWorldGuiBuilder();
            builder.Build();

            // Note: We still spawn a GameObject HERE because Unity's UIDocument fundamentally 
            // requires a GameObject host to render in world-space. But the LOGIC remains pure C#.
            GameObject uiHost = new GameObject($"InWorld_{guiName}");

            ForgeLogger.Log($"Spawning physical UI Host vessel: {uiHost.name}")
                .WithHeader("Fabricator")
                .WithColor("#FFA500")
                .SendToUnity();

            var lmpRoot = GameObject.Find("LMP_Root");
            if (lmpRoot != null) uiHost.transform.SetParent(lmpRoot.transform);

            var anchor = GameObject.Find("Hermit") ?? Camera.main?.gameObject;
            if (anchor != null)
            {
                uiHost.transform.position = anchor.transform.position + anchor.transform.forward * 2.5f + Vector3.up * 1.2f;
                uiHost.transform.LookAt(anchor.transform.position + Vector3.up * 1.2f);
                uiHost.transform.Rotate(0, 180, 0);
            }

            var uiDoc = uiHost.AddComponent<UIDocument>();

            var settings = Resources.Load<PanelSettings>("DefaultPanelSettings");
            if (settings == null)
            {
                ForgeLogger.LogWarning("'DefaultPanelSettings' not found in Resources. Using engine default.")
                    .WithHeader("Fabricator")
                    .SendToUnity();
            }
            else uiDoc.panelSettings = settings;

            uiDoc.rootVisualElement.Clear();
            uiDoc.rootVisualElement.Add(provider.CreateGui(new GuiContext()));

            ForgeLogger.Log($"Manifested {guiName}. Physical Presence Confirmed.")
                .WithHeader("Minion")
                .WithColor("#FF00FF") // Magenta for successful manifestation
                .SendToUnity();
        }
    }
}