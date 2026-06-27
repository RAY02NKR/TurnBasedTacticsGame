using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Planning
{
    public sealed class PlannedActionHoverPreview : MonoBehaviour
    {
        public static PlannedActionHoverPreview Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private TMP_Text skillText;
        [SerializeField] private CanvasGroup skillTextCanvasGroup;

        [Header("Path")]
        [SerializeField] private float lineWidth = 0.16f;
        [SerializeField] private float zOffset = -0.25f;
        [SerializeField] private Color hoverPathColor = new Color(1f, 0.75f, 0.1f, 1f);

        [Header("Info Panel Size")]
        [SerializeField] private RectTransform infoPanelRectTransform;
        [SerializeField] private float infoPanelWidth = 360f;
        [SerializeField] private float minInfoPanelHeight = 170f;
        [SerializeField] private float maxInfoPanelHeight = 420f;
        [SerializeField] private float infoPanelVerticalPadding = 24f;
        [SerializeField] private float infoPanelHorizontalPadding = 24f;

        private BattleContext _ctx;
        private LineRenderer _hoverLine;
        private UnitRuntime _hoveredUnit;
        private GameObject _infoPanelObject;

        private void Awake()
        {
            Instance = this;

            if (targetCamera == null)
                targetCamera = Camera.main;

            DisableInfoPanelRaycasts();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void BeginPlanning(BattleContext ctx)
        {
            _ctx = ctx;
            EnsureLine();

            if (skillTextCanvasGroup != null)
                _infoPanelObject = skillTextCanvasGroup.gameObject;
            else if (skillText != null)
                _infoPanelObject = skillText.gameObject;

            if (infoPanelRectTransform == null)
            {
                if (skillTextCanvasGroup != null)
                    infoPanelRectTransform = skillTextCanvasGroup.GetComponent<RectTransform>();
                else if (skillText != null)
                    infoPanelRectTransform = skillText.GetComponentInParent<RectTransform>();
            }

            DisableInfoPanelRaycasts();
            Hide();
        }

        public void EndPlanning()
        {
            _ctx = null;
            _hoveredUnit = null;
            Hide();
        }

        public void TickPlanning()
        {
            if (_ctx == null || BattleGridView.Instance == null)
            {
                Hide();
                return;
            }

            var targetSelection = TargetSelectionController.Instance;
            if (targetSelection != null && targetSelection.IsSelectingTarget)
            {
                Hide();
                return;
            }

            var selection = PlanningSelectionController.Instance;
            if (selection != null && selection.SelectedUnit != null)
            {
                Hide();
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                Hide();
                return;
            }

            if (targetCamera == null)
                targetCamera = Camera.main;

            if (!BattleGridView.Instance.TryScreenToGrid(targetCamera, Input.mousePosition, out var pos))
            {
                Hide();
                return;
            }

            if (!_ctx.Grid.TryGetUnitAt(pos, out var unit) || unit == null || !unit.IsAlive)
            {
                Hide();
                return;
            }

            _ctx.TryGetPlannedAction(unit, out var action);

            _hoveredUnit = unit;

            DrawPath(action);
            ShowInfoText(unit, action);
        }

        private void EnsureLine()
        {
            if (_hoverLine != null)
                return;

            var obj = new GameObject("HoverPlannedPath");
            obj.transform.SetParent(transform, false);

            _hoverLine = obj.AddComponent<LineRenderer>();
            _hoverLine.material = new Material(Shader.Find("Sprites/Default"));
            _hoverLine.useWorldSpace = true;
            _hoverLine.loop = false;
            _hoverLine.startWidth = lineWidth;
            _hoverLine.endWidth = lineWidth;
            _hoverLine.startColor = hoverPathColor;
            _hoverLine.endColor = hoverPathColor;
            _hoverLine.sortingOrder = 200;
            _hoverLine.positionCount = 0;
        }

        private void DrawPath(PlannedUnitAction action)
        {
            EnsureLine();

            if (_hoverLine == null)
                return;

            if (action == null || !action.HasMove || action.MovePath == null || action.MovePath.Positions.Count < 2)
            {
                _hoverLine.positionCount = 0;
                return;
            }

            _hoverLine.startColor = hoverPathColor;
            _hoverLine.endColor = hoverPathColor;
            _hoverLine.startWidth = lineWidth;
            _hoverLine.endWidth = lineWidth;
            _hoverLine.positionCount = action.MovePath.Positions.Count;

            for (int i = 0; i < action.MovePath.Positions.Count; i++)
            {
                var world = BattleGridView.Instance.GetCellCenter(action.MovePath.Positions[i]);
                world.z += zOffset;
                _hoverLine.SetPosition(i, world);
            }
        }

        private void ShowInfoText(UnitRuntime unit, PlannedUnitAction action)
        {
            if (skillText == null)
                return;

            if (_infoPanelObject != null && !_infoPanelObject.activeSelf)
                _infoPanelObject.SetActive(true);

            DisableInfoPanelRaycasts();

            skillText.textWrappingMode = TextWrappingModes.Normal;
            skillText.overflowMode = TextOverflowModes.Truncate;
            skillText.text = BuildInfoText(unit, action);

            ResizeInfoPanel();

            if (skillTextCanvasGroup != null)
            {
                skillTextCanvasGroup.alpha = 1f;
                skillTextCanvasGroup.interactable = false;
                skillTextCanvasGroup.blocksRaycasts = false;
            }
        }

        private void ResizeInfoPanel()
        {
            if (skillText == null)
                return;

            if (infoPanelRectTransform == null)
                return;

            float textWidth = Mathf.Max(10f, infoPanelWidth - infoPanelHorizontalPadding);

            var textRect = skillText.rectTransform;
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);

            float preferredHeight = skillText.GetPreferredValues(skillText.text, textWidth, 0f).y;

            float panelHeight = preferredHeight + infoPanelVerticalPadding;
            panelHeight = Mathf.Clamp(panelHeight, minInfoPanelHeight, maxInfoPanelHeight);

            infoPanelRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, infoPanelWidth);
            infoPanelRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);
        }

        private string BuildInfoText(UnitRuntime unit, PlannedUnitAction action)
        {
            var sb = new StringBuilder();

            sb.AppendLine(unit.Name);
            sb.AppendLine($"HP: {unit.CurrentHP} / {unit.MaxHP}");
            sb.AppendLine($"攻撃力: {unit.Attack}");
            sb.AppendLine($"速度: {unit.Speed}");
            sb.AppendLine($"ステップ: {unit.Step}");

            sb.AppendLine($"耐性 斬:{FormatResistance(unit.GetResistanceAdd(DamageType.Slash))}  貫:{FormatResistance(unit.GetResistanceAdd(DamageType.Pierce))}  打:{FormatResistance(unit.GetResistanceAdd(DamageType.Blunt))}");

            AppendStatuses(sb, unit);
            AppendPendingStatuses(sb, unit);
            AppendPlannedAction(sb, unit, action);

            return sb.ToString();
        }

        private void AppendStatuses(StringBuilder sb, UnitRuntime unit)
        {
            var statuses = unit.GetStatusDisplayInfos();

            if (statuses.Count == 0)
            {
                sb.AppendLine("状態異常: なし");
                return;
            }

            sb.Append("状態異常: ");

            for (int i = 0; i < statuses.Count; i++)
            {
                var s = statuses[i];

                if (i > 0)
                    sb.Append(", ");

                string turnText = s.RemainingTurns < 0
                    ? "永続"
                    : $"{s.RemainingTurns}T";

                sb.Append($"{s.Name}×{s.Stacks}({turnText})");
            }

            sb.AppendLine();
        }

        private void AppendPendingStatuses(StringBuilder sb, UnitRuntime unit)
        {
            var pendings = unit.GetPendingStatusDisplayInfos();

            if (pendings.Count == 0)
                return;

            sb.Append("予約付与: ");

            for (int i = 0; i < pendings.Count; i++)
            {
                var p = pendings[i];

                if (i > 0)
                    sb.Append(", ");

                sb.Append($"{p.Name}×{p.Stacks}({p.DelayTurns}T後)");
            }

            sb.AppendLine();
        }

        private void AppendPlannedAction(StringBuilder sb, UnitRuntime unit, PlannedUnitAction action)
        {
            sb.AppendLine();

            if (action == null || (!action.HasMove && !action.HasSkill))
            {
                sb.AppendLine("予定: なし");
                return;
            }

            sb.AppendLine("予定");

            string skillName = action.HasSkill && action.SkillUse.Skill != null
                ? action.SkillUse.Skill.skillName
                : "なし";

            string moveText = action.HasMove && action.MovePath != null
                ? $"{action.MovePath.Start} → {action.MovePath.End}"
                : "なし";

            string targetText = "なし";

            if (action.HasTargetUnit)
                targetText = action.TargetUnit.Name;
            else if (action.HasTargetPosition)
                targetText = action.TargetPosition.Value.ToString();
            else if (action.HasTargetDirection)
                targetText = action.TargetDirection.Value.ToString();

            sb.AppendLine($"スキル: {skillName}");
            sb.AppendLine($"移動: {moveText}");
            sb.AppendLine($"対象: {targetText}");
        }

        private string FormatResistance(float value)
        {
            if (value > 0f)
                return $"+{value:0.#}";

            return $"{value:0.#}";
        }

        private void Hide()
        {
            _hoveredUnit = null;

            if (_hoverLine != null)
                _hoverLine.positionCount = 0;

            if (skillText != null)
                skillText.text = "";

            if (skillTextCanvasGroup != null)
            {
                skillTextCanvasGroup.alpha = 0f;
                skillTextCanvasGroup.interactable = false;
                skillTextCanvasGroup.blocksRaycasts = false;
            }

            DisableInfoPanelRaycasts();

            if (_infoPanelObject != null && _infoPanelObject.activeSelf)
                _infoPanelObject.SetActive(false);
        }

        private void DisableInfoPanelRaycasts()
        {
            Transform root = null;

            if (skillTextCanvasGroup != null)
                root = skillTextCanvasGroup.transform;
            else if (skillText != null)
                root = skillText.transform;

            if (root == null)
                return;

            var graphics = root.GetComponentsInChildren<Graphic>(true);

            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].raycastTarget = false;
            }

            var canvasGroups = root.GetComponentsInChildren<CanvasGroup>(true);

            for (int i = 0; i < canvasGroups.Length; i++)
            {
                canvasGroups[i].interactable = false;
                canvasGroups[i].blocksRaycasts = false;
            }
        }
    }
}