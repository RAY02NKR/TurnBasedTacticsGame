// Assets/Scripts/Battle/States/InitBattleState.cs
using System;
using System.Collections.Generic;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Grid.Debug;
using TurnBasedGame.Battle.Services;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.States
{
    public sealed class InitBattleState : IBattleState
    {
        private readonly BattleStateMachine _sm;
        private readonly BattleContext _ctx;
        private readonly IReadOnlyList<UnitDefinition> _allyDefs;
        private readonly IReadOnlyList<UnitDefinition> _enemyDefs;

        public InitBattleState(
            BattleStateMachine sm,
            BattleContext ctx,
            IReadOnlyList<UnitDefinition> allyDefs,
            IReadOnlyList<UnitDefinition> enemyDefs)
        {
            _sm = sm;
            _ctx = ctx;
            _allyDefs = allyDefs;
            _enemyDefs = enemyDefs;
        }

        public void Enter()
        {
            _ctx.Allies.Clear();
            _ctx.Enemies.Clear();
            _ctx.AllUnits.Clear();

            Spawn(_allyDefs, _ctx.Allies);
            Spawn(_enemyDefs, _ctx.Enemies);

            _ctx.AllUnits.AddRange(_ctx.Allies);
            _ctx.AllUnits.AddRange(_ctx.Enemies);

            PlaceInitialUnits();

            _ctx.TurnOrder = TurnOrderService.BuildTurnOrder(_ctx);
            _ctx.TurnIndex = 0;

            _ctx.Logger.Log("=== Battle Init ===");
            for (int i = 0; i < _ctx.TurnOrder.Count; i++)
            {
                _ctx.Logger.Log($"TurnOrder[{i}] {_ctx.TurnOrder[i]}");
            }

            BattleGridDebugLogger.Log(_ctx.Logger, _ctx.Grid, "=== Battle Grid ===");

            _sm.ChangeState(new TurnStartState(_sm, _ctx));
        }

        public void Tick() { }
        public void Exit() { }

        private static void Spawn(IReadOnlyList<UnitDefinition> defs, List<UnitRuntime> targetList)
        {
            if (defs == null) return;

            for (int i = 0; i < defs.Count; i++)
            {
                if (defs[i] == null) continue;
                targetList.Add(UnitFactory.Create(defs[i]));
            }
        }

        private void PlaceInitialUnits()
        {
            var placements = new List<UnitPlacementData>();

            for (int i = 0; i < _ctx.Allies.Count; i++)
            {
                int x = i % _ctx.Grid.Width;
                int y = i / _ctx.Grid.Width;

                if (y >= _ctx.Grid.Height)
                    throw new InvalidOperationException("味方の初期配置数が盤面サイズを超えています。");

                placements.Add(new UnitPlacementData(_ctx.Allies[i], new GridPosition(x, y)));
            }

            for (int i = 0; i < _ctx.Enemies.Count; i++)
            {
                int x = _ctx.Grid.Width - 1 - (i % _ctx.Grid.Width);
                int y = _ctx.Grid.Height - 1 - (i / _ctx.Grid.Width);

                if (y < 0)
                    throw new InvalidOperationException("敵の初期配置数が盤面サイズを超えています。");

                placements.Add(new UnitPlacementData(_ctx.Enemies[i], new GridPosition(x, y)));
            }

            BattleGridPlacementService.PlaceInitialUnits(_ctx.Grid, placements);
        }
    }
}