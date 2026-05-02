using UnityEngine;

namespace Assets.Scripts.raWWar.Drill
{
    public class DrillGroundsLogic
    {
        private DrillGroundsContext _ctx;
        private float _tickTimer;
        private float _baseTickRate = 0.3f;
        private int _kernelId = -1;

        public DrillGroundsLogic(DrillGroundsContext context)
        {
            _ctx = context;

            // Only look for the kernel if the shader actually exists!
            if (_ctx.DrillShader != null)
                _kernelId = _ctx.DrillShader.FindKernel("CSDrillMove");

            SpawnRecruit();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_ctx.IsGameOver) return;

            float currentTickRate = _baseTickRate / _ctx.SpeedMultiplier;

            _tickTimer += deltaTime;
            if (_tickTimer >= currentTickRate)
            {
                _tickTimer = 0f;
                DispatchMarch();
                EvaluateGameRules();
            }
        }

        private void DispatchMarch()
        {
            if (_ctx.DrillShader != null && _kernelId != -1)
            {
                // --- FULL GPU PIPELINE ---
                _ctx.DrillShader.SetBuffer(_kernelId, "_SoldierBufferRead", _ctx.GetReadBuffer());
                _ctx.DrillShader.SetBuffer(_kernelId, "_SoldierBufferWrite", _ctx.GetWriteBuffer());
                _ctx.DrillShader.SetVector("_HeadDirection", _ctx.CurrentDirection);
                _ctx.DrillShader.SetFloat("_GridSpacing", _ctx.GridSpacing);
                _ctx.DrillShader.SetInt("_SnakeLength", _ctx.CurrentArmySize);

                int threadGroups = Mathf.CeilToInt(_ctx.CurrentArmySize / 64f);
                _ctx.DrillShader.Dispatch(_kernelId, threadGroups, 1, 1);
                _ctx.SwapBuffers();
            }
            else
            {
                // --- CPU FALLBACK (FOR EDITOR STANDALONE PREVIEW) ---
                // Grabs only the active soldiers to keep editor fast
                SoldierGPUData[] activeData = new SoldierGPUData[_ctx.CurrentArmySize];
                _ctx.GetReadBuffer().GetData(activeData, 0, 0, _ctx.CurrentArmySize);

                // Body Follows
                for (int i = _ctx.CurrentArmySize - 1; i > 0; i--)
                {
                    activeData[i] = activeData[i - 1];
                }

                // Head Moves
                activeData[0].Position += _ctx.CurrentDirection * _ctx.GridSpacing;

                _ctx.GetWriteBuffer().SetData(activeData, 0, 0, _ctx.CurrentArmySize);
                _ctx.SwapBuffers();
            }
        }

        private void EvaluateGameRules()
        {
            // Read Head position from the active Read Buffer
            SoldierGPUData[] headData = new SoldierGPUData[1];
            _ctx.GetReadBuffer().GetData(headData, 0, 0, 1);
            Vector3 headPos = headData[0].Position;

            // DID WE HIT THE GHOST SOLDIER?
            if (Vector3.Distance(headPos, _ctx.RecruitPosition) < (_ctx.GridSpacing * 0.5f))
            {
                _ctx.CurrentArmySize++;
                SpawnRecruit();
            }
        }

        private void SpawnRecruit()
        {
            int x = Random.Range(-_ctx.MapWidth / 4, _ctx.MapWidth / 4);
            int z = Random.Range(-_ctx.MapHeight / 4, _ctx.MapHeight / 4);
            _ctx.RecruitPosition = new Vector3(x * _ctx.GridSpacing, 0, z * _ctx.GridSpacing);
        }
    }
}