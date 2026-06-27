using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Planning
{
    public static class AutoTurretPlanningService
    {
        public static void BuildPlans(BattleContext ctx)
        {
            if (ctx == null)
                return;

            for (int i = 0; i < ctx.Allies.Count; i++)
            {
                var unit = ctx.Allies[i];
                if (unit == null) continue;
                if (!unit.IsAlive) continue;
                if (!unit.IsTurret) continue;

                var action = BuildAction(ctx, unit);
                ctx.SetPlannedAction(unit, action);
                ctx.Logger.Log($"[TurretPlan] {unit.Name} -> {FormatAction(action)}");
            }
        }

        private static PlannedUnitAction BuildAction(BattleContext ctx, UnitRuntime unit)
        {
            if (!unit.IsPlaced)
                return PlannedUnitAction.Wait();

            var enemy = FindNearestPlacedEnemy(ctx, unit);
            if (enemy == null)
                return PlannedUnitAction.Wait();

            var current = unit.Position;

            var basic = unit.BasicAttack;
            if (basic == null) return PlannedUnitAction.Wait();
            if (!unit.CanUseSkill(basic)) return PlannedUnitAction.Wait();
            if (!BattleGridRangeService.IsInRange(ctx.Grid, current, enemy.Position, basic.range))
                return PlannedUnitAction.Wait();

            return PlannedUnitAction.SkillOnly(SkillUse.Basic(basic), enemy, enemy.Position);
        }

        private static UnitRuntime FindNearestPlacedEnemy(BattleContext ctx, UnitRuntime unit)
        {
            UnitRuntime best = null;
            int bestDistance = int.MaxValue;

            for (int i = 0; i < ctx.Enemies.Count; i++)
            {
                var enemy = ctx.Enemies[i];
                if (enemy == null) continue;
                if (!enemy.IsAlive) continue;
                if (!enemy.IsPlaced) continue;

                int dist = ctx.Grid.GetManhattanDistance(unit.Position, enemy.Position);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    best = enemy;
                }
            }

            return best;
        }

        private static string FormatAction(PlannedUnitAction action)
        {
            if (action == null)
                return "Null";

            if (action.HasTargetUnit)
                return $"Skill -> Unit:{action.TargetUnit.Name}";

            if (action.HasTargetPosition)
                return $"Skill -> Pos:{action.TargetPosition.Value}";

            return "Wait";
        }
    }
}