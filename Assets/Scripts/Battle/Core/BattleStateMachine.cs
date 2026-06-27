// Assets/Scripts/Battle/Core/BattleStateMachine.cs
namespace TurnBasedGame.Battle.Core
{
    public sealed class BattleStateMachine
    {
        public IBattleState Current { get; private set; }

        public void ChangeState(IBattleState next)
        {
            Current?.Exit();
            Current = next;
            Current?.Enter();
        }

        public void Tick() => Current?.Tick();
    }
}
