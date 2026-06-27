using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Planning
{
    public static class DebugPlayerPlanningService
    {
        public static void BuildPlans(BattleContext ctx)
        {
            if (ctx == null)
                return;

            ctx.ClearPlannedActions();

            for (int i = 0; i < ctx.Allies.Count; i++)
            {
                var unit = ctx.Allies[i];
                if (unit == null) continue;
                if (!unit.IsAlive) continue;

                var action = BuildAction(ctx, unit);
                ctx.SetPlannedAction(unit, action);

                ctx.Logger.Log($"[Plan] {unit.Name} -> {FormatAction(action)}");
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

            var preferredSkillUse = TryBuildPreferredSkillUse(unit, enemy, current, ctx);
            if (preferredSkillUse.HasValue)
                return PlannedUnitAction.SkillOnly(preferredSkillUse.Value.use, enemy, preferredSkillUse.Value.targetPosition);

            int plannedStep = unit.Step;

            for (int slot = 0; slot < unit.ActiveSkills.Length; slot++)
            {
                var skill = unit.ActiveSkills[slot];
                if (skill == null) continue;
                if (!skill.modifiesStepDuringPlanning) continue;
                if (!unit.CanUseActiveSkill(slot)) continue;

                plannedStep = unit.Step + skill.planningStepAdd;
                break;
            }

            if (SimpleApproachPathBuilder.TryBuildPath(ctx, unit, enemy.Position, out var path, plannedStep))
            {
                var endPos = path.End;
                var moveSkillUse = TryBuildPreferredSkillUse(unit, enemy, endPos, ctx);

                if (moveSkillUse.HasValue)
                    return PlannedUnitAction.MoveAndSkill(path, moveSkillUse.Value.use, enemy, moveSkillUse.Value.targetPosition);

                return PlannedUnitAction.MoveOnly(path);
            }

            return PlannedUnitAction.Wait();
        }

        private static (SkillUse use, GridPosition targetPosition)? TryBuildPreferredSkillUse(
            UnitRuntime unit,
            UnitRuntime enemy,
            GridPosition fromPosition,
            BattleContext ctx)
        {
            var active = TryGetUsableActiveSkill(unit, enemy, fromPosition, ctx);
            if (active.HasValue)
                return active;

            var basic = TryGetUsableBasicSkill(unit, enemy, fromPosition, ctx);
            if (basic.HasValue)
                return basic;

            return null;
        }

        private static (SkillUse use, GridPosition targetPosition)? TryGetUsableActiveSkill(
            UnitRuntime unit,
            UnitRuntime enemy,
            GridPosition fromPosition,
            BattleContext ctx)
        {
            for (int slot = 0; slot < unit.ActiveSkills.Length; slot++)
            {
                var skill = unit.ActiveSkills[slot];
                if (skill == null) continue;
                if (!unit.CanUseActiveSkill(slot)) continue;
                if (!BattleGridRangeService.IsInRange(ctx.Grid, fromPosition, enemy.Position, skill.range)) continue;

                return (SkillUse.Active(slot, skill), enemy.Position);
            }

            return null;
        }

        private static (SkillUse use, GridPosition targetPosition)? TryGetUsableBasicSkill(
            UnitRuntime unit,
            UnitRuntime enemy,
            GridPosition fromPosition,
            BattleContext ctx)
        {
            var basic = unit.BasicAttack;
            if (basic == null) return null;
            if (!unit.CanUseSkill(basic)) return null;
            if (!BattleGridRangeService.IsInRange(ctx.Grid, fromPosition, enemy.Position, basic.range)) return null;

            return (SkillUse.Basic(basic), enemy.Position);
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

            if (action.HasMove && action.HasSkill && action.MovePath != null)
            {
                if (action.HasTargetUnit)
                    return $"Move {action.MovePath.Start} -> {action.MovePath.End}, Skill -> Unit:{action.TargetUnit.Name}";

                if (action.HasTargetPosition)
                    return $"Move {action.MovePath.Start} -> {action.MovePath.End}, Skill -> Pos:{action.TargetPosition.Value}";

                return $"Move {action.MovePath.Start} -> {action.MovePath.End}, Skill";
            }

            if (action.HasMove && action.MovePath != null)
                return $"Move {action.MovePath.Start} -> {action.MovePath.End}";

            if (action.HasSkill)
            {
                var skillName = action.SkillUse.Skill != null ? action.SkillUse.Skill.skillName : "UnknownSkill";

                if (action.HasTargetUnit)
                    return $"{skillName} -> Unit:{action.TargetUnit.Name}";

                if (action.HasTargetPosition)
                    return $"{skillName} -> Pos:{action.TargetPosition.Value}";

                return skillName;
            }

            return "Wait";
        }
    }
}