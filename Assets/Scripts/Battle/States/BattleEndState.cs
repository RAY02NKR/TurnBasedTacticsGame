// Assets/Scripts/Battle/States/BattleEndState.cs
using UnityEngine;
using TurnBasedGame.Battle.Core;

namespace TurnBasedGame.Battle.States
{
    public sealed class BattleEndState : IBattleState
    {
        private readonly BattleContext _ctx;

        public BattleEndState(BattleContext ctx) => _ctx = ctx;

        public void Enter()
        {
            Debug.Log("=== Battle End State ===");
        }

        public void Tick() { }
        public void Exit() { }
    }
}
