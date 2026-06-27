using TurnBasedGame.Battle.Logging;

namespace TurnBasedGame.Battle.Grid.Debug
{
    public static class BattleGridDebugLogger
    {
        public static void Log(IBattleLogger logger, BattleGrid grid, string title = "Battle Grid")
        {
            if (logger == null)
                return;

            logger.Log($"{title}\n{BattleGridDebugFormatter.Format(grid)}");
        }
    }
}