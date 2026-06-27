using UnityEngine;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.UI;
using TurnBasedGame.Battle.Planning;
using TurnBasedGame.Battle.Services;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.States
{
    public sealed class ResolveTurnState : IBattleState
    {
        private enum ResolvePhase
        {
            None,
            Move,
            Skill,
            EndWait,
            Finished
        }

        private readonly BattleStateMachine _sm;
        private readonly BattleContext _ctx;

        private UnitRuntime _actor;
        private PlannedUnitAction _action;
        private ResolvePhase _phase;

        private int _moveIndex;
        private float _timer;
        private bool _skillExecuted;

        private GridPosition _moveStart;
        private GridPosition _plannedEnd;

        private const float StartDelay = 0.2f;
        private const float MoveStepInterval = 0.18f;
        private const float SkillDelay = 0.25f;
        private const float EndDelay = 0.35f;

        public ResolveTurnState(BattleStateMachine sm, BattleContext ctx)
        {
            _sm = sm;
            _ctx = ctx;
        }

        public void Enter()
        {
            if (_ctx.IsBattleOver)
            {
                _sm.ChangeState(new BattleEndState(_ctx));
                return;
            }

            if (_ctx.TurnIndex == 0)
                BattlePhaseBanner.Instance?.ShowActionPhase();

            if (_ctx.TurnOrder.Count == 0)
            {
                _ctx.Logger.Warn("No units in turn order.");
                _ctx.IsBattleOver = true;
                _sm.ChangeState(new BattleEndState(_ctx));
                return;
            }

            while (_ctx.TurnIndex < _ctx.TurnOrder.Count && !_ctx.TurnOrder[_ctx.TurnIndex].IsAlive)
                _ctx.TurnIndex++;

            if (_ctx.TurnIndex >= _ctx.TurnOrder.Count)
            {
                _sm.ChangeState(new EndCheckState(_sm, _ctx));
                return;
            }

            _actor = _ctx.TurnOrder[_ctx.TurnIndex];
            _ctx.TurnIndex++;

            _ctx.SetCurrentActingUnit(_actor);

            _action = _ctx.GetPlannedActionOrWait(_actor);

            var teamLabel = _actor.Team == Team.Ally ? "Ally" : "Enemy";
            _ctx.Logger.Log($"[Resolve] {teamLabel} {_actor.Name} start at {(_actor.IsPlaced ? _actor.Position.ToString() : "Unplaced")}");

            _timer = -StartDelay;
            _skillExecuted = false;

            if (_action.HasMove && _action.MovePath != null)
                StartMovePhase();
            else
                StartSkillPhase();
        }

        public void Tick()
        {
            if (_actor == null)
            {
                FinishAction();
                return;
            }

            switch (_phase)
            {
                case ResolvePhase.Move:
                    TickMove();
                    break;

                case ResolvePhase.Skill:
                    TickSkill();
                    break;

                case ResolvePhase.EndWait:
                    TickEndWait();
                    break;
            }
        }

        public void Exit()
        {
            _ctx.ClearCurrentActingUnit();
        }

        private void StartMovePhase()
        {
            int plannedStep = _action.GetPlannedStep(_actor);

            if (!BattleGridMovementService.CanMoveAlongPath(_ctx.Grid, _actor, _action.MovePath, out var reason, plannedStep))
            {
                _ctx.Logger.Warn($"[Move Failed] {_actor.Name}: {reason}");
                StartSkillPhase();
                return;
            }

            _moveStart = _actor.Position;
            _plannedEnd = _action.MovePath.End;
            _moveIndex = 1;
            _phase = ResolvePhase.Move;
        }

        private void TickMove()
        {
            _timer += Time.deltaTime;

            if (_timer < MoveStepInterval)
                return;

            _timer = 0f;

            if (_action.MovePath == null || _moveIndex >= _action.MovePath.Positions.Count)
            {
                FinishMove();
                return;
            }

            if (!_actor.IsAlive)
            {
                FinishMove();
                return;
            }

            var next = _action.MovePath.Positions[_moveIndex];
            _moveIndex++;

            if (!_ctx.Grid.IsInside(next))
            {
                FinishMove();
                return;
            }

            if (_ctx.Grid.IsBlockedBySolidObject(next))
            {
                FinishMove();
                return;
            }

            bool occupiedByOther = _ctx.Grid.TryGetUnitAt(next, out var existing) && existing != _actor;

            if (!occupiedByOther)
            {
                _ctx.Grid.MoveUnit(_actor, next);
                RemoveDeadUnitsFromGrid();
            }

            if (_moveIndex >= _action.MovePath.Positions.Count)
                FinishMove();
        }

        private void FinishMove()
        {
            if (_actor.Position == _moveStart)
                _ctx.Logger.Log($"[Move Blocked] {_actor.Name} stays at {_moveStart}");
            else if (_actor.Position != _plannedEnd)
                _ctx.Logger.Log($"[Move Partial] {_actor.Name} -> {_actor.Position} (planned:{_plannedEnd})");
            else
                _ctx.Logger.Log($"[Move] {_actor.Name} -> {_actor.Position}");

            StartSkillPhase();
        }

        private void StartSkillPhase()
        {
            _phase = ResolvePhase.Skill;
            _timer = 0f;
            _skillExecuted = false;
        }

        private void TickSkill()
        {
            _timer += Time.deltaTime;

            if (_timer < SkillDelay)
                return;

            if (_skillExecuted)
                return;

            _skillExecuted = true;

            ResolveSkillOrWait();

            _phase = ResolvePhase.EndWait;
            _timer = 0f;
        }

        private void ResolveSkillOrWait()
        {
            if (!_actor.IsAlive)
                return;

            if (_action.HasSkill)
            {
                var skill = _action.SkillUse.Skill;

                if (skill == null)
                {
                    _ctx.Logger.Warn($"[Skill Failed] {_actor.Name}: skill is null.");
                }
                else if (_action.SkillUse.IsActive && !_actor.CanUseActiveSkill(_action.SkillUse.ActiveSlot))
                {
                    _ctx.Logger.Warn($"[Skill Failed] {_actor.Name}: cannot use active slot {_action.SkillUse.ActiveSlot}.");
                }
                else if (_action.SkillUse.IsBasic && !_actor.CanUseSkill(skill))
                {
                    _ctx.Logger.Warn($"[Skill Failed] {_actor.Name}: cannot use {skill.skillName}.");
                }
                else
                {
                    SkillExecutor.Execute(
                        _ctx,
                        _actor,
                        _action.SkillUse,
                        _action.TargetUnit,
                        _action.TargetPosition,
                        _action.TargetDirection);
                }

                RemoveDeadUnitsFromGrid();
                return;
            }

            if (!_action.HasMove)
                _ctx.Logger.Log($"[Wait] {_actor.Name}");
        }

        private void TickEndWait()
        {
            _timer += Time.deltaTime;

            if (_timer < EndDelay)
                return;

            FinishAction();
        }

        private void FinishAction()
        {
            if (_phase == ResolvePhase.Finished)
                return;

            _phase = ResolvePhase.Finished;

            if (_actor != null)
                _ctx.RemovePlannedAction(_actor);

            _ctx.ClearCurrentActingUnit();

            _sm.ChangeState(new EndCheckState(_sm, _ctx));
        }

        private void RemoveDeadUnitsFromGrid()
        {
            for (int i = 0; i < _ctx.AllUnits.Count; i++)
            {
                var unit = _ctx.AllUnits[i];
                if (unit == null) continue;
                if (unit.IsAlive) continue;
                if (!unit.IsPlaced) continue;

                _ctx.Grid.RemoveUnit(unit);
                _ctx.Logger.Log($"[Grid Remove] {unit.Name} removed from grid.");
            }
        }
    }
}