using UnityEngine;

namespace TurnBasedGame.Battle.StatusEffects.Actions
{
    [CreateAssetMenu(
        fileName = "ReflectReceivedDamageStatusAction",
        menuName = "Battle/StatusEffects/Actions/ReflectReceivedDamage")]
    public sealed class ReflectReceivedDamageStatusActionDefinition : StatusEffectActionDefinition
    {
        [Min(0)] public int multiplier = 2;
        [Min(0)] public int fixedBonus = 0;

        public override void Execute(StatusEffectActionContext ctx, StatusEffectApplyBuffer buffer)
        {
            if (ctx == null) return;
            if (ctx.Owner == null) return;
            if (ctx.SourceUnit == null) return;
            if (ctx.SourceUnit == ctx.Owner) return;
            if (!ctx.SourceUnit.IsAlive) return;
            if (ctx.ReceivedDamage <= 0) return;

            int damage = ctx.ReceivedDamage * multiplier + fixedBonus;
            if (damage <= 0) return;

            ctx.SourceUnit.TakeDamage(damage);
        }
    }
}