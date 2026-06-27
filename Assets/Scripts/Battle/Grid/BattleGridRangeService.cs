using System.Collections.Generic;

namespace TurnBasedGame.Battle.Grid
{
    public static class BattleGridRangeService
    {
        public static bool IsInRange(BattleGrid grid, GridPosition from, GridPosition to, int range)
        {
            if (grid == null) return false;
            if (range < 0) return false;
            if (!grid.IsInside(from) || !grid.IsInside(to)) return false;

            return grid.GetManhattanDistance(from, to) <= range;
        }

        public static List<GridPosition> GetPositionsInRange(BattleGrid grid, GridPosition origin, int range, bool includeOrigin = false)
        {
            var result = new List<GridPosition>();
            if (grid == null) return result;
            if (range < 0) return result;
            if (!grid.IsInside(origin)) return result;

            foreach (var pos in grid.GetAllPositions())
            {
                int dist = grid.GetManhattanDistance(origin, pos);

                if (dist > range)
                    continue;

                if (!includeOrigin && pos == origin)
                    continue;

                result.Add(pos);
            }

            return result;
        }

        public static List<GridPosition> GetPositionsInArea(BattleGrid grid, GridPosition center, int radius, bool includeCenter = true)
        {
            var result = new List<GridPosition>();
            if (grid == null) return result;
            if (radius < 0) return result;
            if (!grid.IsInside(center)) return result;

            foreach (var pos in grid.GetAllPositions())
            {
                int dist = grid.GetManhattanDistance(center, pos);

                if (dist > radius)
                    continue;

                if (!includeCenter && pos == center)
                    continue;

                result.Add(pos);
            }

            return result;
        }

        public static List<GridPosition> GetStraightLinePositions(BattleGrid grid, GridPosition origin, GridDirection direction, int length)
        {
            var result = new List<GridPosition>();
            if (grid == null) return result;
            if (length <= 0) return result;
            if (!grid.IsInside(origin)) return result;

            var offset = GridDirectionUtility.ToOffset(direction);
            var current = origin;

            for (int i = 0; i < length; i++)
            {
                current = new GridPosition(current.x + offset.x, current.y + offset.y);

                if (!grid.IsInside(current))
                    break;

                result.Add(current);
            }

            return result;
        }

        public static bool IsStraightLine(GridPosition from, GridPosition to)
        {
            return from.x == to.x || from.y == to.y;
        }

        public static bool TryGetStraightDirection(GridPosition from, GridPosition to, out GridDirection direction)
        {
            direction = default;

            if (from.x == to.x)
            {
                if (to.y > from.y)
                {
                    direction = GridDirection.Up;
                    return true;
                }

                if (to.y < from.y)
                {
                    direction = GridDirection.Down;
                    return true;
                }
            }

            if (from.y == to.y)
            {
                if (to.x > from.x)
                {
                    direction = GridDirection.Right;
                    return true;
                }

                if (to.x < from.x)
                {
                    direction = GridDirection.Left;
                    return true;
                }
            }

            return false;
        }

        public static List<GridPosition> GetNeighbors(BattleGrid grid, GridPosition center)
        {
            var result = new List<GridPosition>();
            if (grid == null) return result;
            if (!grid.IsInside(center)) return result;

            var candidates = new[]
            {
                new GridPosition(center.x, center.y + 1),
                new GridPosition(center.x, center.y - 1),
                new GridPosition(center.x - 1, center.y),
                new GridPosition(center.x + 1, center.y),
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                if (grid.IsInside(candidates[i]))
                    result.Add(candidates[i]);
            }

            return result;
        }
    }
}