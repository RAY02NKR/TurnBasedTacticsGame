using UnityEngine;
using UnityEngine.UI;
using TurnBasedGame.Battle.Core;

namespace TurnBasedGame.Battle.UI
{
    public sealed class BattleStartButton : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private Button button;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private bool hideWhenNotPlanning = true;

        private void Awake()
        {
            if (battleManager == null)
                battleManager = FindFirstObjectByType<BattleManager>();

            if (button == null)
                button = GetComponent<Button>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (button != null)
                button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }

        private void Update()
        {
            bool canExecute = battleManager != null && battleManager.IsPlanningPhase;

            if (button != null)
                button.interactable = canExecute;

            if (canvasGroup != null && hideWhenNotPlanning)
            {
                canvasGroup.alpha = canExecute ? 1f : 0f;
                canvasGroup.interactable = canExecute;
                canvasGroup.blocksRaycasts = canExecute;
            }
        }

        private void OnClick()
        {
            if (battleManager == null)
                return;

            battleManager.ExecutePlanningPhase();
        }
    }
}