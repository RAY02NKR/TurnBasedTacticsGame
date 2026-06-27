using UnityEngine;

namespace TurnBasedGame.Battle.StatusEffects.Actions
{
    [CreateAssetMenu(
        fileName = "TransformToStatusAtStacksThresholdStatusAction",
        menuName = "Battle/StatusEffects/Actions/TransformToStatusAtStacksThreshold")]
    public sealed class TransformToStatusAtStacksThresholdStatusActionDefinition : StatusEffectActionDefinition
    {
        [Min(1)] public int thresholdStacks = 5;
        public StatusEffectDefinition transformTo;
        [Min(1)] public int transformedStacks = 1;

        public override void Execute(StatusEffectActionContext ctx, StatusEffectApplyBuffer buffer)
        {
            if (ctx == null) return;
            if (ctx.Owner == null) return;
            if (ctx.Instance == null) return;
            if (ctx.Instance.Def == null) return;
            if (transformTo == null) return;
            if (ctx.Stacks < thresholdStacks) return;

            // 今の派生形を消す
            ctx.Owner.RemoveStatusByFamily(ctx.Instance.Def.family);

            // 新しい派生形を、このターンの減少処理後に付与
            buffer.Add(new PendingStatusApply(transformTo, transformedStacks));
        }
    }
}