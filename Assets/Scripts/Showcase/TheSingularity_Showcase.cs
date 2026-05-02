using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.Staging;

namespace Workshop.UI_And_Tools.Showcase
{
    /// <summary>
    /// The master application entry point. Coordinates the transition from boot into the Singularity.
    /// </summary>
    public class TheSingularity_Showcase : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        private IGuiRouter _router;

        private void Awake()
        {
            Debug.Log("[Showcase] Awake() triggered. Initializing Master Router...");

            if (_uiDocument == null)
            {
                Debug.LogError("[Showcase] CRITICAL: UIDocument is missing! Did it lose its reference in the Inspector?");
                return;
            }

            var root = _uiDocument.rootVisualElement;

            _router = new GuiFlowRouterBuilder("MasterRouter", "Boot")
                .WithRoot(root)
                .Build();

            Debug.Log("[Showcase] Router built successfully. Binding root and launching Boot Sequence.");

            var bootSequence = new Showcase_InteractiveBootSequence(OnBootComplete);
            _router.NavigateTo(bootSequence.Title);
        }

        private void OnBootComplete()
        {
            Debug.Log("[Showcase] OnBootComplete() callback fired. Preparing Singularity Lobby...");

            // Transition into the Lobby once the descent is finished
            var lobby = new Showcase_SingularityLobby(_router);

            Debug.Log($"[Showcase] Navigating to target provider: {lobby.Title}");
            _router.NavigateTo(lobby.Title);
        }
    }
}