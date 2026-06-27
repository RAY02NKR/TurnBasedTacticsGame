using UnityEngine;

namespace TurnBasedGame.Battle.Logging
{
    public sealed class UnityDebugBattleLogger : IBattleLogger
    {
        public void Log(string message) => Debug.Log(message);
        public void Warn(string message) => Debug.LogWarning(message);
        public void Error(string message) => Debug.LogError(message);
    }
}