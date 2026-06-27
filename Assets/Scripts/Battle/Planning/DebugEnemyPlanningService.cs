using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Planning
{
    public static class DebugEnemyPlanningService
    {
        public static void BuildPlans(BattleContext ctx)
        {
            if (ctx == null)
                return;

            for (int i = 0; i < ctx.Enemies.Count; i++)
            {
                var unit = ctx.Enemies[i];
                if (unit == null) continue;
                if (!unit.IsAlive) continue;

                var action = BuildAction(ctx, unit);
                ctx.SetPlannedAction(unit, action);

                ctx.Logger.Log($"[EnemyPlan] {unit.Name} -> {FormatAction(action)}");
            }
        }

        private static PlannedUnitAction BuildAction(BattleContext ctx, UnitRuntime unit)
        {
            if (!unit.IsPlaced)
                return PlannedUnitAction.Wait();

            var target = FindNearestPlacedAlly(ctx, unit);
            if (target == null)
                return PlannedUnitAction.Wait();

            if (TryBuildSkillPlanFromPosition(ctx, unit, target, unit.Position, out var immediatePlan))
            {
                return PlannedUnitAction.SkillOnly(
                    immediatePlan.Use,
                    immediatePlan.TargetUnit,
                    immediatePlan.TargetPosition,
                    immediatePlan.TargetDirection);
            }

            int plannedStep = GetPlannedStep(unit);

            if (TryBuildMoveAndSkillPlan(ctx, unit, target, plannedStep, out var moveAndSkillAction))
                return moveAndSkillAction;

            if (SimpleApproachPathBuilder.TryBuildPath(ctx, unit, target.Position, out var path, plannedStep))
                return PlannedUnitAction.MoveOnly(path);

            return PlannedUnitAction.Wait();
        }

        private static int GetPlannedStep(UnitRuntime unit)
        {
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

            return plannedStep;
        }

        private static bool TryBuildMoveAndSkillPlan(
            BattleContext ctx,
            UnitRuntime unit,
            UnitRuntime target,
            int plannedStep,
            out PlannedUnitAction action)
        {
            action = null;

            if (TryBuildMoveAndSkillPlanWithActiveSkill(ctx, unit, target, plannedStep, out action))
                return true;

            if (TryBuildMoveAndSkillPlanWithBasicSkill(ctx, unit, target, plannedStep, out action))
                return true;

            return false;
        }

        private static bool TryBuildMoveAndSkillPlanWithActiveSkill(
            BattleContext ctx,
            UnitRuntime unit,
            UnitRuntime target,
            int plannedStep,
            out PlannedUnitAction action)
        {
            action = null;

            for (int slot = 0; slot < unit.ActiveSkills.Length; slot++)
            {
                var skill = unit.ActiveSkills[slot];
                if (skill == null) continue;
                if (!unit.CanUseActiveSkill(slot)) continue;

                var use = SkillUse.Active(slot, skill);

                if (TryFindMoveAndSkillAction(ctx, unit, target, use, plannedStep, out action))
                    return true;
            }

            return false;
        }

        private static bool TryBuildMoveAndSkillPlanWithBasicSkill(
            BattleContext ctx,
            UnitRuntime unit,
            UnitRuntime target,
            int plannedStep,
            out PlannedUnitAction action)
        {
            action = null;

            var basic = unit.BasicAttack;
            if (basic == null)
                return false;

            if (!unit.CanUseSkill(basic))
                return false;

            var use = SkillUse.Basic(basic);

            return TryFindMoveAndSkillAction(ctx, unit, target, use, plannedStep, out action);
        }

        private static bool TryFindMoveAndSkillAction(
            BattleContext ctx,
            UnitRuntime unit,
            UnitRuntime target,
            SkillUse use,
            int plannedStep,
            out PlannedUnitAction action)
        {
            action = null;

            if (use.Skill == null)
                return false;

            GridPath bestPath = null;
            EnemySkillPlan bestPlan = default;
            int bestCost = int.MaxValue;

            foreach (var pos in ctx.Grid.GetAllPositions())
            {
                if (!CanStandAt(ctx, unit, pos))
                    continue;

                if (!TryBuildSkillPlanFromPosition(ctx, unit, target, pos, use, out var skillPlan))
                    continue;

                if (!SimpleApproachPathBuilder.TryBuildPath(ctx, unit, pos, out var path, plannedStep))
                    continue;

                if (path.End != pos)
                    continue;

                if (path.MoveCost >= bestCost)
                    continue;

                bestPath = path;
                bestPlan = skillPlan;
                bestCost = path.MoveCost;
            }

            if (bestPath == null)
                return false;

            action = PlannedUnitAction.MoveAndSkill(
                bestPath,
                bestPlan.Use,
                bestPlan.TargetUnit,
                bestPlan.TargetPosition,
                bestPlan.TargetDirection);

            return true;
        }

        private static bool TryBuildSkillPlanFromPosition(
            BattleContext ctx,
            UnitRuntime unit,
            UnitRuntime target,
            GridPosition fromPosition,
            out EnemySkillPlan plan)
        {
            plan = default;

            for (int slot = 0; slot < unit.ActiveSkills.Length; slot++)
            {
                var skill = unit.ActiveSkills[slot];
                if (skill == null) continue;
                if (!unit.CanUseActiveSkill(slot)) continue;

                var use = SkillUse.Active(slot, skill);

                if (TryBuildSkillPlanFromPosition(ctx, unit, target, fromPosition, use, out plan))
                    return true;
            }

            var basic = unit.BasicAttack;
            if (basic == null)
                return false;

            if (!unit.CanUseSkill(basic))
                return false;

            return TryBuildSkillPlanFromPosition(
                ctx,
                unit,
                target,
                fromPosition,
                SkillUse.Basic(basic),
                out plan);
        }

        private static bool TryBuildSkillPlanFromPosition(
            BattleContext ctx,
            UnitRuntime unit,
            UnitRuntime target,
            GridPosition fromPosition,
            SkillUse use,
            out EnemySkillPlan plan)
        {
            plan = default;

            var skill = use.Skill;
            if (skill == null)
                return false;

            switch (skill.targetMode)
            {
                case TargetMode.SingleEnemy:
                    return TryBuildSingleEnemyPlan(ctx, target, fromPosition, use, out plan);

                case TargetMode.Position:
                    return TryBuildPositionPlan(ctx, target, fromPosition, use, out plan);

                case TargetMode.LineDirection:
                    return TryBuildLineDirectionPlan(ctx, target, fromPosition, use, out plan);
            }

            return false;
        }

        private static bool TryBuildSingleEnemyPlan(
            BattleContext ctx,
            UnitRuntime target,
            GridPosition fromPosition,
            SkillUse use,
            out EnemySkillPlan plan)
        {
            plan = default;

            var skill = use.Skill;
            if (skill == null)
                return false;

            if (!BattleGridRangeService.IsInRange(ctx.Grid, fromPosition, target.Position, skill.range))
                return false;

            plan = new EnemySkillPlan
            {
                Use = use,
                TargetUnit = target,
                TargetPosition = null,
                TargetDirection = null
            };

            return true;
        }

        private static bool TryBuildPositionPlan(
            BattleContext ctx,
            UnitRuntime target,
            GridPosition fromPosition,
            SkillUse use,
            out EnemySkillPlan plan)
        {
            plan = default;

            var skill = use.Skill;
            if (skill == null)
                return false;

            if (!BattleGridRangeService.IsInRange(ctx.Grid, fromPosition, target.Position, skill.range))
                return false;

            plan = new EnemySkillPlan
            {
                Use = use,
                TargetUnit = null,
                TargetPosition = target.Position,
                TargetDirection = null
            };

            return true;
        }

        private static bool TryBuildLineDirectionPlan(
            BattleContext ctx,
            UnitRuntime target,
            GridPosition fromPosition,
            SkillUse use,
            out EnemySkillPlan plan)
        {
            plan = default;

            var skill = use.Skill;
            if (skill == null)
                return false;

            if (fromPosition == target.Position)
                return false;

            if (!BattleGridRangeService.IsInRange(ctx.Grid, fromPosition, target.Position, skill.range))
                return false;

            if (!BattleGridRangeService.TryGetStraightDirection(fromPosition, target.Position, out var direction))
                return false;

            plan = new EnemySkillPlan
            {
                Use = use,
                TargetUnit = null,
                TargetPosition = null,
                TargetDirection = direction
            };

            return true;
        }

        private static bool CanStandAt(BattleContext ctx, UnitRuntime unit, GridPosition pos)
        {
            if (!ctx.Grid.IsInside(pos))
                return false;

            if (ctx.Grid.IsBlockedBySolidObject(pos))
                return false;

            if (ctx.Grid.TryGetUnitAt(pos, out var existing) && existing != unit)
                return false;

            return true;
        }

        private static UnitRuntime FindNearestPlacedAlly(BattleContext ctx, UnitRuntime unit)
        {
            UnitRuntime best = null;
            int bestDistance = int.MaxValue;

            for (int i = 0; i < ctx.Allies.Count; i++)
            {
                var ally = ctx.Allies[i];
                if (ally == null) continue;
                if (!ally.IsAlive) continue;
                if (!ally.IsPlaced) continue;

                int dist = ctx.Grid.GetManhattanDistance(unit.Position, ally.Position);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    best = ally;
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
                string skillName = action.SkillUse.Skill != null
                    ? action.SkillUse.Skill.skillName
                    : "UnknownSkill";

                if (action.HasTargetUnit)
                    return $"Move {action.MovePath.Start} -> {action.MovePath.End}, {skillName} -> Unit:{action.TargetUnit.Name}";

                if (action.HasTargetPosition)
                    return $"Move {action.MovePath.Start} -> {action.MovePath.End}, {skillName} -> Pos:{action.TargetPosition.Value}";

                if (action.HasTargetDirection)
                    return $"Move {action.MovePath.Start} -> {action.MovePath.End}, {skillName} -> Direction:{action.TargetDirection.Value}";

                return $"Move {action.MovePath.Start} -> {action.MovePath.End}, {skillName}";
            }

            if (action.HasMove && action.MovePath != null)
                return $"Move {action.MovePath.Start} -> {action.MovePath.End}";

            if (action.HasSkill)
            {
                string skillName = action.SkillUse.Skill != null
                    ? action.SkillUse.Skill.skillName
                    : "UnknownSkill";

                if (action.HasTargetUnit)
                    return $"{skillName} -> Unit:{action.TargetUnit.Name}";

                if (action.HasTargetPosition)
                    return $"{skillName} -> Pos:{action.TargetPosition.Value}";

                if (action.HasTargetDirection)
                    return $"{skillName} -> Direction:{action.TargetDirection.Value}";

                return skillName;
            }

            return "Wait";
        }

        private struct EnemySkillPlan
        {
            public SkillUse Use;
            public UnitRuntime TargetUnit;
            public GridPosition? TargetPosition;
            public GridDirection? TargetDirection;
        }
    }
}