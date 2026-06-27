// Assets/Scripts/Battle/States/EndCheckState.cs
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.States
{
    public sealed class EndCheckState : IBattleState
    {
        private readonly BattleStateMachine _sm;
        private readonly BattleContext _ctx;

        public EndCheckState(BattleStateMachine sm, BattleContext ctx)
        {
            _sm = sm;
            _ctx = ctx;
        }

        public void Enter()
        {
            if (_ctx.IsAllDead(Team.Ally))
            {
                _ctx.Logger.Log("=== Defeat (All Allies Dead) ===");
                _ctx.IsBattleOver = true;
                _sm.ChangeState(new BattleEndState(_ctx));
                return;
            }

            if (_ctx.IsAllDead(Team.Enemy))
            {
                _ctx.Logger.Log("=== Victory (All Enemies Dead) ===");
                _ctx.IsBattleOver = true;
                _sm.ChangeState(new BattleEndState(_ctx));
                return;
            }

            // 続行：次の行動へ
            if (_ctx.TurnIndex >= _ctx.TurnOrder.Count)
                _sm.ChangeState(new TurnEndState(_sm, _ctx));
            else
                _sm.ChangeState(new ResolveTurnState(_sm, _ctx));
        }

        public void Tick() { }
        public void Exit() { }
    }
}
