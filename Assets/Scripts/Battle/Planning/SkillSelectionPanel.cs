using UnityEngine;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Planning
{
    public sealed class SkillSelectionPanel : MonoBehaviour
    {
        public static SkillSelectionPanel Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private Rect panelRect = new Rect(20f, 20f, 300f, 240f);
        [SerializeField] private float normalPanelWidth = 300f;
        [SerializeField] private float reselectPanelWidth = 360f;
        [SerializeField] private float basePanelHeight = 150f;
        [SerializeField] private float skillButtonHeight = 30f;
        [SerializeField] private float reselectExtraHeight = 55f;
        [SerializeField] private float minPanelHeight = 240f;

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

        public bool IsPanelOpen
        {
            get
            {
                return _ctx != null
                    && PlanningSelectionController.Instance != null
                    && PlanningSelectionController.Instance.SelectedUnit != null;
            }
        }

        public bool ShouldBlockGridInput
        {
            get
            {
                if (!IsPanelOpen)
                    return false;

                var unit = PlanningSelectionController.Instance.SelectedUnit;
                var action = _ctx.GetPlannedActionOrWait(unit);
                var currentRect = GetCurrentPanelRect(unit, action);

                Vector2 mouseGuiPosition = new Vector2(
                    Input.mousePosition.x,
                    Screen.height - Input.mousePosition.y);

                return currentRect.Contains(mouseGuiPosition);
            }
        }

        public void BeginPlanning(BattleContext ctx)
        {
            _ctx = ctx;
        }

        public void EndPlanning()
        {
            _ctx = null;
        }

        private void OnGUI()
        {
            if (_ctx == null)
                return;

            var selection = PlanningSelectionController.Instance;
            if (selection == null || selection.SelectedUnit == null)
                return;

            var pathDrag = PathDragInputController.Instance;
            if (pathDrag != null && pathDrag.IsDragging)
                return;

            var targetSelection = TargetSelectionController.Instance;
            if (targetSelection != null && targetSelection.IsSelectingTarget)
                return;

            var unit = selection.SelectedUnit;
            if (unit == null || !unit.IsAlive)
                return;

            var currentAction = _ctx.GetPlannedActionOrWait(unit);
            bool isReselect = IsSkillPlanComplete(currentAction);

            var currentRect = GetCurrentPanelRect(unit, currentAction);

            GUILayout.BeginArea(currentRect, GUI.skin.box);

            GUILayout.Label($"Selected: {unit.Name}");

            var moveText = currentAction.HasMove && currentAction.MovePath != null
                ? $"{currentAction.MovePath.Start} -> {currentAction.MovePath.End}"
                : "None";

            GUILayout.Label($"Move: {moveText}");

            if (isReselect)
            {
                string currentSkillName = currentAction.SkillUse.Skill != null
                    ? currentAction.SkillUse.Skill.skillName
                    : "None";

                GUILayout.Space(4f);
                GUILayout.Label($"Current Skill: {currentSkillName}");
                GUILayout.Label("Select a skill below to reselect.");
            }

            GUILayout.Space(6f);
            GUILayout.Label("Skills");

            DrawBasicSkillButton(unit);

            if (unit.ActiveSkills != null)
            {
                for (int i = 0; i < unit.ActiveSkills.Length; i++)
                {
                    DrawActiveSkillButton(unit, i);
                }
            }

            GUILayout.Space(8f);

            if (GUILayout.Button("Clear Skill"))
            {
                ClearSkill(unit);
            }

            if (GUILayout.Button("Clear Plan"))
            {
                _ctx.SetPlannedAction(unit, PlannedUnitAction.Wait());
                TargetSelectionController.Instance?.CancelTargetSelection();
                _ctx.Logger.Log($"[Plan Clear] {unit.Name}");
            }

            GUILayout.EndArea();
        }

        private Rect GetCurrentPanelRect(UnitRuntime unit, PlannedUnitAction action)
        {
            bool isReselect = IsSkillPlanComplete(action);

            float width = isReselect
                ? reselectPanelWidth
                : normalPanelWidth;

            int skillButtonCount = CountSkillButtons(unit);

            float height = basePanelHeight + skillButtonCount * skillButtonHeight;

            if (isReselect)
                height += reselectExtraHeight;

            height = Mathf.Max(height, minPanelHeight);

            return new Rect(panelRect.x, panelRect.y, width, height);
        }

        private int CountSkillButtons(UnitRuntime unit)
        {
            if (unit == null)
                return 0;

            int count = 0;

            if (unit.BasicAttack != null)
                count++;

            if (unit.ActiveSkills != null)
            {
                for (int i = 0; i < unit.ActiveSkills.Length; i++)
                {
                    if (unit.ActiveSkills[i] != null)
                        count++;
                }
            }

            return count;
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

        private void DrawBasicSkillButton(UnitRuntime unit)
        {
            var basic = unit.BasicAttack;
            if (basic == null)
                return;

            GUI.enabled = unit.CanUseSkill(basic);

            if (GUILayout.Button($"Basic : {basic.skillName}", GUILayout.Height(26f)))
            {
                PathDragInputController.Instance?.SuppressInputUntilMouseReleased();
                TargetSelectionController.Instance?.BeginTargetSelection(_ctx, unit, SkillUse.Basic(basic));
            }

            GUI.enabled = true;
        }

        private void DrawActiveSkillButton(UnitRuntime unit, int slot)
        {
            var skill = unit.ActiveSkills[slot];
            if (skill == null)
                return;

            GUI.enabled = unit.CanUseActiveSkill(slot);

            if (GUILayout.Button($"Skill {slot + 1} : {skill.skillName}", GUILayout.Height(26f)))
            {
                PathDragInputController.Instance?.SuppressInputUntilMouseReleased();
                TargetSelectionController.Instance?.BeginTargetSelection(_ctx, unit, SkillUse.Active(slot, skill));
            }

            GUI.enabled = true;
        }

        private void ClearSkill(UnitRuntime unit)
        {
            var currentAction = _ctx.GetPlannedActionOrWait(unit);

            PlannedUnitAction nextAction;

            if (currentAction.HasMove && currentAction.MovePath != null)
                nextAction = PlannedUnitAction.MoveOnly(currentAction.MovePath);
            else
                nextAction = PlannedUnitAction.Wait();

            _ctx.SetPlannedAction(unit, nextAction);
            TargetSelectionController.Instance?.CancelTargetSelection();
            _ctx.Logger.Log($"[Plan Skill Clear] {unit.Name}");
        }
    }
}