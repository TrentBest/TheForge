// File: Assets/Scripts/Workshop/Cast/Gestures/GestureDefinition.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.Gameplay.Cast.Gestures
{
    /// <summary>
    /// A "Flipbook" of procedural poses. 
    /// This is what the Ladder hands to the Soldier.
    /// </summary>
    public class GestureDefinition : ScriptableObject
    {
        public string GestureName;
        public List<PoseFrame> Frames = new List<PoseFrame>();
    }

    [Serializable]
    public class PoseFrame
    {
        public string PoseName; // e.g., "Hand on Rung"

        // Maps the name of the joint/bone to its local position/rotation
        public List<JointState> JointStates = new List<JointState>();

        // The equation governing how we arrive at THIS pose from the PREVIOUS pose
        public EquationOfMotion ApproachMotion = new EquationOfMotion();
    }

    [Serializable]
    public struct JointState
    {
        public string Path; // e.g., "Spine/Shoulder_R/Elbow_R"
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
    }
}