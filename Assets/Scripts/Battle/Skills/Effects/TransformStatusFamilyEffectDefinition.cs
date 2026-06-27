using UnityEngine;
using TurnBasedGame.Battle.Services;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.StatusEffects;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Skills.Effects
{
    [CreateAssetMenu(
        fileName = "TransformStatusFamilyEffect",
        menuName = "Battle/Skills/Effects/TransformStatusFamily")]
    public sealed class TransformStatusFamilyEffectDefinition : SkillEffectDefinition
    {
        public StatusFamily sourceFamily = StatusFamily.None;
        public StatusEffectDefinition transformTo;

        public override void Apply(SkillEffectContext ctx)
        {
            if (ctx == null) return;
            if (ctx.Actor == null) return;
            if (ctx.Skill == null) return;
            if (ctx.Battle == null) return;
            if (transformTo == null) return;
            if (sourceFamily == StatusFamily.None) return;

            switch (ctx.Skill.targetMode)
            {
                case TargetMode.SingleEnemy:
                    TransformTarget(ctx.Target);
                    break;

                case TargetMode.Position:
                    TransformPositionTarget(ctx);
                    break;

                case TargetMode.LineDirection:
                    TransformLineTargets(ctx);
                    break;
            }
        }

        private void TransformPositionTarget(SkillEffectContext ctx)
        {
            if (!ctx.TargetPosition.HasValue)
                return;

            var pos = ctx.TargetPosition.Value;

            if (!ctx.Battle.Grid.TryGetUnitAt(pos, out var target))
                return;

            TransformTarget(target);
        }

        private void TransformLineTargets(SkillEffectContext ctx)
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
                TransformTarget(targets[i]);
            }
        }

        private void TransformTarget(UnitRuntime target)
        {
            if (target == null) return;
            if (!target.IsAlive) return;

            int stacks = target.GetStatusStacksByFamily(sourceFamily);
            if (stacks <= 0) return;

            target.RemoveStatusByFamily(sourceFamily);
            StatusService.Apply(target, transformTo, stacks);
        }
    }
}