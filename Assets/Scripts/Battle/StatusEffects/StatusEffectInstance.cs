namespace TurnBasedGame.Battle.StatusEffects
{
    public sealed class StatusEffectInstance
    {
        public StatusEffectDefinition Def { get; }
        public int RemainingTurns { get; set; } // -1なら永続
        public int Stacks { get; set; }

        public StatusEffectInstance(StatusEffectDefinition def, int remainingTurns, int stacks)
        {
            Def = def;
            RemainingTurns = remainingTurns;
            Stacks = stacks;
        }
    }
}