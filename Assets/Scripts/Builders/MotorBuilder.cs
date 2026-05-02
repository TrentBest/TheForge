using System;
using UnityEngine;

namespace Assets.Scripts.Builders
{
    /// <summary>
    /// The Architect for drone propulsion units.
    /// Reforged with the requested constructor and fluent API.
    /// </summary>
    [Serializable]
    public class MotorBuilder
    {
        [SerializeField] private string _name;
        [SerializeField] private float _maxThrust;
        [SerializeField] private Vector3 _offset;

        public string Name => _name;
        public float MaxThrust => _maxThrust;

        // FIXED: Added 2-argument constructor for the Library
        public MotorBuilder(string name, float maxThrust)
        {
            _name = name;
            _maxThrust = maxThrust;
        }

        public MotorBuilder WithName(string name) { _name = name; return this; }
        public MotorBuilder WithMaxThrust(float thrust) { _maxThrust = thrust; return this; }

        public ThrustData Build() => new ThrustData { MaxForce = _maxThrust, LocalOffset = _offset };
    }

    [Serializable]
    public struct ThrustData { public float MaxForce; public Vector3 LocalOffset; }
}