using System.Collections.Generic;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Services
{
    public static class DirectionalSkillTargetingService
    {
        public static List<UnitRuntime> GetUnitsOnSelectedLine(
            BattleContext battle,
            UnitRuntime actor,
            int range,
            GridDirection? direction)
        {
            var result = new List<UnitRuntime>();

            if (battle == null || actor == null)
                return result;

            if (!actor.IsAlive || !actor.IsPlaced)
                return result;

            if (!direction.HasValue)
                return result;

            var linePositions = BattleGridRangeService.GetStraightLinePositions(
                battle.Grid,
                actor.Position,
                direction.Value,
                range);

            for (int i = 0; i < linePositions.Count; i++)
            {
                var pos = linePositions[i];

                if (!battle.Grid.TryGetUnitAt(pos, out var unit))
                    continue;

                if (unit == null || !unit.IsAlive)
                    continue;

                if (unit.Team == actor.Team)
                    continue;

                result.Add(unit);
            }

            return result;
        }

        public static List<UnitRuntime> GetUnitsOnSelectedLineFromPosition(
            BattleContext battle,
            UnitRuntime actor,
            GridPosition origin,
            int range,
            GridDirection? direction)
        {
            var result = new List<UnitRuntime>();

            if (battle == null || actor == null)
                return result;

            if (!direction.HasValue)
                return result;

            var linePositions = BattleGridRangeService.GetStraightLinePositions(
                battle.Grid,
                origin,
                direction.Value,
                range);

            for (int i = 0; i < linePositions.Count; i++)
            {
                var pos = linePositions[i];

                if (!battle.Grid.TryGetUnitAt(pos, out var unit))
                    continue;

                if (unit == null || !unit.IsAlive)
                    continue;

                if (unit.Team == actor.Team)
                    continue;

                result.Add(unit);
            }

            return result;
        }
    }
}