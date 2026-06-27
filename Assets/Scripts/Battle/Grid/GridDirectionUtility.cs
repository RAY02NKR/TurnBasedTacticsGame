using System;

namespace TurnBasedGame.Battle.Grid
{
    public static class GridDirectionUtility
    {
        public static GridPosition ToOffset(GridDirection direction)
        {
            return direction switch
            {
                GridDirection.Up => new GridPosition(0, 1),
                GridDirection.Down => new GridPosition(0, -1),
                GridDirection.Left => new GridPosition(-1, 0),
                GridDirection.Right => new GridPosition(1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
        }
    }
}