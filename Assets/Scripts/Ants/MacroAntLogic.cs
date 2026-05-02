using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.Ants
{
    // --- 2. THE API FSM DEFINITION ---

    public static class MacroAntLogic
    {
        public static void InitializeFSM()
        {
            FSM_API.Create.CreateFiniteStateMachine("MacroAntAgent", processRate: 1, processingGroup: "MacroSwarm")
                .State("Scouting", null, OnUpdateScouting, null)
                .State("HaulingFood", null, OnUpdateHaulingFood, null)
                .State("HaulingWater", null, OnUpdateHaulingWater, null)

                .Transition("Scouting", "HaulingFood", ctx => ctx is MacroAntContext a && a.HasFood)
                .Transition("Scouting", "HaulingWater", ctx => ctx is MacroAntContext a && a.HasWater)

                .Transition("HaulingFood", "Scouting", ctx => ctx is MacroAntContext a && !a.HasFood)
                .Transition("HaulingWater", "Scouting", ctx => ctx is MacroAntContext a && !a.HasWater)
                .BuildDefinition();
        }

        private static void OnUpdateScouting(IStateContext context)
        {
            if (context is MacroAntContext ant)
            {
                if (!HandleMovementAndPausing(ant)) return;

                if (FollowPheromone(ant, ant.World.PheromoneG))
                {
                    ant.Speed = ant.World.Settings.AntBaseSpeed * 1.3f;
                }
                else
                {
                    WanderErratically(ant);
                }

                DropPheromone(ant, 2); // Drop Blue (Home) trail
                CheckForResources(ant);
            }
        }

        private static void OnUpdateHaulingFood(IStateContext context)
        {
            if (context is MacroAntContext ant)
            {
                if (!HandleMovementAndPausing(ant)) return;

                if (FollowPheromone(ant, ant.World.PheromoneB))
                {
                    ant.Speed = ant.World.Settings.AntBaseSpeed * 1.3f;
                    Vector2 home = new Vector2(ant.World.Width / 2, ant.World.Height / 2);
                    float targetAngle = Mathf.Atan2(home.y - ant.Position.y, home.x - ant.Position.x) * Mathf.Rad2Deg;
                    float angleDiff = Mathf.DeltaAngle(ant.Heading, targetAngle);
                    ant.Heading += angleDiff * 0.05f;
                }
                else
                {
                    ReturnToCenter(ant);
                }

                DropPheromone(ant, 1); // Drop Green (Food) trail
                CheckForHome(ant);
            }
        }

        private static void OnUpdateHaulingWater(IStateContext context)
        {
            if (context is MacroAntContext ant)
            {
                if (!HandleMovementAndPausing(ant)) return;

                if (FollowPheromone(ant, ant.World.PheromoneB))
                {
                    ant.Speed = ant.World.Settings.AntBaseSpeed * 1.3f;
                    Vector2 home = new Vector2(ant.World.Width / 2, ant.World.Height / 2);
                    float targetAngle = Mathf.Atan2(home.y - ant.Position.y, home.x - ant.Position.x) * Mathf.Rad2Deg;
                    float angleDiff = Mathf.DeltaAngle(ant.Heading, targetAngle);
                    ant.Heading += angleDiff * 0.05f;
                }
                else
                {
                    ReturnToCenter(ant);
                }

                DropPheromone(ant, 1);
                CheckForHome(ant);
            }
        }

        private static bool HandleMovementAndPausing(MacroAntContext ant)
        {
            if (ant.PauseTicks > 0)
            {
                ant.PauseTicks--;
                return false;
            }

            if (UnityEngine.Random.value < 0.02f)
            {
                ant.PauseTicks = UnityEngine.Random.Range(5, 20);
                return false;
            }

            float rad = ant.Heading * Mathf.Deg2Rad;
            float nextX = ant.Position.x + Mathf.Cos(rad) * ant.Speed;
            float nextY = ant.Position.y + Mathf.Sin(rad) * ant.Speed;

            int nx = (int)nextX;
            int ny = (int)nextY;

            if (nx < 5 || nx >= ant.World.Width - 5 || ny < 5 || ny >= ant.World.Height - 5 ||
                ant.World.ResourceGrid[nx, ny] == 3)
            {
                ant.Heading += UnityEngine.Random.Range(135f, 225f);
                return false;
            }

            ant.Position.x = nextX;
            ant.Position.y = nextY;

            // DECAY CHARGE: Every step they take physically depletes their scent gland
            ant.PheromoneCharge = Mathf.Max(0f, ant.PheromoneCharge - ant.World.Settings.ChargeDecayRate);

            return true;
        }

        private static bool FollowPheromone(MacroAntContext ant, byte[,] scentGrid)
        {
            byte center = SampleAntenna(ant, scentGrid, 0f, 8f);
            byte left = SampleAntenna(ant, scentGrid, -40f, 8f);
            byte right = SampleAntenna(ant, scentGrid, 40f, 8f);

            if (center == 0 && left == 0 && right == 0) return false;

            if (center >= left && center >= right)
            {
                ant.Heading += UnityEngine.Random.Range(-5f, 5f);
            }
            else if (left > right)
            {
                ant.Heading -= 20f;
            }
            else if (right > left)
            {
                ant.Heading += 20f;
            }

            return true;
        }

        private static byte SampleAntenna(MacroAntContext ant, byte[,] grid, float angleOffset, float distance)
        {
            float rad = (ant.Heading + angleOffset) * Mathf.Deg2Rad;
            int sx = Mathf.Clamp((int)(ant.Position.x + Mathf.Cos(rad) * distance), 0, ant.World.Width - 1);
            int sy = Mathf.Clamp((int)(ant.Position.y + Mathf.Sin(rad) * distance), 0, ant.World.Height - 1);

            if (ant.World.ResourceGrid[sx, sy] == 3) return 0;
            return grid[sx, sy];
        }

        private static void WanderErratically(MacroAntContext ant)
        {
            ant.Speed = ant.World.Settings.AntBaseSpeed * 0.8f;
            if (UnityEngine.Random.value < 0.1f)
            {
                ant.Heading += UnityEngine.Random.Range(-45f, 45f);
            }
            else
            {
                ant.Heading += UnityEngine.Random.Range(-ant.World.Settings.WanderJitter, ant.World.Settings.WanderJitter);
            }
        }

        private static void ReturnToCenter(MacroAntContext ant)
        {
            ant.Speed = ant.World.Settings.AntBaseSpeed * 1.0f;
            Vector2 home = new Vector2(ant.World.Width / 2, ant.World.Height / 2);
            float targetAngle = Mathf.Atan2(home.y - ant.Position.y, home.x - ant.Position.x) * Mathf.Rad2Deg;

            float angleDiff = Mathf.DeltaAngle(ant.Heading, targetAngle);
            ant.Heading += Mathf.Clamp(angleDiff, -5f, 5f);
            ant.Heading += UnityEngine.Random.Range(-15f, 15f);
        }

        private static void CheckForResources(MacroAntContext ant)
        {
            int px = (int)ant.Position.x;
            int py = (int)ant.Position.y;

            if (ant.World.ResourceGrid[px, py] == 1 || ant.World.ResourceGrid[px, py] == 2)
            {
                if (ant.World.ResourceGrid[px, py] == 1) ant.HasFood = true;
                if (ant.World.ResourceGrid[px, py] == 2) ant.HasWater = true;

                ant.World.ResourceGrid[px, py] = 0;
                ant.Heading += 180f;

                // REFILL CHARGE: They hit the source, their gland is full of target scent!
                ant.PheromoneCharge = 255f;
            }
        }

        private static void CheckForHome(MacroAntContext ant)
        {
            if (Vector2.Distance(ant.Position, new Vector2(ant.World.Width / 2, ant.World.Height / 2)) < 15f)
            {
                if (ant.HasFood) ant.World.TotalFoodStored++;
                if (ant.HasWater) ant.World.TotalWaterStored++;

                ant.HasFood = false;
                ant.HasWater = false;
                ant.Heading += 180f;

                // REFILL CHARGE: They are back home, refill the home scent gland!
                ant.PheromoneCharge = 255f;
            }
        }

        // PERFECT GRADIENT MATH: Capped Additive Deposition
        private static void DropPheromone(MacroAntContext ant, int channel)
        {
            if (UnityEngine.Random.value > ant.World.Settings.PheromoneDropChance) return;

            int x = (int)ant.Position.x;
            int y = (int)ant.Position.y;

            // Base amount scaled down as they run out of charge
            int dropAmt = Mathf.RoundToInt((ant.PheromoneCharge / 255f) * ant.World.Settings.PheromoneDropAmount);
            if (dropAmt <= 0) return;

            // The absolute maximum concentration this cell can reach is the ant's current charge.
            // This guarantees a perfect slope downwards from the source, no matter how many ants walk it!
            int gradientCap = (int)ant.PheromoneCharge;

            if (channel == 1)
            {
                int current = ant.World.PheromoneG[x, y];
                if (current < gradientCap) ant.World.PheromoneG[x, y] = (byte)Mathf.Clamp(current + dropAmt, 0, gradientCap);
            }
            if (channel == 2)
            {
                int current = ant.World.PheromoneB[x, y];
                if (current < gradientCap) ant.World.PheromoneB[x, y] = (byte)Mathf.Clamp(current + dropAmt, 0, gradientCap);
            }
        }
    }
}