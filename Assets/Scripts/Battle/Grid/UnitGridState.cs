using System;

namespace TurnBasedGame.Battle.Grid
{
    public sealed class UnitGridState
    {
        public bool IsPlaced { get; private set; }
        public GridPosition Position { get; private set; }

        public void PlaceAt(GridPosition position)
        {
            Position = position;
            IsPlaced = true;
        }

        public void MoveTo(GridPosition position)
        {
            if (!IsPlaced)
                throw new InvalidOperationException("このユニットはまだ盤面に配置されていません。");

            Position = position;
        }

        public void RemoveFromGrid()
        {
            IsPlaced = false;
            Position = default;
        }
    }
}