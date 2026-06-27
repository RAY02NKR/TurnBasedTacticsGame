using System.Collections.Generic;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Services
{
    public static class TurnOrderService
    {
        public static List<UnitRuntime> BuildTurnOrder(BattleContext ctx)
        {
            var result = new List<UnitRuntime>();

            for (int i = 0; i < ctx.AllUnits.Count; i++)
            {
                var unit = ctx.AllUnits[i];
                if (unit == null) continue;
                if (!unit.IsAlive) continue;

                result.Add(unit);
            }

            result.Sort((a, b) =>
            {
                int speedCompare = b.Speed.CompareTo(a.Speed);
                if (speedCompare != 0)
                    return speedCompare;

                if (a.Team != b.Team)
                    return a.Team == Team.Ally ? -1 : 1;

                if (a.Team == Team.Ally)
                {
                    int orderCompare = ctx.GetPlannedOrder(a).CompareTo(ctx.GetPlannedOrder(b));
                    if (orderCompare != 0)
                        return orderCompare;
                }
                else
                {
                    int idCompare = a.RuntimeId.CompareTo(b.RuntimeId);
                    if (idCompare != 0)
                        return idCompare;
                }

                return a.RuntimeId.CompareTo(b.RuntimeId);
            });

            return result;
        }
    }
}