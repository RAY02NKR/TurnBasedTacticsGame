using UnityEngine;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.UI;

namespace TurnBasedGame.Battle.States
{
    public sealed class TurnStartState : IBattleState
    {
        private const float TurnStartWaitSeconds = 0.85f;

        private readonly BattleStateMachine _sm;
        private readonly BattleContext _ctx;

        private float _timer;
        private bool _initialized;

        public TurnStartState(BattleStateMachine sm, BattleContext ctx)
        {
            _sm = sm;
            _ctx = ctx;
        }

        public void Enter()
        {
            _timer = 0f;
            _initialized = false;

            _ctx.TurnNumber++;
            BattlePhaseBanner.Instance?.ShowTurnStart(_ctx.TurnNumber);

            _ctx.TurnIndex = 0;
            _ctx.ClearPlannedActions();

            for (int i = 0; i < _ctx.AllUnits.Count; i++)
            {
                var unit = _ctx.AllUnits[i];
                if (!unit.IsAlive) continue;

                unit.OnTurnStart();
            }

            _initialized = true;
        }

        public void Tick()
        {
            if (!_initialized)
                return;

            _timer += Time.deltaTime;

            if (_timer < TurnStartWaitSeconds)
                return;

            _sm.ChangeState(new PlanningState(_sm, _ctx));
        }

        public void Exit() { }
    }
}