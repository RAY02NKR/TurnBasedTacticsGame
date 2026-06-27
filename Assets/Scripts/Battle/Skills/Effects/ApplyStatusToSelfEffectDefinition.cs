using UnityEngine;
using TurnBasedGame.Battle.StatusEffects;

namespace TurnBasedGame.Battle.Skills.Effects
{
    [CreateAssetMenu(menuName = "TurnBasedGame/SkillEffects/ApplyStatusToSelf")]
    public sealed class ApplyStatusToSelfEffectDefinition : SkillEffectDefinition
    {
        public StatusEffectDefinition status;
        [Min(1)] public int stacksToAdd = 1;

        public override void Apply(SkillEffectContext ctx)
        {
            StatusService.Apply(ctx.Actor, status, stacksToAdd);
        }
    }
}