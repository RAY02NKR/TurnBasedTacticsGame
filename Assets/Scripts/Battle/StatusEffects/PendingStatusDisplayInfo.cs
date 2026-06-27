namespace TurnBasedGame.Battle.StatusEffects
{
    public readonly struct PendingStatusDisplayInfo
    {
        public string Name { get; }
        public int Stacks { get; }
        public int DelayTurns { get; }

        public PendingStatusDisplayInfo(string name, int stacks, int delayTurns)
        {
            Name = name;
            Stacks = stacks;
            DelayTurns = delayTurns;
        }
    }
}