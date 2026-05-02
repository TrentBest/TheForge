using UnityEngine;
using Workshop.Core.Memory;
using Workshop.Core.Math;
using System.Diagnostics;

namespace Workshop.Gameplay
{
    public class ShowcaseSwarm
    {
        private DataShelf<FSMAgentData> _shelf;
        private int _agentCount;
        private Stopwatch _stopwatch = new Stopwatch();
        private int _frameCount = 0;

        public int SwarmSize => _agentCount;

        public void Ignite(DataWarehouse warehouse, int count = 1000000, uint cosmicSeed = 1337)
        {
            _agentCount = count;
            _shelf = warehouse.GetOrCreateShelf<FSMAgentData>(1000000); // Expanding to 1 Million

            _stopwatch.Restart();

            // Beautiful galaxy color palette
            Color coreColor = new Color(1f, 0.9f, 0.6f, 1f);   // Bright yellow-white core
            Color midColor = new Color(0.0f, 0.8f, 1f, 1f);    // Brilliant Cyan mid-arms
            Color edgeColor = new Color(0.3f, 0.0f, 0.8f, 1f); // Deep purple fading edges

            unsafe
            {
                for (int i = 0; i < _agentCount; i++)
                {
                    _shelf.LifeTracker.Set(i);

                    // t goes from 0.0 to 1.0 based on index
                    float t = i / (float)_agentCount;

                    // Pow(t) forces more agents to cluster tightly in the center, thinning out at the edges
                    float curvedT = Mathf.Pow(t, 2.0f);

                    float angle = i * 137.508f * Mathf.Deg2Rad; // Golden ratio layout
                    float radius = 300f * curvedT; // 300 meter wide galaxy

                    float verticalNoise = Rng.GetFloat(i, cosmicSeed) - 0.5f;

                    _shelf.RawData[i].Position = new Vector3(
                        radius * Mathf.Cos(angle),
                        verticalNoise * 40f * (1f - curvedT), // Swarm is thickest in the middle, flat at the edges
                        radius * Mathf.Sin(angle)
                    );

                    _shelf.RawData[i].Scale = Mathf.Lerp(0.6f, 0.05f, curvedT);

                    // Apply color based on distance from center
                    if (curvedT < 0.25f)
                        _shelf.RawData[i].Color = Color.Lerp(coreColor, midColor, curvedT / 0.25f);
                    else
                        _shelf.RawData[i].Color = Color.Lerp(midColor, edgeColor, (curvedT - 0.25f) / 0.75f);
                }
            }

            _stopwatch.Stop();
            UnityEngine.Debug.Log($"[CPU Telemetry] Ignited {_agentCount} agents into memory in {_stopwatch.Elapsed.TotalMilliseconds:F3} ms.");
        }

        public void Tick(float deltaTime)
        {
            if (_shelf == null) return;

            _stopwatch.Restart();

            unsafe
            {
                // Slowed down the rotation slightly so it feels massive and heavy
                float speed = 0.1f;
                float s = Mathf.Sin(deltaTime * speed);
                float c = Mathf.Cos(deltaTime * speed);

                for (int i = 0; i < _agentCount; i++)
                {
                    if (_shelf.LifeTracker.IsSet(i))
                    {
                        var pos = _shelf.RawData[i].Position;

                        // O(1) Matrix Rotation - Math on 1,000,000 elements!
                        _shelf.RawData[i].Position.x = pos.x * c - pos.z * s;
                        _shelf.RawData[i].Position.z = pos.x * s + pos.z * c;
                    }
                }
            }

            _stopwatch.Stop();
            _frameCount++;

            if (_frameCount % 60 == 0)
            {
                UnityEngine.Debug.Log($"[CPU Telemetry] Rotated {_agentCount} agents. Math Chronos: {_stopwatch.Elapsed.TotalMilliseconds:F4} ms.");
            }
        }
    }
}