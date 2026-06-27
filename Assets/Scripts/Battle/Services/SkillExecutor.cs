using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.Skills.Effects;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Services
{
    public static class SkillExecutor
    {
        public static void Execute(
            BattleContext battle,
            UnitRuntime actor,
            SkillUse use,
            UnitRuntime targetUnit = null,
            GridPosition? targetPosition = null,
            GridDirection? targetDirection = null)
        {
            var skill = use.Skill;
            if (skill == null)
            {
                battle.Logger.Warn($"[Act] {actor.Name} has no skill assigned.");
                return;
            }

            var target = ResolveTarget(battle, actor, skill, targetUnit, targetPosition);

            if (skill.targetMode == TargetMode.SingleEnemy && target == null)
            {
                battle.Logger.Log($"[Act] {actor.Name} uses {skill.skillName} but no target.");
                return;
            }

            if (skill.targetMode == TargetMode.Position && !targetPosition.HasValue)
            {
                battle.Logger.Log($"[Act] {actor.Name} uses {skill.skillName} but no target position.");
                return;
            }

            if (skill.targetMode == TargetMode.LineDirection && !targetDirection.HasValue)
            {
                battle.Logger.Log($"[Act] {actor.Name} uses {skill.skillName} but no target direction.");
                return;
            }

            var ctx = new SkillEffectContext(battle, actor, target, skill, targetPosition, targetDirection);

            if (skill.effects == null || skill.effects.Count == 0)
            {
                if (target != null)
                    ctx.LastDamageResult = DamageService.Deal(actor, target, skill);
            }
            else
            {
                for (int i = 0; i < skill.effects.Count; i++)
                {
                    var eff = skill.effects[i];
                    if (eff == null) continue;
                    eff.Apply(ctx);
                }
            }

            if (use.IsActive)
                actor.PutActiveSkillOnCooldown(use.ActiveSlot);

            if (target != null)
            {
                if (ctx.LastDamageResult.Amount > 0)
                    battle.Logger.Log($"[Act] {actor.Name} uses {skill.skillName} -> {target.Name} dmg={ctx.LastDamageResult.Amount} HP={ctx.LastDamageResult.TargetHPAfter}");
                else
                    battle.Logger.Log($"[Act] {actor.Name} uses {skill.skillName} -> {target.Name}");
            }
            else if (targetPosition.HasValue)
            {
                battle.Logger.Log($"[Act] {actor.Name} uses {skill.skillName} -> {targetPosition.Value}");
            }
            else if (targetDirection.HasValue)
            {
                battle.Logger.Log($"[Act] {actor.Name} uses {skill.skillName} -> {targetDirection.Value}");
            }
            else
            {
                battle.Logger.Log($"[Act] {actor.Name} uses {skill.skillName}");
            }
        }

        private static UnitRuntime ResolveTarget(
            BattleContext battle,
            UnitRuntime actor,
            SkillDefinition skill,
            UnitRuntime targetUnit,
            GridPosition? targetPosition)
        {
            if (skill == null)
                return null;

            if (targetUnit != null)
            {
                if (!targetUnit.IsAlive || !targetUnit.IsPlaced || !actor.IsPlaced)
                    return null;

                if (!BattleGridRangeService.IsInRange(battle.Grid, actor.Position, targetUnit.Position, skill.range))
                    return null;

                return targetUnit;
            }

            if (targetPosition.HasValue)
            {
                if (!actor.IsPlaced)
                    return null;

                var pos = targetPosition.Value;

                if (!battle.Grid.IsInside(pos))
                    return null;

                if (!BattleGridRangeService.IsInRange(battle.Grid, actor.Position, pos, skill.range))
                    return null;

                if (skill.targetMode == TargetMode.Position)
                    return null;

                if (!battle.Grid.TryGetUnitAt(pos, out var target))
                    return null;

                if (target == null || !target.IsAlive)
                    return null;

                return target;
            }

            switch (skill.targetMode)
            {
                case TargetMode.SingleEnemy:
                    {
                        var enemyTeam = actor.Team == Team.Ally ? Team.Enemy : Team.Ally;
                        var autoTarget = UnitQueryService.FindFirstAlive(battle, enemyTeam);

                        if (autoTarget == null || !autoTarget.IsPlaced || !actor.IsPlaced)
                            return null;

                        if (!BattleGridRangeService.IsInRange(battle.Grid, actor.Position, autoTarget.Position, skill.range))
                            return null;

                        return autoTarget;
                    }

                default:
                    return null;
            }
        }
    }
}