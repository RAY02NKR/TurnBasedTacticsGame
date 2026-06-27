using System.Collections.Generic;
using UnityEngine;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Grid
{
    public sealed class BattleUnitMarkerRenderer : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private float markerScale = 0.45f;
        [SerializeField] private float markerZOffset = -0.2f;
        [SerializeField] private float labelYOffset = 0.28f;
        [SerializeField] private float labelZOffset = -0.1f;

        [Header("Acting Highlight")]
        [SerializeField] private bool highlightActingUnit = true;
        [SerializeField] private float actingMarkerScaleMultiplier = 1.25f;
        [SerializeField] private Color actingMarkerColor = new Color(1f, 0.9f, 0.2f, 1f);

        [Header("Health Bar")]
        [SerializeField] private bool showHealthBars = true;
        [SerializeField] private float healthBarWidth = 0.55f;
        [SerializeField] private float healthBarHeight = 0.08f;
        [SerializeField] private float healthBarYOffset = 0.48f;
        [SerializeField] private float healthBarZOffset = -0.05f;
        [SerializeField] private Color healthBarBackgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.9f);
        [SerializeField] private Color allyHealthBarColor = new Color(0.2f, 1f, 0.25f, 1f);
        [SerializeField] private Color enemyHealthBarColor = new Color(1f, 0.25f, 0.25f, 1f);
        [SerializeField] private Color turretHealthBarColor = new Color(0.2f, 1f, 1f, 1f);

        private const string MarkerName = "Marker";
        private const string LabelName = "Label";
        private const string HealthBarRootName = "HealthBar";
        private const string HealthBarBackgroundName = "Background";
        private const string HealthBarFillName = "Fill";

        private readonly Dictionary<int, GameObject> _markerRoots = new();

        private void Awake()
        {
            if (battleManager == null)
                battleManager = FindFirstObjectByType<BattleManager>();
        }

        private void LateUpdate()
        {
            if (battleManager == null || battleManager.Context == null || BattleGridView.Instance == null)
                return;

            SyncMarkers(battleManager.Context);
        }

        private void SyncMarkers(BattleContext ctx)
        {
            var aliveIds = new HashSet<int>();

            for (int i = 0; i < ctx.AllUnits.Count; i++)
            {
                var unit = ctx.AllUnits[i];
                if (unit == null) continue;
                if (!unit.IsAlive) continue;
                if (!unit.IsPlaced) continue;

                aliveIds.Add(unit.RuntimeId);

                if (!_markerRoots.TryGetValue(unit.RuntimeId, out var root) || root == null)
                {
                    root = CreateMarkerRoot(unit);
                    _markerRoots[unit.RuntimeId] = root;
                }

                UpdateMarkerRoot(root, unit);
            }

            var removeIds = new List<int>();

            foreach (var pair in _markerRoots)
            {
                if (!aliveIds.Contains(pair.Key))
                {
                    if (pair.Value != null)
                        Destroy(pair.Value);

                    removeIds.Add(pair.Key);
                }
            }

            for (int i = 0; i < removeIds.Count; i++)
                _markerRoots.Remove(removeIds[i]);
        }

        private GameObject CreateMarkerRoot(UnitRuntime unit)
        {
            var root = new GameObject($"UnitMarker_{unit.Name}_{unit.RuntimeId}");
            root.transform.SetParent(transform, false);

            CreateMarkerVisual(root.transform, unit);
            CreateLabel(root.transform, unit);

            if (showHealthBars)
                CreateHealthBar(root.transform, unit);

            return root;
        }

        private void CreateMarkerVisual(Transform parent, UnitRuntime unit)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.name = MarkerName;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = Vector3.one * markerScale;

            var collider = marker.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            var renderer = marker.GetComponent<MeshRenderer>();
            renderer.material = CreateMaterial(GetUnitColor(unit));
            renderer.sortingOrder = 20;
        }

        private void CreateLabel(Transform parent, UnitRuntime unit)
        {
            var label = new GameObject(LabelName);
            label.transform.SetParent(parent, false);
            label.transform.localPosition = new Vector3(0f, labelYOffset, labelZOffset);

            var text = label.AddComponent<TextMesh>();
            text.text = unit.Name;
            text.characterSize = 0.12f;
            text.fontSize = 48;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Color.white;

            var renderer = label.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sortingOrder = 40;
        }

        private void CreateHealthBar(Transform parent, UnitRuntime unit)
        {
            var barRoot = new GameObject(HealthBarRootName);
            barRoot.transform.SetParent(parent, false);
            barRoot.transform.localPosition = new Vector3(0f, healthBarYOffset, healthBarZOffset);

            var background = CreateQuad(
                HealthBarBackgroundName,
                barRoot.transform,
                healthBarBackgroundColor,
                30);

            background.transform.localScale = new Vector3(healthBarWidth, healthBarHeight, 1f);
            background.transform.localPosition = Vector3.zero;

            var fill = CreateQuad(
                HealthBarFillName,
                barRoot.transform,
                GetHealthBarColor(unit),
                31);

            fill.transform.localScale = new Vector3(healthBarWidth, healthBarHeight, 1f);
            fill.transform.localPosition = Vector3.zero;
        }

        private GameObject CreateQuad(string objectName, Transform parent, Color color, int sortingOrder)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = objectName;
            quad.transform.SetParent(parent, false);

            var collider = quad.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.material = CreateMaterial(color);
            renderer.sortingOrder = sortingOrder;

            return quad;
        }

        private Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Sprites/Default"));
            material.color = color;
            return material;
        }

        private void UpdateMarkerRoot(GameObject root, UnitRuntime unit)
        {
            var pos = BattleGridView.Instance.GetCellCenter(unit.Position);
            root.transform.position = new Vector3(pos.x, pos.y, pos.z + markerZOffset);

            UpdateMarkerVisual(root, unit);
            UpdateHealthBar(root, unit);
        }

        private void UpdateMarkerVisual(GameObject root, UnitRuntime unit)
        {
            var marker = root.transform.Find(MarkerName);
            if (marker == null)
                return;

            bool isActing = highlightActingUnit
                && battleManager != null
                && battleManager.Context != null
                && battleManager.Context.CurrentActingUnit == unit;

            float scale = markerScale * (isActing ? actingMarkerScaleMultiplier : 1f);
            marker.localScale = Vector3.one * scale;

            var renderer = marker.GetComponent<MeshRenderer>();
            if (renderer == null)
                return;

            renderer.material.color = isActing
                ? actingMarkerColor
                : GetUnitColor(unit);
        }

        private void UpdateHealthBar(GameObject root, UnitRuntime unit)
        {
            var barRoot = root.transform.Find(HealthBarRootName);

            if (barRoot == null)
            {
                if (!showHealthBars)
                    return;

                CreateHealthBar(root.transform, unit);
                barRoot = root.transform.Find(HealthBarRootName);

                if (barRoot == null)
                    return;
            }

            barRoot.gameObject.SetActive(showHealthBars);

            var fill = barRoot.Find(HealthBarFillName);
            if (fill == null)
                return;

            float hpRate = 0f;

            if (unit.MaxHP > 0)
                hpRate = Mathf.Clamp01((float)unit.CurrentHP / unit.MaxHP);

            float fillWidth = healthBarWidth * hpRate;

            fill.localScale = new Vector3(fillWidth, healthBarHeight, 1f);

            float fillX = -healthBarWidth * 0.5f + fillWidth * 0.5f;
            fill.localPosition = new Vector3(fillX, 0f, 0f);
        }

        private Color GetUnitColor(UnitRuntime unit)
        {
            if (unit.IsTurret)
                return unit.Team == Team.Ally
                    ? new Color(0.2f, 1f, 1f)
                    : new Color(1f, 0.4f, 0.4f);

            return unit.Team == Team.Ally
                ? new Color(0.2f, 0.8f, 1f)
                : new Color(1f, 0.2f, 0.2f);
        }

        private Color GetHealthBarColor(UnitRuntime unit)
        {
            if (unit.IsTurret)
                return turretHealthBarColor;

            return unit.Team == Team.Ally
                ? allyHealthBarColor
                : enemyHealthBarColor;
        }
    }
}