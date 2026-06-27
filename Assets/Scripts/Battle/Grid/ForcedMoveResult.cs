namespace TurnBasedGame.Battle.Grid
{
    public readonly struct ForcedMoveResult
    {
        public GridPosition FinalPosition { get; }
        public int MovedSteps { get; }
        public bool HitSolidObject { get; }
        public bool HitUnit { get; }

        public ForcedMoveResult(
            GridPosition finalPosition,
            int movedSteps,
            bool hitSolidObject,
            bool hitUnit)
        {
            FinalPosition = finalPosition;
            MovedSteps = movedSteps;
            HitSolidObject = hitSolidObject;
            HitUnit = hitUnit;
        }
    }
}