using UnityEngine;
using TurnBasedGame.Battle.Services;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Skills.Effects
{
    [CreateAssetMenu(menuName = "TurnBasedGame/SkillEffects/Damage")]
    public sealed class DamageEffectDefinition : SkillEffectDefinition
    {
        public override void Apply(SkillEffectContext ctx)
        {
            if (ctx == null) return;
            if (ctx.Skill == null) return;
            if (ctx.Battle == null) return;
            if (ctx.Actor == null) return;

            switch (ctx.Skill.targetMode)
            {
                case TargetMode.SingleEnemy:
                    ApplySingleTargetDamage(ctx);
                    break;

                case TargetMode.Position:
                    ApplyPositionDamage(ctx);
                    break;

                case TargetMode.LineDirection:
                    ApplyLineDamage(ctx);
                    break;
            }
        }

        private void ApplySingleTargetDamage(SkillEffectContext ctx)
        {
            if (ctx.Target == null)
                return;

            if (!ctx.Target.IsAlive)
                return;

            ctx.LastDamageResult = DamageService.Deal(ctx.Actor, ctx.Target, ctx.Skill);
        }

        private void ApplyPositionDamage(SkillEffectContext ctx)
        {
            if (!ctx.TargetPosition.HasValue)
                return;

            var pos = ctx.TargetPosition.Value;

            if (!ctx.Battle.Grid.TryGetUnitAt(pos, out var target))
                return;

            if (target == null)
                return;

            if (!target.IsAlive)
                return;

            if (target.Team == ctx.Actor.Team)
                return;

            ctx.LastDamageResult = DamageService.Deal(ctx.Actor, target, ctx.Skill);
        }

        private void ApplyLineDamage(SkillEffectContext ctx)
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
                var target = targets[i];
                if (target == null) continue;
                if (!target.IsAlive) continue;
                if (target.Team == ctx.Actor.Team) continue;

                ctx.LastDamageResult = DamageService.Deal(ctx.Actor, target, ctx.Skill);
            }
        }
    }
}