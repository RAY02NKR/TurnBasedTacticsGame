namespace TurnBasedGame.Battle.Logging
{
    // Logger未設定でも落ちないためのダミー実装
    public sealed class NullBattleLogger : IBattleLogger
    {
        public void Log(string message) { }
        public void Warn(string message) { }
        public void Error(string message) { }
    }
}