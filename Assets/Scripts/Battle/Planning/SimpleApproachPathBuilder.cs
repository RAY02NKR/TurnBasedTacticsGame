using System.Collections.Generic;
using TurnBasedGame.Battle;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Units;
using TurnBasedGame.Battle.Core;

namespace TurnBasedGame.Battle.Planning
{
    public static class SimpleApproachPathBuilder
    {
        public static bool TryBuildPath(BattleContext ctx, UnitRuntime unit, GridPosition target, out GridPath path, int? stepOverride = null)
        {
            path = null;

            if (ctx == null || unit == null)
                return false;

            if (!unit.IsPlaced)
                return false;

            int availableStep = stepOverride ?? unit.Step;
            if (availableStep <= 0)
                return false;

            var positions = new List<GridPosition> { unit.Position };
            var current = unit.Position;

            for (int i = 0; i < availableStep; i++)
            {
                if (current == target)
                    break;

                if (!TryGetNextStep(ctx, current, target, out var next))
                    break;

                positions.Add(next);
                current = next;
            }

            if (positions.Count <= 1)
                return false;

            path = new GridPath(positions);
            return true;
        }

        private static bool TryGetNextStep(BattleContext ctx, GridPosition current, GridPosition target, out GridPosition next)
        {
            next = current;

            if (current.x < target.x)
            {
                next = new GridPosition(current.x + 1, current.y);
                return true;
            }

            if (current.x > target.x)
            {
                next = new GridPosition(current.x - 1, current.y);
                return true;
            }

            if (current.y < target.y)
            {
                next = new GridPosition(current.x, current.y + 1);
                return true;
            }

            if (current.y > target.y)
            {
                next = new GridPosition(current.x, current.y - 1);
                return true;
            }

            return false;
        }
    }
}