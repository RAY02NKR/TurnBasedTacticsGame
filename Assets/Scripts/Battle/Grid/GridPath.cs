using System;
using System.Collections.Generic;

namespace TurnBasedGame.Battle.Grid
{
    public sealed class GridPath
    {
        private readonly List<GridPosition> _positions;

        public IReadOnlyList<GridPosition> Positions => _positions;
        public GridPosition Start => _positions[0];
        public GridPosition End => _positions[_positions.Count - 1];
        public int MoveCost => _positions.Count - 1;

        public GridPath(IEnumerable<GridPosition> positions)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));

            _positions = new List<GridPosition>(positions);

            if (_positions.Count == 0)
                throw new InvalidOperationException("GridPath は1マス以上必要です。");
        }
    }
}