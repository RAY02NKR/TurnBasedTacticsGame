using System.Collections.Generic;
using UnityEngine;

namespace TurnBasedGame.Battle.Grid
{
    public sealed class BattleGridView : MonoBehaviour
    {
        public static BattleGridView Instance { get; private set; }

        [SerializeField] private Vector3 origin = Vector3.zero;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private float planeZ = 0f;
        [SerializeField] private int previewWidth = 6;
        [SerializeField] private int previewHeight = 6;

        [Header("Runtime Grid Lines")]
        [SerializeField] private bool showRuntimeGrid = true;
        [SerializeField] private float lineWidth = 0.04f;
        [SerializeField] private Color lineColor = Color.white;

        private BattleGrid _grid;
        private readonly List<LineRenderer> _lines = new();

        public float CellSize => cellSize;
        public Vector3 Origin => origin;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void BindGrid(BattleGrid grid)
        {
            _grid = grid;

            if (showRuntimeGrid)
                RebuildRuntimeGridLines();
        }

        public Vector3 GetCellCenter(GridPosition pos)
        {
            return origin + new Vector3(
                (pos.x + 0.5f) * cellSize,
                (pos.y + 0.5f) * cellSize,
                planeZ);
        }

        public bool TryScreenToGrid(Camera cam, Vector2 screenPosition, out GridPosition gridPosition)
        {
            gridPosition = default;

            if (cam == null)
                return false;

            var ray = cam.ScreenPointToRay(screenPosition);
            var plane = new Plane(Vector3.forward, new Vector3(0f, 0f, planeZ));

            if (!plane.Raycast(ray, out var enter))
                return false;

            var world = ray.GetPoint(enter);

            int x = Mathf.FloorToInt((world.x - origin.x) / cellSize);
            int y = Mathf.FloorToInt((world.y - origin.y) / cellSize);

            gridPosition = new GridPosition(x, y);

            if (_grid != null)
                return _grid.IsInside(gridPosition);

            return x >= 0 && x < previewWidth && y >= 0 && y < previewHeight;
        }

        private void RebuildRuntimeGridLines()
        {
            ClearRuntimeGridLines();

            int width = _grid != null ? _grid.Width : previewWidth;
            int height = _grid != null ? _grid.Height : previewHeight;

            for (int x = 0; x <= width; x++)
            {
                var from = origin + new Vector3(x * cellSize, 0f, planeZ);
                var to = origin + new Vector3(x * cellSize, height * cellSize, planeZ);
                _lines.Add(CreateLine($"GridLine_V_{x}", from, to));
            }

            for (int y = 0; y <= height; y++)
            {
                var from = origin + new Vector3(0f, y * cellSize, planeZ);
                var to = origin + new Vector3(width * cellSize, y * cellSize, planeZ);
                _lines.Add(CreateLine($"GridLine_H_{y}", from, to));
            }
        }

        private LineRenderer CreateLine(string lineName, Vector3 from, Vector3 to)
        {
            var go = new GameObject(lineName);
            go.transform.SetParent(transform, false);

            var line = go.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.useWorldSpace = true;
            line.loop = false;
            line.positionCount = 2;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.startColor = lineColor;
            line.endColor = lineColor;
            line.sortingOrder = 10;
            line.SetPosition(0, from);
            line.SetPosition(1, to);

            return line;
        }

        private void ClearRuntimeGridLines()
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                if (_lines[i] != null)
                    Destroy(_lines[i].gameObject);
            }

            _lines.Clear();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = lineColor;

            int width = _grid != null ? _grid.Width : previewWidth;
            int height = _grid != null ? _grid.Height : previewHeight;

            for (int x = 0; x <= width; x++)
            {
                var from = origin + new Vector3(x * cellSize, 0f, planeZ);
                var to = origin + new Vector3(x * cellSize, height * cellSize, planeZ);
                Gizmos.DrawLine(from, to);
            }

            for (int y = 0; y <= height; y++)
            {
                var from = origin + new Vector3(0f, y * cellSize, planeZ);
                var to = origin + new Vector3(width * cellSize, y * cellSize, planeZ);
                Gizmos.DrawLine(from, to);
            }
        }
    }
}