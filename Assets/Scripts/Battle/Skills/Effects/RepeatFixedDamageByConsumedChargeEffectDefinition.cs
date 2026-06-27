using System.Collections.Generic;
using UnityEngine;
using TurnBasedGame.Battle.Services;
using TurnBasedGame.Battle.StatusEffects;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Skills.Effects
{
    [CreateAssetMenu(
        fileName = "RepeatFixedDamageByConsumedChargeEffect",
        menuName = "Battle/Skills/Effects/RepeatFixedDamageByConsumedCharge")]
    public sealed class RepeatFixedDamageByConsumedChargeEffectDefinition : SkillEffectDefinition
    {
        [Min(0)] public int damagePerHit = 5;

        public override void Apply(SkillEffectContext ctx)
        {
            if (ctx == null) return;
            if (ctx.Actor == null) return;
            if (ctx.Battle == null) return;
            if (ctx.Skill == null) return;
            if (ctx.ConsumedCharge <= 0) return;
            if (damagePerHit <= 0) return;

            var targets = GetTargets(ctx);
            if (targets.Count == 0) return;

            for (int t = 0; t < targets.Count; t++)
            {
                var target = targets[t];

                if (target == null) continue;
                if (!target.IsAlive) continue;
                if (target.Team == ctx.Actor.Team) continue;

                ApplyRepeatedDamage(ctx, target);
            }
        }

        private void ApplyRepeatedDamage(SkillEffectContext ctx, UnitRuntime target)
        {
            for (int i = 0; i < ctx.ConsumedCharge; i++)
            {
                if (target == null) break;
                if (!target.IsAlive) break;

                int before = target.CurrentHP;
                target.TakeDamage(damagePerHit);
                int actualDamage = before - target.CurrentHP;

                if (actualDamage <= 0)
                    continue;

                ctx.LastDamageResult = new DamageResult(
                    actualDamage,
                    before,
                    target.CurrentHP);

                StatusService.ExecuteDamagedEffects(
                    target,
                    ctx.Actor,
                    actualDamage,
                    isSkillDamage: true);
            }
        }

        private List<UnitRuntime> GetTargets(SkillEffectContext ctx)
        {
            var targets = new List<UnitRuntime>();

            switch (ctx.Skill.targetMode)
            {
                case TargetMode.SingleEnemy:
                    if (ctx.Target != null)
                        targets.Add(ctx.Target);
                    break;

                case TargetMode.Position:
                    AddPositionTarget(ctx, targets);
                    break;

                case TargetMode.LineDirection:
                    AddLineTargets(ctx, targets);
                    break;
            }

            return targets;
        }

        private void AddPositionTarget(SkillEffectContext ctx, List<UnitRuntime> targets)
        {
            if (!ctx.TargetPosition.HasValue)
                return;

            if (!ctx.Battle.Grid.TryGetUnitAt(ctx.TargetPosition.Value, out var target))
                return;

            if (target != null)
                targets.Add(target);
        }

        private void AddLineTargets(SkillEffectContext ctx, List<UnitRuntime> targets)
        {
            var origin = ctx.Actor.Position;

            var currentAction = ctx.Battle.GetPlannedActionOrWait(ctx.Actor);
            if (currentAction.HasMove && currentAction.MovePath != null)
                origin = currentAction.MovePath.End;

            var lineTargets = DirectionalSkillTargetingService.GetUnitsOnSelectedLineFromPosition(
                ctx.Battle,
                ctx.Actor,
                origin,
                ctx.Skill.range,
                ctx.TargetDirection);

            for (int i = 0; i < lineTargets.Count; i++)
            {
                if (lineTargets[i] != null)
                    targets.Add(lineTargets[i]);
            }
        }
    }
}