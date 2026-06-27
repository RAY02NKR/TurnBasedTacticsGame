using System;

namespace TurnBasedGame.Battle.StatusEffects
{
    [Serializable]
    public sealed class PendingStatusEffectInstance
    {
        public StatusEffectDefinition Def { get; }
        public int DelayTurns { get; set; }   // あと何ターン後に発動するか
        public int Stacks { get; set; }       // 付与予定スタック数

        public PendingStatusEffectInstance(StatusEffectDefinition def, int delayTurns, int stacks)
        {
            Def = def;
            DelayTurns = delayTurns;
            Stacks = stacks;
        }
    }
}