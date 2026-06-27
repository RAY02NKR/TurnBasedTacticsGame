using UnityEngine;

namespace TurnBasedGame.Battle.StatusEffects.Actions
{
    [CreateAssetMenu(
        fileName = "DealFixedDamageStatusAction",
        menuName = "Battle/StatusEffects/Actions/DealFixedDamage")]
    public sealed class DealFixedDamageStatusActionDefinition : StatusEffectActionDefinition
    {
        [Min(0)] public int damage = 0;

        public override void Execute(StatusEffectActionContext ctx, StatusEffectApplyBuffer buffer)
        {
            if (ctx == null) return;
            if (ctx.Owner == null) return;
            if (damage <= 0) return;

            ctx.Owner.TakeDamage(damage);
        }
    }
}