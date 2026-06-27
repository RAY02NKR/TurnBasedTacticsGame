using UnityEngine;

namespace TurnBasedGame.Battle.StatusEffects.Actions
{
    [CreateAssetMenu(
        fileName = "CarryHalfStacksToNextTurnStatusAction",
        menuName = "Battle/StatusEffects/Actions/CarryHalfStacksToNextTurn")]
    public sealed class CarryHalfStacksToNextTurnStatusActionDefinition : StatusEffectActionDefinition
    {
        public override void Execute(StatusEffectActionContext ctx, StatusEffectApplyBuffer buffer)
        {
            if (ctx == null) return;
            if (ctx.Owner == null) return;
            if (ctx.Instance == null) return;
            if (ctx.Instance.Def == null) return;

            int nextStacks = ctx.Stacks / 2; // 切り捨て
            if (nextStacks <= 0) return;

            ctx.Owner.ReserveStatus(ctx.Instance.Def, 1, nextStacks);
        }
    }
}