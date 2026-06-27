using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Planning
{
    public sealed class PlannedUnitAction
    {
        public GridPath MovePath { get; }
        public SkillUse SkillUse { get; }
        public UnitRuntime TargetUnit { get; }
        public GridPosition? TargetPosition { get; }
        public GridDirection? TargetDirection { get; }

        public bool HasMove => MovePath != null && MovePath.MoveCost > 0;
        public bool HasSkill => SkillUse.Skill != null;
        public bool HasTargetUnit => TargetUnit != null;
        public bool HasTargetPosition => TargetPosition.HasValue;
        public bool HasTargetDirection => TargetDirection.HasValue;

        public PlannedUnitAction(
            GridPath movePath,
            SkillUse skillUse,
            UnitRuntime targetUnit,
            GridPosition? targetPosition,
            GridDirection? targetDirection)
        {
            MovePath = movePath;
            SkillUse = skillUse;
            TargetUnit = targetUnit;
            TargetPosition = targetPosition;
            TargetDirection = targetDirection;
        }

        public GridPosition GetFinalPosition(GridPosition currentPosition)
        {
            if (MovePath == null)
                return currentPosition;

            return MovePath.End;
        }

        public static PlannedUnitAction Wait()
        {
            return new PlannedUnitAction(null, default, null, null, null);
        }

        public static PlannedUnitAction MoveOnly(GridPath movePath)
        {
            return new PlannedUnitAction(movePath, default, null, null, null);
        }

        public static PlannedUnitAction SkillOnly(
            SkillUse skillUse,
            UnitRuntime targetUnit = null,
            GridPosition? targetPosition = null,
            GridDirection? targetDirection = null)
        {
            return new PlannedUnitAction(null, skillUse, targetUnit, targetPosition, targetDirection);
        }

        public static PlannedUnitAction MoveAndSkill(
            GridPath movePath,
            SkillUse skillUse,
            UnitRuntime targetUnit = null,
            GridPosition? targetPosition = null,
            GridDirection? targetDirection = null)
        {
            return new PlannedUnitAction(movePath, skillUse, targetUnit, targetPosition, targetDirection);
        }

        public int GetPlannedStep(UnitRuntime unit)
        {
            if (unit == null)
                return 0;

            if (SkillUse.Skill == null)
                return unit.Step;

            if (!SkillUse.Skill.modifiesStepDuringPlanning)
                return unit.Step;

            return unit.Step + SkillUse.Skill.planningStepAdd;
        }
    }
}