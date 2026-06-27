using UnityEngine;

namespace TurnBasedGame.Battle.StatusEffects.Actions
{
    [CreateAssetMenu(
        fileName = "ReserveStatusPerStackToNextTurnStatusAction",
        menuName = "Battle/StatusEffects/Actions/ReserveStatusPerStackToNextTurn")]
    public sealed class ReserveStatusPerStackToNextTurnStatusActionDefinition : StatusEffectActionDefinition
    {
        public StatusEffectDefinition statusToReserve;
        [Min(0)] public int baseStacksToAdd = 0;
        [Min(0)] public int stacksPerSourceStack = 1;
        [Min(1)] public int delayTurns = 1;

        public override void Execute(StatusEffectActionContext ctx, StatusEffectApplyBuffer buffer)
        {
            if (ctx == null) return;
            if (ctx.Owner == null) return;
            if (statusToReserve == null) return;

            int stacks = baseStacksToAdd + ctx.Stacks * stacksPerSourceStack;
            if (stacks <= 0) return;

            ctx.Owner.ReserveStatus(statusToReserve, delayTurns, stacks);
        }
    }
}