using UnityEngine;

namespace TurnBasedGame.Battle.StatusEffects.Actions
{
    [CreateAssetMenu(
        fileName = "ApplyStatusPerStackStatusAction",
        menuName = "Battle/StatusEffects/Actions/ApplyStatusPerStack")]
    public sealed class ApplyStatusPerStackStatusActionDefinition : StatusEffectActionDefinition
    {
        public StatusEffectDefinition statusToApply;
        public int baseStacksToAdd = 0;
        public int stacksPerSourceStack = 1;

        public override void Execute(StatusEffectActionContext ctx, StatusEffectApplyBuffer buffer)
        {
            if (statusToApply == null) return;

            int stacks = baseStacksToAdd + ctx.Stacks * stacksPerSourceStack;
            if (stacks < 1) stacks = 1;

            buffer.Add(new PendingStatusApply(statusToApply, stacks));
        }
    }
}