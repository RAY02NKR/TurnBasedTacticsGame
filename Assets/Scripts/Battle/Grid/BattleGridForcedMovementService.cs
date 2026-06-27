using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Grid
{
    public static class BattleGridForcedMovementService
    {
        public static ForcedMoveResult Knockback(
            BattleGrid grid,
            UnitRuntime source,
            UnitRuntime target,
            int maxDistance)
        {
            if (grid == null || source == null || target == null)
                return new ForcedMoveResult(target.Position, 0, false, false);

            var direction = GetKnockbackDirection(source.Position, target.Position);

            var current = target.Position;
            int movedSteps = 0;
            bool hitSolid = false;
            bool hitUnit = false;

            for (int i = 0; i < maxDistance; i++)
            {
                var next = new GridPosition(
                    current.x + direction.x,
                    current.y + direction.y);

                if (!grid.IsInside(next))
                {
                    hitSolid = true;
                    break;
                }

                if (grid.IsBlockedBySolidObject(next))
                {
                    hitSolid = true;
                    break;
                }

                if (grid.TryGetUnitAt(next, out var existing) && existing != null && existing != target)
                {
                    hitUnit = true;
                    break;
                }

                grid.MoveUnit(target, next);
                current = next;
                movedSteps++;

                if (!target.IsAlive)
                    break;
            }

            return new ForcedMoveResult(current, movedSteps, hitSolid, hitUnit);
        }

        private static GridPosition GetKnockbackDirection(GridPosition source, GridPosition target)
        {
            int dx = target.x - source.x;
            int dy = target.y - source.y;

            if (dx > 0) return new GridPosition(1, 0);
            if (dx < 0) return new GridPosition(-1, 0);
            if (dy > 0) return new GridPosition(0, 1);
            return new GridPosition(0, -1);
        }
    }
}