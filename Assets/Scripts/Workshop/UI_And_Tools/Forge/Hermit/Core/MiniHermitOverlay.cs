using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{
    public class MiniHermitOverlay : MonoBehaviour
    {
        private MiniHermitWorker _worker;
        private GUIStyle _style;

        private void Start()
        {
            _worker = GetComponent<MiniHermitWorker>();
            _style = new GUIStyle();
            _style.normal.textColor = Color.white;
            _style.alignment = TextAnchor.MiddleCenter;
            _style.fontSize = 12;
            _style.fontStyle = FontStyle.Bold;
        }

        private void OnGUI()
        {
            if (_worker == null) return;

            // Convert 3D position to 2D screen space for the label
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.5f);

            if (screenPos.z > 0) // Only draw if in front of camera
            {
                Rect rect = new Rect(screenPos.x - 100, Screen.height - screenPos.y - 20, 200, 20);

                // Draw background shadow for readability
                GUI.color = new Color(0, 0, 0, 0.6f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);

                // Draw the Status
                GUI.color = Color.orange;
                GUI.Label(rect, _worker.Status.CurrentState, _style);
            }
        }
    }
}