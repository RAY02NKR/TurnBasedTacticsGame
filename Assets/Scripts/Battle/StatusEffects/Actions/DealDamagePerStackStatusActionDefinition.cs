using UnityEngine;

namespace TurnBasedGame.Battle.StatusEffects.Actions
{
    [CreateAssetMenu(
        fileName = "DealDamagePerStackStatusAction",
        menuName = "Battle/StatusEffects/Actions/DealDamagePerStack")]
    public sealed class DealDamagePerStackStatusActionDefinition : StatusEffectActionDefinition
    {
        [Min(0)] public int damagePerStack = 1;

        public override void Execute(StatusEffectActionContext ctx, StatusEffectApplyBuffer buffer)
        {
            int damage = damagePerStack * ctx.Stacks;
            ctx.Owner.TakeDamage(damage);
        }
    }
}