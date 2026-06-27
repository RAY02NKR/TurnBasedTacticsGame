using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Planning
{
    public static class ManualPlayerPlanningService
    {
        public static bool TryBuildPlans(BattleContext ctx)
        {
            var provider = DebugManualPlanningProvider.Instance;
            if (provider == null || !provider.UseManualPlanning)
                return false;

            ctx.ClearPlannedActions();

            var entries = provider.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null) continue;
                if (!entry.enabled) continue;
                if (entry.planningTurn != ctx.TurnNumber) continue;
                if (entry.allyIndex < 0 || entry.allyIndex >= ctx.Allies.Count) continue;

                var unit = ctx.Allies[entry.allyIndex];
                if (unit == null || !unit.IsAlive) continue;
                if (unit.IsTurret) continue;
                if (unit.IsTurret) continue;

                var action = BuildAction(ctx, unit, entry);
                ctx.SetPlannedAction(unit, action);
                ctx.Logger.Log($"[ManualPlan T{entry.planningTurn}] {unit.Name} -> {FormatAction(action)}");
            }

            for (int i = 0; i < ctx.Allies.Count; i++)
            {
                var unit = ctx.Allies[i];
                if (unit == null || !unit.IsAlive) continue;
                if (unit.IsTurret) continue;

                if (!ctx.TryGetPlannedAction(unit, out _))
                    ctx.SetPlannedAction(unit, PlannedUnitAction.Wait());
            }

            return true;
        }

        private static PlannedUnitAction BuildAction(BattleContext ctx, UnitRuntime unit, DebugManualPlanEntry entry)
        {
            GridPath path = null;

            if (entry.movePath != null && entry.movePath.Count >= 2)
                path = new GridPath(entry.movePath);

            if (!entry.useSkill)
            {
                if (path != null) return PlannedUnitAction.MoveOnly(path);
                return PlannedUnitAction.Wait();
            }

            SkillUse skillUse = default;

            if (entry.useBasicAttack)
            {
                if (unit.BasicAttack != null)
                    skillUse = SkillUse.Basic(unit.BasicAttack);
            }
            else
            {
                int slot = entry.activeSkillSlot;
                if (slot >= 0 && slot < unit.ActiveSkills.Length)
                {
                    var skill = unit.ActiveSkills[slot];
                    if (skill != null)
                        skillUse = SkillUse.Active(slot, skill);
                }
            }

            UnitRuntime targetUnit = ResolveTargetUnit(ctx, entry);
            GridPosition? targetPosition = entry.useTargetPosition ? entry.targetPosition : null;

            if (path != null)
                return PlannedUnitAction.MoveAndSkill(path, skillUse, targetUnit, targetPosition);

            return PlannedUnitAction.SkillOnly(skillUse, targetUnit, targetPosition);
        }

        private static UnitRuntime ResolveTargetUnit(BattleContext ctx, DebugManualPlanEntry entry)
        {
            if (!entry.useTargetUnit)
                return null;

            if (entry.targetGroup == DebugManualTargetGroup.Enemy)
            {
                if (entry.targetUnitIndex < 0 || entry.targetUnitIndex >= ctx.Enemies.Count)
                    return null;

                return ctx.Enemies[entry.targetUnitIndex];
            }

            if (entry.targetUnitIndex < 0 || entry.targetUnitIndex >= ctx.Allies.Count)
                return null;

            return ctx.Allies[entry.targetUnitIndex];
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
                if (action.HasTargetUnit)
                    return $"Skill -> Unit:{action.TargetUnit.Name}";

                if (action.HasTargetPosition)
                    return $"Skill -> Pos:{action.TargetPosition.Value}";

                return "Skill";
            }

            return "Wait";
        }
    }
}