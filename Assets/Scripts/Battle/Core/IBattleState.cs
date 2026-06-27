// Assets/Scripts/Battle/Core/IBattleState.cs
namespace TurnBasedGame.Battle.Core
{
    public interface IBattleState
    {
        void Enter();
        void Tick();
        void Exit();
    }
}
