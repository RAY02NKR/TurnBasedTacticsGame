using System;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Grid
{
    public static class BattleGridSummonService
    {
        public static bool CanSummonUnit(
            BattleGrid grid,
            UnitRuntime summoner,
            UnitRuntime summonUnit,
            GridPosition position,
            int summonRange,
            out string reason)
        {
            reason = null;

            if (grid == null)
            {
                reason = "grid が null です。";
                return false;
            }

            if (summoner == null)
            {
                reason = "summoner が null です。";
                return false;
            }

            if (summonUnit == null)
            {
                reason = "summonUnit が null です。";
                return false;
            }

            if (!summoner.IsAlive)
            {
                reason = "召喚者が戦闘不能です。";
                return false;
            }

            if (!summoner.IsPlaced)
            {
                reason = "召喚者が盤面に配置されていません。";
                return false;
            }

            if (summonUnit.IsPlaced)
            {
                reason = "召喚対象ユニットはすでに配置済みです。";
                return false;
            }

            if (!grid.IsInside(position))
            {
                reason = $"召喚先が盤面外です: {position}";
                return false;
            }

            if (grid.IsOccupied(position))
            {
                reason = $"召喚先マスは使用中です: {position}";
                return false;
            }

            if (!BattleGridRangeService.IsInRange(grid, summoner.Position, position, summonRange))
            {
                reason = $"召喚射程外です。 summoner:{summoner.Position} target:{position} range:{summonRange}";
                return false;
            }

            return true;
        }

        public static bool TrySummonUnit(
            BattleGrid grid,
            UnitRuntime summoner,
            UnitRuntime summonUnit,
            GridPosition position,
            int summonRange,
            out string reason)
        {
            if (!CanSummonUnit(grid, summoner, summonUnit, position, summonRange, out reason))
                return false;

            grid.PlaceUnit(summonUnit, position);
            return true;
        }

        public static void SummonUnit(
            BattleGrid grid,
            UnitRuntime summoner,
            UnitRuntime summonUnit,
            GridPosition position,
            int summonRange)
        {
            if (!TrySummonUnit(grid, summoner, summonUnit, position, summonRange, out var reason))
                throw new InvalidOperationException(reason);
        }

        public static int CountPlacedTurrets(BattleGrid grid, Team team)
        {
            if (grid == null) return 0;

            int count = 0;

            foreach (var unit in grid.GetAllUnits())
            {
                if (unit == null) continue;
                if (!unit.IsAlive) continue;
                if (!unit.IsTurret) continue;
                if (unit.Team != team) continue;

                count++;
            }

            return count;
        }
    }
}