using System;
using System.Collections.Generic;

namespace TurnBasedGame.Battle.Grid
{
    public static class BattleGridPlacementService
    {
        public static void PlaceInitialUnits(BattleGrid grid, IEnumerable<UnitPlacementData> placements)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (placements == null)
                throw new ArgumentNullException(nameof(placements));

            foreach (var placement in placements)
            {
                if (placement.Unit == null)
                    throw new InvalidOperationException("配置対象のユニットが null です。");

                grid.PlaceUnit(placement.Unit, placement.Position);
            }
        }
    }
}