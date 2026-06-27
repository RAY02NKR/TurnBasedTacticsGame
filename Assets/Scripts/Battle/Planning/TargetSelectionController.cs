using System.Collections.Generic;
using UnityEngine;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Planning
{
    public sealed class TargetSelectionController : MonoBehaviour
    {
        public static TargetSelectionController Instance { get; private set; }

        private BattleContext _ctx;
        private UnitRuntime _actor;
        private SkillUse _pendingSkillUse;
        private int _ignoreInputUntilFrame;
        private bool _waitForMouseRelease;
        private readonly List<UnitRuntime> _validTargets = new();
        private readonly List<GridPosition> _validPositions = new();
        private readonly List<GridDirection> _validDirections = new();

        public bool IsSelectingTarget => _actor != null && _pendingSkillUse.Skill != null;
        public UnitRuntime Actor => _actor;
        public SkillUse PendingSkillUse => _pendingSkillUse;
        public IReadOnlyList<UnitRuntime> ValidTargets => _validTargets;
        public IReadOnlyList<GridPosition> ValidPositions => _validPositions;
        public IReadOnlyList<GridDirection> ValidDirections => _validDirections;

        public bool IsSelectingUnitTarget =>
            IsSelectingTarget &&
            _pendingSkillUse.Skill != null &&
            _pendingSkillUse.Skill.targetMode == TargetMode.SingleEnemy;

        public bool IsSelectingPositionTarget =>
            IsSelectingTarget &&
            _pendingSkillUse.Skill != null &&
            _pendingSkillUse.Skill.targetMode == TargetMode.Position;

        public bool IsSelectingDirectionTarget =>
            IsSelectingTarget &&
            _pendingSkillUse.Skill != null &&
            _pendingSkillUse.Skill.targetMode == TargetMode.LineDirection;

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
            CancelTargetSelection();
        }

        public void EndPlanning()
        {
            _ctx = null;
            CancelTargetSelection();
        }

        public void BeginTargetSelection(BattleContext ctx, UnitRuntime actor, SkillUse use)
        {
            _ctx = ctx;
            _actor = actor;
            _pendingSkillUse = use;

            _waitForMouseRelease = Input.GetMouseButton(0);
            _ignoreInputUntilFrame = Time.frameCount + 2;

            RefreshValidTargets();

            if (_pendingSkillUse.Skill != null)
                _ctx.Logger.Log($"[Target Select] {_actor.Name} : {_pendingSkillUse.Skill.skillName}");
        }

        public void CancelTargetSelection()
        {
            _actor = null;
            _pendingSkillUse = default;
            _validTargets.Clear();
            _validPositions.Clear();
            _validDirections.Clear();

            _waitForMouseRelease = false;
            _ignoreInputUntilFrame = 0;
        }

        public void TickPlanning()
        {
            if (_ctx == null || !IsSelectingTarget)
                return;

            RefreshValidTargets();

            if (_waitForMouseRelease)
            {
                if (!Input.GetMouseButton(0))
                {
                    _waitForMouseRelease = false;
                    _ignoreInputUntilFrame = Time.frameCount + 1;
                }

                return;
            }

            if (Time.frameCount <= _ignoreInputUntilFrame)
                return;

            if (Input.GetMouseButtonDown(1))
            {
                _ctx.Logger.Log("[Target Select Cancel]");
                CancelTargetSelection();
                return;
            }

            if (!Input.GetMouseButtonDown(0))
                return;

            var skill = _pendingSkillUse.Skill;
            if (skill == null)
                return;

            switch (skill.targetMode)
            {
                case TargetMode.SingleEnemy:
                    {
                        if (!TryGetHoveredUnit(out var hoveredUnit))
                            return;

                        if (!IsValidTarget(hoveredUnit))
                            return;

                        CommitUnitTarget(hoveredUnit);
                        return;
                    }

                case TargetMode.Position:
                    {
                        if (!TryGetHoveredGridPosition(out var hoveredPos))
                            return;

                        if (!IsValidPosition(hoveredPos))
                            return;

                        CommitPositionTarget(hoveredPos);
                        return;
                    }

                case TargetMode.LineDirection:
                    {
                        if (!TryGetHoveredGridPosition(out var hoveredPos))
                            return;

                        if (!TryGetDirectionFromHoveredPosition(hoveredPos, out var direction))
                            return;

                        if (!IsValidDirection(direction))
                            return;

                        CommitDirectionTarget(direction);
                        return;
                    }
            }
        }

        private void CommitUnitTarget(UnitRuntime target)
        {
            var currentAction = _ctx.GetPlannedActionOrWait(_actor);

            PlannedUnitAction nextAction =
                currentAction.HasMove && currentAction.MovePath != null
                    ? PlannedUnitAction.MoveAndSkill(currentAction.MovePath, _pendingSkillUse, target, null, null)
                    : PlannedUnitAction.SkillOnly(_pendingSkillUse, target, null, null);

            _ctx.SetPlannedAction(_actor, nextAction);
            _ctx.Logger.Log($"[Plan Skill] {_actor.Name} -> {target.Name}");

            PlanningSelectionController.Instance?.ClearSelection();

            CancelTargetSelection();
        }

        private void CommitPositionTarget(GridPosition targetPosition)
        {
            var currentAction = _ctx.GetPlannedActionOrWait(_actor);

            PlannedUnitAction nextAction =
                currentAction.HasMove && currentAction.MovePath != null
                    ? PlannedUnitAction.MoveAndSkill(currentAction.MovePath, _pendingSkillUse, null, targetPosition, null)
                    : PlannedUnitAction.SkillOnly(_pendingSkillUse, null, targetPosition, null);

            _ctx.SetPlannedAction(_actor, nextAction);
            _ctx.Logger.Log($"[Plan Skill] {_actor.Name} -> {targetPosition}");

            PlanningSelectionController.Instance?.ClearSelection();

            CancelTargetSelection();
        }

        private void CommitDirectionTarget(GridDirection direction)
        {
            var currentAction = _ctx.GetPlannedActionOrWait(_actor);

            PlannedUnitAction nextAction =
                currentAction.HasMove && currentAction.MovePath != null
                    ? PlannedUnitAction.MoveAndSkill(currentAction.MovePath, _pendingSkillUse, null, null, direction)
                    : PlannedUnitAction.SkillOnly(_pendingSkillUse, null, null, direction);

            _ctx.SetPlannedAction(_actor, nextAction);
            _ctx.Logger.Log($"[Plan Skill] {_actor.Name} -> {direction}");

            PlanningSelectionController.Instance?.ClearSelection();

            CancelTargetSelection();
        }

        private void RefreshValidTargets()
        {
            _validTargets.Clear();
            _validPositions.Clear();
            _validDirections.Clear();

            if (!IsSelectingTarget)
                return;

            var skill = _pendingSkillUse.Skill;
            if (skill == null)
                return;

            var origin = GetSkillOriginPosition(_actor);

            switch (skill.targetMode)
            {
                case TargetMode.SingleEnemy:
                    {
                        var targetList = _actor.Team == Team.Ally ? _ctx.Enemies : _ctx.Allies;

                        for (int i = 0; i < targetList.Count; i++)
                        {
                            var target = targetList[i];
                            if (target == null || !target.IsAlive || !target.IsPlaced)
                                continue;

                            if (!BattleGridRangeService.IsInRange(_ctx.Grid, origin, target.Position, skill.range))
                                continue;

                            _validTargets.Add(target);
                        }

                        break;
                    }

                case TargetMode.Position:
                    {
                        foreach (var pos in _ctx.Grid.GetAllPositions())
                        {
                            if (BattleGridRangeService.IsInRange(_ctx.Grid, origin, pos, skill.range))
                                _validPositions.Add(pos);
                        }

                        break;
                    }

                case TargetMode.LineDirection:
                    {
                        TryAddDirectionLine(origin, GridDirection.Up, skill.range);
                        TryAddDirectionLine(origin, GridDirection.Down, skill.range);
                        TryAddDirectionLine(origin, GridDirection.Left, skill.range);
                        TryAddDirectionLine(origin, GridDirection.Right, skill.range);
                        break;
                    }
            }
        }

        private void TryAddDirectionLine(GridPosition origin, GridDirection direction, int range)
        {
            var linePositions = BattleGridRangeService.GetStraightLinePositions(_ctx.Grid, origin, direction, range);
            if (linePositions == null || linePositions.Count == 0)
                return;

            _validDirections.Add(direction);

            for (int i = 0; i < linePositions.Count; i++)
                _validPositions.Add(linePositions[i]);
        }

        private GridPosition GetSkillOriginPosition(UnitRuntime actor)
        {
            var currentAction = _ctx.GetPlannedActionOrWait(actor);

            if (currentAction.HasMove && currentAction.MovePath != null)
                return currentAction.MovePath.End;

            return actor.Position;
        }

        private bool TryGetHoveredUnit(out UnitRuntime unit)
        {
            unit = null;

            if (BattleGridView.Instance == null)
                return false;

            if (!BattleGridView.Instance.TryScreenToGrid(Camera.main, Input.mousePosition, out var gridPos))
                return false;

            if (!_ctx.Grid.TryGetUnitAt(gridPos, out unit))
                return false;

            return unit != null;
        }

        private bool TryGetHoveredGridPosition(out GridPosition position)
        {
            position = default;

            if (BattleGridView.Instance == null)
                return false;

            return BattleGridView.Instance.TryScreenToGrid(Camera.main, Input.mousePosition, out position);
        }

        private bool TryGetDirectionFromHoveredPosition(GridPosition hoveredPosition, out GridDirection direction)
        {
            direction = default;

            var origin = GetSkillOriginPosition(_actor);

            for (int i = 0; i < _validDirections.Count; i++)
            {
                var dir = _validDirections[i];
                var linePositions = BattleGridRangeService.GetStraightLinePositions(
                    _ctx.Grid,
                    origin,
                    dir,
                    _pendingSkillUse.Skill.range);

                for (int p = 0; p < linePositions.Count; p++)
                {
                    if (linePositions[p] == hoveredPosition)
                    {
                        direction = dir;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsValidTarget(UnitRuntime unit)
        {
            if (unit == null)
                return false;

            for (int i = 0; i < _validTargets.Count; i++)
            {
                if (_validTargets[i] == unit)
                    return true;
            }

            return false;
        }

        private bool IsValidPosition(GridPosition position)
        {
            for (int i = 0; i < _validPositions.Count; i++)
            {
                if (_validPositions[i] == position)
                    return true;
            }

            return false;
        }

        private bool IsValidDirection(GridDirection direction)
        {
            for (int i = 0; i < _validDirections.Count; i++)
            {
                if (_validDirections[i] == direction)
                    return true;
            }

            return false;
        }
    }
}