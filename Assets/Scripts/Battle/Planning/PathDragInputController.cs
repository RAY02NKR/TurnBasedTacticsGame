using System.Collections.Generic;
using UnityEngine;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Units;
using TurnBasedGame.Battle.Skills;

namespace TurnBasedGame.Battle.Planning
{
    public sealed class PathDragInputController : MonoBehaviour
    {
        public static PathDragInputController Instance { get; private set; }

        [SerializeField] private float longPressSeconds = 0.2f;
        [SerializeField] private float dragStartPixels = 2f;

        private BattleContext _ctx;
        private bool _isDragging;
        private UnitRuntime _dragUnit;
        private readonly List<GridPosition> _dragPositions = new();
        private bool _suppressInputUntilMouseReleased;
        private bool _isPressTracking;
        private float _pressStartTime;
        private Vector2 _pressStartScreenPosition;
        private UnitRuntime _pressUnit;

        private UnitRuntime _releasedClickUnit;

        public bool IsPressTracking => _isPressTracking;
        public bool IsDragging => _isDragging;

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
            _isDragging = false;
            _dragUnit = null;
            _dragPositions.Clear();
            _suppressInputUntilMouseReleased = false;
            _isPressTracking = false;
            _pressUnit = null;
            _pressStartTime = 0f;
            _pressStartScreenPosition = Vector2.zero;
            _releasedClickUnit = null;

            PathPreviewRenderer.Instance?.ClearPreview();
        }

        public void EndPlanning()
        {
            _ctx = null;
            _isDragging = false;
            _dragUnit = null;
            _dragPositions.Clear();
            _suppressInputUntilMouseReleased = false;
            _isPressTracking = false;
            _pressUnit = null;
            _pressStartTime = 0f;
            _pressStartScreenPosition = Vector2.zero;
            _releasedClickUnit = null;

            PathPreviewRenderer.Instance?.ClearPreview();
        }

        public void TickPlanning()
        {
            if (_ctx == null)
                return;

            if (_suppressInputUntilMouseReleased)
            {
                _releasedClickUnit = null;
                ClearPressTracking();

                if (!Input.GetMouseButton(0))
                    _suppressInputUntilMouseReleased = false;

                return;
            }

            if (TargetSelectionController.Instance != null && TargetSelectionController.Instance.IsSelectingTarget)
                return;

            if (!_isDragging)
            {
                UpdatePressTracking();
                return;
            }

            UpdateDrag();

            if (Input.GetMouseButtonUp(0))
            {
                CommitDrag();
            }
        }

        public bool TryConsumeReleasedClick(out UnitRuntime unit)
        {
            unit = _releasedClickUnit;
            _releasedClickUnit = null;
            return unit != null;
        }

        private void UpdatePressTracking()
        {
            if (TargetSelectionController.Instance != null && TargetSelectionController.Instance.IsSelectingTarget)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                _releasedClickUnit = null;

                if (TryGetHoveredAlly(out var ally))
                {
                    _isPressTracking = true;
                    _pressUnit = ally;
                    _pressStartTime = Time.unscaledTime;
                    _pressStartScreenPosition = Input.mousePosition;
                }
                else
                {
                    ClearPressTracking();
                }
            }

            if (!_isPressTracking)
                return;

            if (_pressUnit == null || !_pressUnit.IsAlive || !_pressUnit.IsPlaced)
            {
                ClearPressTracking();
                return;
            }

            // 短押しで離した時は、ここでクリック選択として確定する
            if (Input.GetMouseButtonUp(0))
            {
                _releasedClickUnit = _pressUnit;
                ClearPressTracking();
                return;
            }

            if (!Input.GetMouseButton(0))
                return;

            bool moved =
                ((Vector2)Input.mousePosition - _pressStartScreenPosition).sqrMagnitude >=
                dragStartPixels * dragStartPixels;

            bool longPressed = Time.unscaledTime - _pressStartTime >= longPressSeconds;

            if (moved || longPressed)
            {
                StartDrag(_pressUnit);
            }
        }

        private void StartDrag(UnitRuntime unit)
        {
            _isDragging = true;
            _dragUnit = unit;
            _dragPositions.Clear();
            _dragPositions.Add(unit.Position);

            PlanningSelectionController.Instance?.ForceSelect(unit);
            TargetSelectionController.Instance?.CancelTargetSelection();
            PathPreviewRenderer.Instance?.SetPreviewPositions(_dragPositions);

            ClearPressTracking();
        }

        private void ClearPressTracking()
        {
            _isPressTracking = false;
            _pressUnit = null;
            _pressStartTime = 0f;
            _pressStartScreenPosition = Vector2.zero;
        }

        private void UpdateDrag()
        {
            if (_dragUnit == null)
                return;

            if (!Input.GetMouseButton(0))
                return;

            if (BattleGridView.Instance == null)
                return;

            if (!BattleGridView.Instance.TryScreenToGrid(Camera.main, Input.mousePosition, out var hovered))
                return;

            var last = _dragPositions[_dragPositions.Count - 1];
            if (hovered == last)
                return;

            if (_dragPositions.Count >= 2)
            {
                var previous = _dragPositions[_dragPositions.Count - 2];
                if (hovered == previous)
                {
                    _dragPositions.RemoveAt(_dragPositions.Count - 1);
                    PathPreviewRenderer.Instance?.SetPreviewPositions(_dragPositions);
                    return;
                }
            }

            int maxStep = GetMaxStep(_dragUnit);

            if (_dragPositions.Count - 1 >= maxStep)
                return;

            if (_ctx.Grid.GetManhattanDistance(last, hovered) != 1)
                return;

            if (_ctx.Grid.IsBlockedBySolidObject(hovered))
                return;

            _dragPositions.Add(hovered);
            PathPreviewRenderer.Instance?.SetPreviewPositions(_dragPositions);
        }

        private int GetMaxStep(UnitRuntime unit)
        {
            var currentAction = _ctx.GetPlannedActionOrWait(unit);
            return currentAction.GetPlannedStep(unit);
        }

        private void CommitDrag()
        {
            _isDragging = false;

            if (_dragUnit == null)
            {
                _dragPositions.Clear();
                PathPreviewRenderer.Instance?.ClearPreview();
                return;
            }

            var currentAction = _ctx.GetPlannedActionOrWait(_dragUnit);

            PlannedUnitAction nextAction;

            if (_dragPositions.Count >= 2)
            {
                var path = new GridPath(_dragPositions);

                if (currentAction.HasSkill)
                {
                    nextAction = PlannedUnitAction.MoveAndSkill(
                        path,
                        currentAction.SkillUse,
                        currentAction.TargetUnit,
                        currentAction.TargetPosition,
                        currentAction.TargetDirection);
                }
                else
                {
                    nextAction = PlannedUnitAction.MoveOnly(path);
                }
            }
            else
            {
                if (currentAction.HasSkill)
                {
                    nextAction = PlannedUnitAction.SkillOnly(
                        currentAction.SkillUse,
                        currentAction.TargetUnit,
                        currentAction.TargetPosition,
                        currentAction.TargetDirection);
                }
                else
                {
                    nextAction = PlannedUnitAction.Wait();
                }
            }

            _ctx.SetPlannedAction(_dragUnit, nextAction);
            PlanningSelectionController.Instance?.ForceSelect(_dragUnit);
            _ctx.Logger.Log($"[Plan Move] {_dragUnit.Name}");

            _dragUnit = null;
            _dragPositions.Clear();
            PathPreviewRenderer.Instance?.ClearPreview();
        }

        private bool IsSkillPlanComplete(PlannedUnitAction action)
        {
            if (action == null || !action.HasSkill || action.SkillUse.Skill == null)
                return false;

            switch (action.SkillUse.Skill.targetMode)
            {
                case TargetMode.SingleEnemy:
                    return action.HasTargetUnit;

                case TargetMode.Position:
                    return action.HasTargetPosition;

                case TargetMode.LineDirection:
                    return action.HasTargetDirection;

                default:
                    return true;
            }
        }

        private bool TryGetHoveredAlly(out UnitRuntime ally)
        {
            ally = null;

            if (_ctx == null)
                return false;

            if (BattleGridView.Instance == null)
                return false;

            if (!BattleGridView.Instance.TryScreenToGrid(Camera.main, Input.mousePosition, out var gridPos))
                return false;

            if (!_ctx.Grid.TryGetUnitAt(gridPos, out var unit))
                return false;

            if (unit == null || !unit.IsAlive)
                return false;

            if (unit.Team != Team.Ally)
                return false;

            if (unit.IsTurret)
                return false;

            ally = unit;
            return true;
        }

        public void CancelInput()
        {
            _isDragging = false;
            _dragUnit = null;
            _dragPositions.Clear();

            _isPressTracking = false;
            _pressUnit = null;
            _pressStartTime = 0f;
            _pressStartScreenPosition = Vector2.zero;
            _releasedClickUnit = null;

            PathPreviewRenderer.Instance?.ClearPreview();
        }

        public void SuppressInputUntilMouseReleased()
        {
            _suppressInputUntilMouseReleased = true;
            _releasedClickUnit = null;
            ClearPressTracking();

            if (_isDragging)
            {
                _isDragging = false;
                _dragUnit = null;
                _dragPositions.Clear();
                PathPreviewRenderer.Instance?.ClearPreview();
            }
        }
    }
}