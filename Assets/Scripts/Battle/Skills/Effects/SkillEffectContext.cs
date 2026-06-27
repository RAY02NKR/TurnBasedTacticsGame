using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Services;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Skills.Effects
{
    public sealed class SkillEffectContext
    {
        public BattleContext Battle { get; }
        public UnitRuntime Actor { get; }
        public UnitRuntime Target { get; }
        public SkillDefinition Skill { get; }
        public GridPosition? TargetPosition { get; }
        public GridDirection? TargetDirection { get; }
        public int ConsumedCharge { get; set; }
        public DamageResult LastDamageResult;

        public SkillEffectContext(
            BattleContext battle,
            UnitRuntime actor,
            UnitRuntime target,
            SkillDefinition skill,
            GridPosition? targetPosition,
            GridDirection? targetDirection)
        {
            Battle = battle;
            Actor = actor;
            Target = target;
            Skill = skill;
            TargetPosition = targetPosition;
            TargetDirection = targetDirection;
            LastDamageResult = default;
        }
    }
}