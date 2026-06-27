using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Services
{
    public static class UnitQueryService
    {
        public static UnitRuntime FindFirstAlive(BattleContext ctx, Team team)
        {
            var list = team == Team.Ally ? ctx.Allies : ctx.Enemies;
            for (int i = 0; i < list.Count; i++)
                if (list[i].IsAlive) return list[i];
            return null;
        }
    }
}