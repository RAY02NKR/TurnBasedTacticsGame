using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Grid
{
    public readonly struct UnitPlacementData
    {
        public UnitRuntime Unit { get; }
        public GridPosition Position { get; }

        public UnitPlacementData(UnitRuntime unit, GridPosition position)
        {
            Unit = unit;
            Position = position;
        }
    }
}