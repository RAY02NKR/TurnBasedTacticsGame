using System.Collections.Generic;
using UnityEngine;
using TurnBasedGame.Battle.Grid;

namespace TurnBasedGame.Battle.Planning
{
    public sealed class TargetHighlightRenderer : MonoBehaviour
    {
        public static TargetHighlightRenderer Instance { get; private set; }

        [SerializeField] private float scale = 0.72f;
        [SerializeField] private float zOffset = -0.15f;
        [SerializeField] private Color highlightColor = new Color(1f, 0.9f, 0.2f, 0.55f);

        private readonly Dictionary<string, GameObject> _markers = new();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void BeginPlanning()
        {
        }

        public void EndPlanning()
        {
            ClearAll();
        }

        private void LateUpdate()
        {
            var targetController = TargetSelectionController.Instance;
            if (targetController == null || !targetController.IsSelectingTarget || BattleGridView.Instance == null)
            {
                ClearAll();
                return;
            }

            if (targetController.IsSelectingUnitTarget)
            {
                SyncUnitMarkers(targetController.ValidTargets);
                return;
            }

            if (targetController.IsSelectingPositionTarget || targetController.IsSelectingDirectionTarget)
            {
                SyncPositionMarkers(targetController.ValidPositions);
                return;
            }

            ClearAll();
        }

        private void SyncUnitMarkers(IReadOnlyList<TurnBasedGame.Battle.Units.UnitRuntime> targets)
        {
            var aliveKeys = new HashSet<string>();

            for (int i = 0; i < targets.Count; i++)
            {
                var unit = targets[i];
                if (unit == null) continue;
                if (!unit.IsAlive) continue;
                if (!unit.IsPlaced) continue;

                string key = $"{unit.Position.x}_{unit.Position.y}";
                aliveKeys.Add(key);

                if (!_markers.TryGetValue(key, out var marker) || marker == null)
                {
                    marker = CreateMarker(key);
                    _markers[key] = marker;
                }

                UpdateMarker(marker, unit.Position);
            }

            RemoveUnused(aliveKeys);
        }

        private void SyncPositionMarkers(IReadOnlyList<GridPosition> positions)
        {
            var aliveKeys = new HashSet<string>();

            for (int i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                string key = $"{pos.x}_{pos.y}";
                aliveKeys.Add(key);

                if (!_markers.TryGetValue(key, out var marker) || marker == null)
                {
                    marker = CreateMarker(key);
                    _markers[key] = marker;
                }

                UpdateMarker(marker, pos);
            }

            RemoveUnused(aliveKeys);
        }

        private GameObject CreateMarker(string key)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = $"TargetHighlight_{key}";
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * scale;

            var collider = go.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = highlightColor;
            renderer.sortingOrder = 40;

            return go;
        }

        private void UpdateMarker(GameObject marker, GridPosition pos)
        {
            var world = BattleGridView.Instance.GetCellCenter(pos);
            marker.transform.position = new Vector3(world.x, world.y, world.z + zOffset);
        }

        private void RemoveUnused(HashSet<string> aliveKeys)
        {
            var removeKeys = new List<string>();

            foreach (var pair in _markers)
            {
                if (!aliveKeys.Contains(pair.Key))
                {
                    if (pair.Value != null)
                        Destroy(pair.Value);

                    removeKeys.Add(pair.Key);
                }
            }

            for (int i = 0; i < removeKeys.Count; i++)
                _markers.Remove(removeKeys[i]);
        }

        private void ClearAll()
        {
            foreach (var pair in _markers)
            {
                if (pair.Value != null)
                    Destroy(pair.Value);
            }

            _markers.Clear();
        }
    }
}