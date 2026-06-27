using System.Collections.Generic;

namespace TurnBasedGame.Battle.StatusEffects.Actions
{
    public sealed class StatusEffectApplyBuffer
    {
        public readonly List<PendingStatusApply> Pending = new();

        public void Add(PendingStatusApply item)
        {
            Pending.Add(item);
        }
    }

    public readonly struct PendingStatusApply
    {
        public readonly StatusEffectDefinition Def;
        public readonly int StacksToAdd;

        public PendingStatusApply(StatusEffectDefinition def, int stacksToAdd)
        {
            Def = def;
            StacksToAdd = stacksToAdd;
        }
    }
}