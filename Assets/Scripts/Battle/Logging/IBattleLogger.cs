namespace TurnBasedGame.Battle.Logging
{
    public interface IBattleLogger
    {
        void Log(string message);
        void Warn(string message);
        void Error(string message);
    }
}