using System;
using System.Collections.Generic;
using UnityEngine;
using TurnBasedGame.Battle.Units;
using TurnBasedGame.Battle.StatusEffects;
using TurnBasedGame.Battle.StatusEffects.Actions;

namespace TurnBasedGame.Battle.Grid
{
    public sealed class BattleGrid
    {
        private readonly Dictionary<GridPosition, UnitRuntime> _unitsByPosition = new();

        public int Width { get; }
        public int Height { get; }

        public BattleGrid(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
        }

        public bool IsInside(GridPosition pos)
        {
            return pos.x >= 0 && pos.x < Width
                && pos.y >= 0 && pos.y < Height;
        }

        public int GetManhattanDistance(GridPosition a, GridPosition b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        public bool IsInRange(GridPosition from, GridPosition to, int range)
        {
            return GetManhattanDistance(from, to) <= range;
        }

        public bool IsBlockedBySolidObject(GridPosition pos)
        {
            return false;
        }

        public bool IsOccupied(GridPosition pos)
        {
            return _unitsByPosition.ContainsKey(pos);
        }

        public bool TryGetUnitAt(GridPosition pos, out UnitRuntime unit)
        {
            return _unitsByPosition.TryGetValue(pos, out unit);
        }

        public bool CanPlace(UnitRuntime unit, GridPosition pos)
        {
            if (unit == null) return false;
            if (!IsInside(pos)) return false;

            if (_unitsByPosition.TryGetValue(pos, out var existing))
                return existing == unit;

            return true;
        }

        public void PlaceUnit(UnitRuntime unit, GridPosition pos)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            if (!IsInside(pos))
                throw new InvalidOperationException($"盤面外です: {pos}");

            if (_unitsByPosition.TryGetValue(pos, out var existing) && existing != unit)
                throw new InvalidOperationException($"配置先マスは使用中です: {pos}");

            if (unit.IsPlaced)
            {
                _unitsByPosition.Remove(unit.Position);
            }

            _unitsByPosition[pos] = unit;
            unit.PlaceAt(pos);
        }

        public void RemoveUnit(UnitRuntime unit)
        {
            if (unit == null) return;
            if (!unit.IsPlaced) return;

            _unitsByPosition.Remove(unit.Position);
            unit.RemoveFromGrid();
        }

        public bool CanMove(UnitRuntime unit, GridPosition to)
        {
            if (unit == null) return false;
            if (!unit.IsPlaced) return false;
            if (!IsInside(to)) return false;

            if (_unitsByPosition.TryGetValue(to, out var existing))
                return existing == unit;

            return true;
        }

        public void MoveUnit(UnitRuntime unit, GridPosition to)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            if (!unit.IsPlaced)
                throw new InvalidOperationException("未配置ユニットは移動できません。");

            if (!IsInside(to))
                throw new InvalidOperationException($"移動先が盤面外です: {to}");

            if (_unitsByPosition.TryGetValue(to, out var existing) && existing != unit)
                throw new InvalidOperationException($"移動先マスは使用中です: {to}");

            var from = unit.Position;

            if (from == to)
                return;

            _unitsByPosition.Remove(from);
            _unitsByPosition[to] = unit;
            unit.MoveTo(to);

            var buffer = new StatusEffectApplyBuffer();
            StatusService.ExecuteMovedEffects(unit, 1);
            StatusService.FlushPendingApplies(unit, buffer);
        }

        public IEnumerable<GridPosition> GetAllPositions()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    yield return new GridPosition(x, y);
                }
            }
        }

        public IEnumerable<UnitRuntime> GetAllUnits()
        {
            return _unitsByPosition.Values;
        }
    }
}