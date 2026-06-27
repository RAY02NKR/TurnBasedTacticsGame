using TMPro;
using UnityEngine;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.UI
{
    public sealed class BattleResultPanel : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text subText;

        [Header("Text")]
        [SerializeField] private string victoryText = "VICTORY";
        [SerializeField] private string defeatText = "DEFEAT";
        [SerializeField] private string victorySubText = "敵を全滅させました";
        [SerializeField] private string defeatSubText = "味方が全滅しました";

        private bool _shown;

        private void Awake()
        {
            if (battleManager == null)
                battleManager = FindFirstObjectByType<BattleManager>();

            HideImmediate();
        }

        private void Update()
        {
            if (_shown)
                return;

            if (battleManager == null || battleManager.Context == null)
                return;

            var ctx = battleManager.Context;

            if (!ctx.IsBattleOver)
                return;

            if (ctx.IsAllDead(Team.Enemy))
            {
                Show(victoryText, victorySubText);
                return;
            }

            if (ctx.IsAllDead(Team.Ally))
            {
                Show(defeatText, defeatSubText);
                return;
            }

            Show("BATTLE END", "");
        }

        private void Show(string main, string sub)
        {
            _shown = true;

            if (resultText != null)
                resultText.text = main;

            if (subText != null)
                subText.text = sub;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void HideImmediate()
        {
            _shown = false;

            if (resultText != null)
                resultText.text = "";

            if (subText != null)
                subText.text = "";

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
}