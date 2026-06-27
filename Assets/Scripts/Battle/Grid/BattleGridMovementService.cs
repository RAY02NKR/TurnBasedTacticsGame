using System;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Grid
{
    public static class BattleGridMovementService
    {
        public static bool CanMoveAlongPath(BattleGrid grid, UnitRuntime unit, GridPath path, out string reason, int? stepOverride = null)
        {
            reason = null;
            int availableStep = stepOverride ?? unit.Step;

            if (grid == null)
            {
                reason = "grid が null です。";
                return false;
            }

            if (unit == null)
            {
                reason = "unit が null です。";
                return false;
            }

            if (path == null)
            {
                reason = "path が null です。";
                return false;
            }

            if (!unit.IsPlaced)
            {
                reason = "未配置ユニットは移動できません。";
                return false;
            }

            if (path.Start != unit.Position)
            {
                reason = $"経路の開始地点が現在位置と一致していません。 start:{path.Start} current:{unit.Position}";
                return false;
            }

            if (path.MoveCost > availableStep)
            {
                reason = $"移動コスト超過です。 cost:{path.MoveCost} step:{availableStep}";
                return false;
            }

            for (int i = 0; i < path.Positions.Count; i++)
            {
                var pos = path.Positions[i];

                if (!grid.IsInside(pos))
                {
                    reason = $"盤面外のマスが含まれています: {pos}";
                    return false;
                }

                if (i == 0)
                    continue;

                var prev = path.Positions[i - 1];
                int dist = grid.GetManhattanDistance(prev, pos);

                if (dist != 1)
                {
                    reason = $"隣接していない移動があります。 from:{prev} to:{pos}";
                    return false;
                }
            }

            return true;
        }

        public static GridPosition ResolveReachableDestination(BattleGrid grid, UnitRuntime unit, GridPath path, int? stepOverride = null)
        {
            if (!CanMoveAlongPath(grid, unit, path, out var reason, stepOverride))
                throw new InvalidOperationException(reason);

            var lastReachable = unit.Position;

            for (int i = 1; i < path.Positions.Count; i++)
            {
                var next = path.Positions[i];

                bool occupiedByOther = grid.TryGetUnitAt(next, out var existing) && existing != unit;

                if (!occupiedByOther)
                    lastReachable = next;
            }

            return lastReachable;
        }

        public static GridPosition MoveAlongPath(BattleGrid grid, UnitRuntime unit, GridPath path, int? stepOverride = null)
        {
            if (!CanMoveAlongPath(grid, unit, path, out var reason, stepOverride))
                throw new InvalidOperationException(reason);

            var positions = path.Positions;
            var lastReachable = unit.Position;

            for (int i = 1; i < positions.Count; i++)
            {
                var next = positions[i];

                if (IsBlockedBySolidObject(grid, next))
                    break;

                bool occupiedByOther = grid.TryGetUnitAt(next, out var existing) && existing != unit;

                if (occupiedByOther)
                    continue;

                grid.MoveUnit(unit, next);
                lastReachable = next;

                if (!unit.IsAlive)
                    break;
            }

            return lastReachable;
        }

        private static bool IsBlockedBySolidObject(BattleGrid grid, GridPosition pos)
        {
            return false;
        }
    }
}