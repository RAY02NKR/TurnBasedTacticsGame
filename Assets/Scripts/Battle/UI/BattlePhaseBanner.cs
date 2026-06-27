using System.Collections;
using TMPro;
using UnityEngine;

namespace TurnBasedGame.Battle.UI
{
    public sealed class BattlePhaseBanner : MonoBehaviour
    {
        public static BattlePhaseBanner Instance { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text mainText;
        [SerializeField] private TMP_Text subText;

        [Header("Timing")]
        [SerializeField] private float fadeInTime = 0.15f;
        [SerializeField] private float holdTime = 0.65f;
        [SerializeField] private float fadeOutTime = 0.25f;

        private Coroutine _routine;

        private void Awake()
        {
            Instance = this;
            HideImmediate();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void ShowTurnStart(int turnNumber)
        {
            Show($"TURN {turnNumber}", "計画フェーズ");
        }

        public void ShowPlanning()
        {
            Show("PLANNING", "行動を計画してください");
        }

        public void ShowActionPhase()
        {
            Show("ACTION", "戦闘開始");
        }

        public void ShowVictory()
        {
            Show("VICTORY", "敵を全滅させました");
        }

        public void ShowDefeat()
        {
            Show("DEFEAT", "味方が全滅しました");
        }

        public void Show(string main, string sub = "")
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(ShowRoutine(main, sub));
        }

        private IEnumerator ShowRoutine(string main, string sub)
        {
            if (mainText != null)
                mainText.text = main;

            if (subText != null)
                subText.text = sub;

            SetAlpha(0f);

            float timer = 0f;

            while (timer < fadeInTime)
            {
                timer += Time.deltaTime;
                SetAlpha(Mathf.Clamp01(timer / fadeInTime));
                yield return null;
            }

            SetAlpha(1f);

            yield return new WaitForSeconds(holdTime);

            timer = 0f;

            while (timer < fadeOutTime)
            {
                timer += Time.deltaTime;
                SetAlpha(1f - Mathf.Clamp01(timer / fadeOutTime));
                yield return null;
            }

            HideImmediate();
            _routine = null;
        }

        private void SetAlpha(float alpha)
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = alpha;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void HideImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (mainText != null)
                mainText.text = "";

            if (subText != null)
                subText.text = "";
        }
    }
}