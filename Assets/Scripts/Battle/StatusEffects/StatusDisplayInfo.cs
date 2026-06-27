namespace TurnBasedGame.Battle.StatusEffects
{
    public readonly struct StatusDisplayInfo
    {
        public string Name { get; }
        public int Stacks { get; }
        public int RemainingTurns { get; }

        public StatusDisplayInfo(string name, int stacks, int remainingTurns)
        {
            Name = name;
            Stacks = stacks;
            RemainingTurns = remainingTurns;
        }
    }
}