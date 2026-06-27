using UnityEngine;
using TurnBasedGame.Battle.StatusEffects;

namespace TurnBasedGame.Battle.Skills.Effects
{
    [CreateAssetMenu(
        fileName = "ConsumeOwnStatusEffect",
        menuName = "Battle/Skills/Effects/ConsumeOwnStatus")]
    public sealed class ConsumeOwnStatusEffectDefinition : SkillEffectDefinition
    {
        public StatusEffectDefinition status;
        [Min(1)] public int amount = 1;
        public bool consumeAll = true;
        public bool requireFullAmount = false;

        public override void Apply(SkillEffectContext ctx)
        {
            if (ctx == null) return;
            if (ctx.Actor == null) return;
            if (status == null)
            {
                ctx.ConsumedCharge = 0;
                return;
            }

            int amountToConsume = consumeAll
                ? ctx.Actor.GetStatusStacks(status)
                : amount;

            if (amountToConsume <= 0)
            {
                ctx.ConsumedCharge = 0;
                return;
            }

            int consumed = ctx.Actor.ConsumeStatusStacks(status, amountToConsume);

            if (requireFullAmount && consumed < amountToConsume)
            {
                ctx.ConsumedCharge = 0;
                return;
            }

            ctx.ConsumedCharge = consumed;
        }
    }
}