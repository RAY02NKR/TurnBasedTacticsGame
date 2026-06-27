using UnityEngine;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Services;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.Skills.Effects;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Skills.Effects
{
    [CreateAssetMenu(
        fileName = "KnockbackAndDamageEffect",
        menuName = "Battle/Skills/Effects/KnockbackAndDamage")]
    public sealed class KnockbackAndDamageEffectDefinition : SkillEffectDefinition
    {
        [Min(0)] public int baseDamage = 0;
        [Min(1)] public int maxDistance = 4;

        public override void Apply(SkillEffectContext ctx)
        {
            if (ctx == null) return;
            if (ctx.Actor == null) return;
            if (ctx.Skill == null) return;
            if (ctx.Battle == null) return;
            if (ctx.Battle.Grid == null) return;

            switch (ctx.Skill.targetMode)
            {
                case TargetMode.SingleEnemy:
                    ApplyToTarget(ctx, ctx.Target);
                    break;

                case TargetMode.Position:
                    ApplyToPositionTarget(ctx);
                    break;

                case TargetMode.LineDirection:
                    ApplyToLineTargets(ctx);
                    break;
            }
        }

        private void ApplyToPositionTarget(SkillEffectContext ctx)
        {
            if (!ctx.TargetPosition.HasValue)
                return;

            var pos = ctx.TargetPosition.Value;

            if (!ctx.Battle.Grid.TryGetUnitAt(pos, out var target))
                return;

            ApplyToTarget(ctx, target);
        }

        private void ApplyToLineTargets(SkillEffectContext ctx)
        {
            var origin = ctx.Actor.Position;

            var currentAction = ctx.Battle.GetPlannedActionOrWait(ctx.Actor);
            if (currentAction.HasMove && currentAction.MovePath != null)
                origin = currentAction.MovePath.End;

            var targets = DirectionalSkillTargetingService.GetUnitsOnSelectedLineFromPosition(
                ctx.Battle,
                ctx.Actor,
                origin,
                ctx.Skill.range,
                ctx.TargetDirection);

            for (int i = 0; i < targets.Count; i++)
            {
                ApplyToTarget(ctx, targets[i]);
            }
        }

        private void ApplyToTarget(SkillEffectContext ctx, UnitRuntime target)
        {
            if (target == null) return;
            if (!target.IsAlive) return;
            if (target.Team == ctx.Actor.Team) return;

            if (baseDamage > 0)
                target.TakeDamage(baseDamage);

            if (!target.IsAlive)
                return;

            var result = BattleGridForcedMovementService.Knockback(
                ctx.Battle.Grid,
                ctx.Actor,
                target,
                maxDistance);

            if (result.HitSolidObject)
            {
                int bonusDamage = Mathf.Max(1, Mathf.FloorToInt(target.MaxHP * 0.1f));
                target.TakeDamage(bonusDamage);
            }
        }
    }
}