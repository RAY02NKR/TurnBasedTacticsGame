using UnityEngine;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Planning
{
    public sealed class PlanningSelectionController : MonoBehaviour
    {
        public static PlanningSelectionController Instance { get; private set; }

        public UnitRuntime SelectedUnit { get; private set; }

        private BattleContext _ctx;

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
            SelectedUnit = null;
        }

        public void EndPlanning()
        {
            _ctx = null;
            SelectedUnit = null;
        }

        public void TickPlanning()
        {
            if (_ctx == null)
                return;

            var targetSelection = TargetSelectionController.Instance;

            if (Input.GetMouseButtonDown(1))
            {
                // 対象指定中の右クリックはTargetSelectionController側に任せる
                if (targetSelection != null && targetSelection.IsSelectingTarget)
                    return;

                ClearSelection();
                return;
            }

            var pathDrag = PathDragInputController.Instance;
            if (pathDrag == null)
                return;

            if (pathDrag.TryConsumeReleasedClick(out var clickedUnit))
            {
                SelectedUnit = clickedUnit;
                _ctx.Logger.Log($"[Select] {clickedUnit.Name}");
            }
        }

        public void ForceSelect(UnitRuntime unit)
        {
            if (unit == null)
                return;

            SelectedUnit = unit;
        }

        public void ClearSelection()
        {
            SelectedUnit = null;
        }
    }
}