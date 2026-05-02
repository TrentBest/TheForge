using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.GURPS
{
    [Serializable]
    public class GURPS_EntityContext
    {
        [Header("Identity")]
        public string Name = "Dai Blackthorn";
        public string Occupation = "Thief";

        [Header("GURPS Core (Reflective)")]
        public int ST = 10;
        public int DX = 12;
        public int IQ = 11;
        public int HT = 10;

        [Header("Appearance & Equipment")]
        public Mesh BodyMesh;
        public Material SkinMaterial;
        public List<string> EquippedSnaps = new List<string>(); // IDs of hats, coats, etc.

        [Header("The Stage")]
        public float SpotlightIntensity = 1.0f;
        public Color RimLightColor = Color.cyan;

        public float RotationY = 0f;
        // Key: Socket Name, Value: The Item Data
        public Dictionary<GURPS_SocketType, object> ActiveSnaps = new Dictionary<GURPS_SocketType, object>();

        public GameObject ActiveModel { get; internal set; }

        public void SnapItem(GURPS_SocketType socket, object item)
        {
            ActiveSnaps[socket] = item;
            Debug.Log($"[Assembly] Snapped {item} to {socket}");
        }
    }
}