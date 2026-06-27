using UnityEngine;
using TurnBasedGame.Battle.Services;
using TurnBasedGame.Battle.StatusEffects;

namespace TurnBasedGame.Battle.Skills.Effects
{
    [CreateAssetMenu(menuName = "TurnBasedGame/SkillEffects/ApplyStatusToTarget")]
    public sealed class ApplyStatusToTargetEffectDefinition : SkillEffectDefinition
    {
        public StatusEffectDefinition status;
        [Min(1)] public int stacksToAdd = 1;

        public override void Apply(SkillEffectContext ctx)
        {
            if (ctx.Skill == null)
                return;

            if (ctx.Skill.targetMode == TurnBasedGame.Battle.Skills.TargetMode.LineDirection)
            {
                ApplyStatusOnLine(ctx);
                return;
            }

            if (ctx.Target == null)
                return;

            StatusService.Apply(ctx.Target, status, stacksToAdd);
        }

        private void ApplyStatusOnLine(SkillEffectContext ctx)
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

                StatusService.Apply(target, status, stacksToAdd);
            }
        }
    }
}