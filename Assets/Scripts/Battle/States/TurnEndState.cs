using TurnBasedGame.Battle.Core;

namespace TurnBasedGame.Battle.States
{
    public sealed class TurnEndState : IBattleState
    {
        private readonly BattleStateMachine _sm;
        private readonly BattleContext _ctx;

        public TurnEndState(BattleStateMachine sm, BattleContext ctx)
        {
            _sm = sm;
            _ctx = ctx;
        }

        public void Enter()
        {
            // ターン終了：CD/状態異常の残りターン減少（全ユニット）
            for (int i = 0; i < _ctx.AllUnits.Count; i++)
                _ctx.AllUnits[i].OnTurnEnd();

            _ctx.Logger.Log($"=== Turn {_ctx.TurnNumber} End ===");

            // 次ターン開始へ
            _sm.ChangeState(new TurnStartState(_sm, _ctx));
        }

        public void Tick() { }
        public void Exit() { }
    }
}