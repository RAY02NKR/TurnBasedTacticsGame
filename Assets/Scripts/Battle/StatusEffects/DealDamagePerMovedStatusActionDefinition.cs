using UnityEngine;

namespace TurnBasedGame.Battle.StatusEffects.Actions
{
    [CreateAssetMenu(
        fileName = "DealDamagePerMovedStepStatusAction",
        menuName = "Battle/StatusEffects/Actions/DealDamagePerMovedStep")]
    public sealed class DealDamagePerMovedStepStatusActionDefinition : StatusEffectActionDefinition
    {
        [Min(0)] public int damagePerStepPerStack = 1;

        public override void Execute(StatusEffectActionContext ctx, StatusEffectApplyBuffer buffer)
        {
            if (ctx == null) return;
            if (ctx.Owner == null) return;
            if (ctx.MovedSteps <= 0) return;
            if (damagePerStepPerStack <= 0) return;

            int damage = damagePerStepPerStack * ctx.MovedSteps * ctx.Stacks;
            ctx.Owner.TakeDamage(damage);
        }
    }
}