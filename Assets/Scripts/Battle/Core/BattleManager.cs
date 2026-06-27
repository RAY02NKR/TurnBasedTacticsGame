using System.Collections.Generic;
using UnityEngine;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Logging;
using TurnBasedGame.Battle.States;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Core
{
    public sealed class BattleManager : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int gridWidth = 6;
        [SerializeField] private int gridHeight = 6;

        [Header("Initial Unit Definitions")]
        [SerializeField] private List<UnitDefinition> allyDefinitions = new();
        [SerializeField] private List<UnitDefinition> enemyDefinitions = new();

        private BattleContext _ctx;
        private BattleStateMachine _sm;

        public BattleContext Context => _ctx;
        public bool IsPlanningPhase => _sm != null && _sm.Current is PlanningState;

        private void Start()
        {
            var logger = new UnityDebugBattleLogger();
            var grid = new BattleGrid(gridWidth, gridHeight);

            _ctx = new BattleContext(logger, grid);

            _sm = new BattleStateMachine();
            _sm.ChangeState(new InitBattleState(_sm, _ctx, allyDefinitions, enemyDefinitions));
        }

        private void Update()
        {
            _sm?.Tick();
        }

        public void ExecutePlanningPhase()
        {
            if (_sm == null)
                return;

            if (_sm.Current is PlanningState planningState)
                planningState.ExecutePlans();
        }
    }
}