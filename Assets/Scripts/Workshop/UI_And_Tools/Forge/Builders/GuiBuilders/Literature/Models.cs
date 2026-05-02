using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.Systems.MicroPackages.Literature
{
    [Serializable]
    public class StoryboardFrame
    {
        public string FrameId = Guid.NewGuid().ToString();
        public int SequenceNumber;

        [Header("Narrative Context")]
        public string SceneDescription;
        public float ChronologicalTime; // The exact moment in the world's timeline

        [Header("Scryer Lens Data (The 3D State)")]
        public Vector3 CameraPosition;
        public Quaternion CameraRotation;
        public float FieldOfView;

        [Header("The 2D Artifact")]
        public Texture2D CapturedArtifact; // The actual scried image
    }

    [Serializable]
    public class StoryboardSequence
    {
        public string SequenceName = "New Sequence";
        public List<StoryboardFrame> Frames = new();
    }

    [Serializable]
    public class SemanticBeat
    {
        public string BeatId = Guid.NewGuid().ToString();

        // THE MAD LIBS CORE
        public string SubjectId;   // Noun 1 (The Character)
        public string ActionVerb;  // Verb (The Action)
        public string ObjectId;    // Noun 2 (The Target/Item)

        // Modifiers
        public List<string> Adjectives = new();
        public string ContextualAdverb;

        // The actual generated sentence (e.g., "The [Adjective] [Subject] [Adverb] [Action] the [Object].")
        public string GetConstructedSentence()
        {
            string adj = Adjectives.Count > 0 ? string.Join(", ", Adjectives) + " " : "";
            string adv = !string.IsNullOrEmpty(ContextualAdverb) ? ContextualAdverb + " " : "";
            return $"The {adj}{SubjectId} {adv}{ActionVerb} the {ObjectId}.";
        }
    }
}