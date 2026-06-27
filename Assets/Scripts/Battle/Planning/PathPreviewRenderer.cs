using System.Collections.Generic;
using UnityEngine;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Planning
{
    public sealed class PathPreviewRenderer : MonoBehaviour
    {
        public static PathPreviewRenderer Instance { get; private set; }

        [SerializeField] private float lineWidth = 0.12f;
        [SerializeField] private Color previewColor = Color.yellow;
        [SerializeField] private Color confirmedColor = Color.cyan;
        [SerializeField] private float zOffset = -0.1f;

        private BattleContext _ctx;
        private readonly Dictionary<int, LineRenderer> _confirmedLines = new();
        private LineRenderer _previewLine;
        private readonly List<GridPosition> _previewPositions = new();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void BeginPlanning(BattleContext ctx)
        {
            _ctx = ctx;
            EnsurePreviewLine();
        }

        public void EndPlanning()
        {
            _ctx = null;
            _previewPositions.Clear();

            if (_previewLine != null)
                _previewLine.positionCount = 0;

            foreach (var pair in _confirmedLines)
            {
                if (pair.Value != null)
                    pair.Value.positionCount = 0;
            }
        }

        public void SetPreviewPositions(IReadOnlyList<GridPosition> positions)
        {
            _previewPositions.Clear();

            if (positions != null)
            {
                for (int i = 0; i < positions.Count; i++)
                    _previewPositions.Add(positions[i]);
            }

            RedrawPreview();
        }

        public void ClearPreview()
        {
            _previewPositions.Clear();

            if (_previewLine != null)
                _previewLine.positionCount = 0;
        }

        private void LateUpdate()
        {
            if (_ctx == null)
                return;

            RedrawConfirmedPaths();
            RedrawPreview();
        }

        private void RedrawConfirmedPaths()
        {
            var usedIds = new HashSet<int>();

            for (int i = 0; i < _ctx.Allies.Count; i++)
            {
                var unit = _ctx.Allies[i];
                if (unit == null) continue;
                if (!unit.IsAlive) continue;

                usedIds.Add(unit.RuntimeId);

                if (!_confirmedLines.TryGetValue(unit.RuntimeId, out var line))
                {
                    line = CreateLine($"ConfirmedPath_{unit.RuntimeId}", confirmedColor);
                    _confirmedLines.Add(unit.RuntimeId, line);
                }

                var action = _ctx.GetPlannedActionOrWait(unit);
                if (action.HasMove && action.MovePath != null)
                    ApplyPositions(line, action.MovePath.Positions, confirmedColor);
                else
                    line.positionCount = 0;
            }

            var removeKeys = new List<int>();

            foreach (var pair in _confirmedLines)
            {
                if (!usedIds.Contains(pair.Key))
                {
                    if (pair.Value != null)
                        Destroy(pair.Value.gameObject);

                    removeKeys.Add(pair.Key);
                }
            }

            for (int i = 0; i < removeKeys.Count; i++)
                _confirmedLines.Remove(removeKeys[i]);
        }

        private void RedrawPreview()
        {
            EnsurePreviewLine();

            if (_previewPositions.Count >= 2)
                ApplyPositions(_previewLine, _previewPositions, previewColor);
            else
                _previewLine.positionCount = 0;
        }

        private void EnsurePreviewLine()
        {
            if (_previewLine == null)
                _previewLine = CreateLine("PreviewPath", previewColor);
        }

        private LineRenderer CreateLine(string lineName, Color color)
        {
            var go = new GameObject(lineName);
            go.transform.SetParent(transform, false);

            var line = go.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.useWorldSpace = true;
            line.loop = false;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.positionCount = 0;
            line.startColor = color;
            line.endColor = color;
            line.sortingOrder = 100;

            return line;
        }

        private void ApplyPositions(LineRenderer line, IReadOnlyList<GridPosition> positions, Color color)
        {
            if (line == null || BattleGridView.Instance == null)
                return;

            line.startColor = color;
            line.endColor = color;
            line.positionCount = positions.Count;

            for (int i = 0; i < positions.Count; i++)
            {
                var world = BattleGridView.Instance.GetCellCenter(positions[i]);
                world.z += zOffset;
                line.SetPosition(i, world);
            }
        }
    }
}