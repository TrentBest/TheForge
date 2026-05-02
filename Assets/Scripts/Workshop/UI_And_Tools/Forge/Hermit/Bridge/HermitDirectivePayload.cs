using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Hermit.Bridge
{
    [Serializable]
    public class HermitDirectivePayload
    {
        public string AgentId;
        public string Intent = "ChatResponse";
        public string TargetId; // The ID of the specific entity or ontology link
        public string Content;
        public Vector3 TargetPosition;
        public Dictionary<string, string> MetaData = new Dictionary<string, string>();

        public DirectiveSafetyLevel SafetyTier = DirectiveSafetyLevel.RequiresReview;
        public ProposalStatus Status = ProposalStatus.Proposed;

        public int PromptTokens;
        public int GenTokens;
        public int TotalTokens;
        public int SessionTokens;
        public int CurrentRPM;
        public int MaxRPM;

        public int TemporalIndex; // For "Back up... right there"
    }

    public enum DirectiveSafetyLevel
    {
        AutoExecute,
        InertDisplay,
        RequiresReview,
        DeepOntology
    }

    public enum ProposalStatus
    {
        Proposed, // Awaiting human review
        Approved, // Locked in, Minions are scrambling
        Rejected, // Sent to the Void
        Executed, // Task complete
        RolledBack // Temporal backtrack triggered
    }
}