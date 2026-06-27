using System;
using System.Collections.Generic;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Logging;
using TurnBasedGame.Battle.Planning;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Core
{
    public sealed class BattleContext
    {
        public IBattleLogger Logger { get; }
        public BattleGrid Grid { get; }

        public readonly List<UnitRuntime> Allies = new();
        public readonly List<UnitRuntime> Enemies = new();
        public readonly List<UnitRuntime> AllUnits = new();

        private readonly Dictionary<UnitRuntime, PlannedUnitAction> _plannedActions = new();
        private readonly Dictionary<UnitRuntime, int> _plannedOrderByUnit = new();
        public List<UnitRuntime> TurnOrder = new();
        public int TurnIndex = 0;
        public int TurnNumber = 0;
        public UnitRuntime CurrentActingUnit { get; private set; }
        public bool IsBattleOver { get; set; } = false;
        private int _nextPlannedOrder = 0;

        public BattleContext(IBattleLogger logger, BattleGrid grid)
        {
            Logger = logger ?? new NullBattleLogger();
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
        }

        public void AddUnit(UnitRuntime unit)
        {
            if (unit == null) return;

            AllUnits.Add(unit);

            if (unit.Team == Team.Ally)
                Allies.Add(unit);
            else
                Enemies.Add(unit);
        }

        public void SetPlannedAction(UnitRuntime unit, PlannedUnitAction action)
        {
            if (unit == null) return;
            if (action == null) return;

            _plannedActions[unit] = action;
            _plannedOrderByUnit[unit] = _nextPlannedOrder++;
        }

        public bool TryGetPlannedAction(UnitRuntime unit, out PlannedUnitAction action)
        {
            if (unit == null)
            {
                action = null;
                return false;
            }

            return _plannedActions.TryGetValue(unit, out action);
        }

        public int GetPlannedOrder(UnitRuntime unit)
        {
            if (unit == null)
                return int.MaxValue;

            if (_plannedOrderByUnit.TryGetValue(unit, out var order))
                return order;

            return int.MaxValue;
        }

        public PlannedUnitAction GetPlannedActionOrWait(UnitRuntime unit)
        {
            if (TryGetPlannedAction(unit, out var action))
                return action;

            return PlannedUnitAction.Wait();
        }

        public void RemovePlannedAction(UnitRuntime unit)
        {
            if (unit == null) return;
            _plannedActions.Remove(unit);
            _plannedOrderByUnit.Remove(unit);
        }

        public void ClearPlannedActions()
        {
            _plannedActions.Clear();
            _plannedOrderByUnit.Clear();
            _nextPlannedOrder = 0;
        }

        public void SetCurrentActingUnit(UnitRuntime unit)
        {
            CurrentActingUnit = unit;
        }

        public void ClearCurrentActingUnit()
        {
            CurrentActingUnit = null;
        }

        public int CountAliveTurrets(Team team)
        {
            int count = 0;

            for (int i = 0; i < AllUnits.Count; i++)
            {
                var unit = AllUnits[i];
                if (!unit.IsAlive) continue;
                if (unit.Team != team) continue;
                if (!unit.IsTurret) continue;

                count++;
            }

            return count;
        }

        public bool IsAllDead(Team team)
        {
            var list = team == Team.Ally ? Allies : Enemies;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IsAlive) return false;
            }

            return true;
        }
    }
}