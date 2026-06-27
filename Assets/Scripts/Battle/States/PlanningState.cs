using UnityEngine;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.UI;
using TurnBasedGame.Battle.Planning;
using TurnBasedGame.Battle.Services;

namespace TurnBasedGame.Battle.States
{
    public sealed class PlanningState : IBattleState
    {
        private readonly BattleStateMachine _sm;
        private readonly BattleContext _ctx;

        public PlanningState(BattleStateMachine sm, BattleContext ctx)
        {
            _sm = sm;
            _ctx = ctx;
        }

        public void Enter()
        {
            _ctx.Logger.Log($"=== Planning Turn {_ctx.TurnNumber} ===");

            BattleGridView.Instance?.BindGrid(_ctx.Grid);
            PlanningSelectionController.Instance?.BeginPlanning(_ctx);
            PathPreviewRenderer.Instance?.BeginPlanning(_ctx);
            PathDragInputController.Instance?.BeginPlanning(_ctx);
            SkillSelectionPanel.Instance?.BeginPlanning(_ctx);
            TargetSelectionController.Instance?.BeginPlanning(_ctx);
            TargetHighlightRenderer.Instance?.BeginPlanning();
            DebugEnemyPlanningService.BuildPlans(_ctx);
            PlannedActionHoverPreview.Instance?.BeginPlanning(_ctx);
            BattlePhaseBanner.Instance?.ShowPlanning();
        }

        public void Tick()
        {
            PlanningSelectionController.Instance?.TickPlanning();
            PathDragInputController.Instance?.TickPlanning();
            TargetSelectionController.Instance?.TickPlanning();
            PlannedActionHoverPreview.Instance?.TickPlanning();

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                ExecutePlans();
            }
        }

        public void Exit()
        {
            TargetHighlightRenderer.Instance?.EndPlanning();
            TargetSelectionController.Instance?.EndPlanning();
            SkillSelectionPanel.Instance?.EndPlanning();
            PathDragInputController.Instance?.EndPlanning();
            PathPreviewRenderer.Instance?.EndPlanning();
            PlanningSelectionController.Instance?.EndPlanning();
            PlannedActionHoverPreview.Instance?.EndPlanning();
        }

        public void ExecutePlans()
        {
            EnsureAllAlliesHavePlans();

            AutoTurretPlanningService.BuildPlans(_ctx);

            _ctx.TurnOrder = TurnOrderService.BuildTurnOrder(_ctx);
            _ctx.TurnIndex = 0;

            _sm.ChangeState(new ResolveTurnState(_sm, _ctx));
        }

        private void EnsureAllAlliesHavePlans()
        {
            for (int i = 0; i < _ctx.Allies.Count; i++)
            {
                var unit = _ctx.Allies[i];
                if (unit == null) continue;
                if (!unit.IsAlive) continue;

                if (!_ctx.TryGetPlannedAction(unit, out _))
                    _ctx.SetPlannedAction(unit, PlannedUnitAction.Wait());
            }
        }
    }
}