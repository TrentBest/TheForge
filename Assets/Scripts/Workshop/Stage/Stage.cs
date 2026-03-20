// File: Assets/Scripts/Workshop/Stage/Stage.cs
using System.Collections.Generic;
using UnityEngine;

namespace TheSingularityWorkshop.Stage
{
    [CreateAssetMenu(fileName = "NewStage", menuName = "Workshop/Stage Definition")]
    public class Stage : ScriptableObject
    {
        public string StageName;
        public string SettingDescription; // e.g., "A desolate, red sand basin under a scorching sun."

        // The cast and environment data
        public List<Prop> Props = new List<Prop>();
        // public List<Actor> Cast = new List<Actor>(); // To be added later

        public void AddProp(Prop newProp)
        {
            Props.Add(newProp);
            Debug.Log($"[Workshop.Stage] Added {newProp.Name} at {newProp.Position}.");
        }

        /// <summary>
        /// The magic: Translating the 3D data into a book format.
        /// </summary>
        public string RenderSceneToText()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"--- {StageName.ToUpper()} ---");
            sb.AppendLine(SettingDescription);

            foreach (var prop in Props)
            {
                // This could be made infinitely more complex with AI, but the logic holds:
                sb.AppendLine($"At coordinates {prop.Position}, stands {prop.RenderToString()}.");
            }

            return sb.ToString();
        }
    }
}