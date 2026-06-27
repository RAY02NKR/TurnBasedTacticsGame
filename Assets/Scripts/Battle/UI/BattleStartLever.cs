using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using TurnBasedGame.Battle.Core;

namespace TurnBasedGame.Battle.UI
{
    public sealed class BattleStartLever : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private BattleManager battleManager;

        [Header("UI")]
        [SerializeField] private RectTransform trackRect;
        [SerializeField] private RectTransform handleRect;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text label;
        [Header("Settings")]
        [Header("Tilt")]
        [SerializeField] private RectTransform leverPivotRect;
        [SerializeField] private float topTiltAngleX = 0f;
        [SerializeField] private float bottomTiltAngleX = 180f;
        [SerializeField, Range(0.5f, 1f)] private float executeThreshold = 0.9f;
        [SerializeField] private float returnSpeed = 5f;
        [SerializeField] private bool hideWhenNotPlanning = true;
        private bool _wasPlanning;
        private int _lastTurnNumber = -1;
        private bool _dragging;
        private bool _executed;
        private float _progress;

        private void Awake()
        {
            if (battleManager == null)
                battleManager = FindFirstObjectByType<BattleManager>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            bool isPlanning = battleManager != null && battleManager.IsPlanningPhase;

            if (isPlanning && !_wasPlanning)
            {
                ResetLever();
            }

            if (isPlanning && battleManager.Context != null)
            {
                int currentTurn = battleManager.Context.TurnNumber;

                if (_lastTurnNumber != currentTurn)
                {
                    _lastTurnNumber = currentTurn;
                    ResetLever();
                }
            }

            _wasPlanning = isPlanning;

            if (canvasGroup != null)
            {
                bool visible = !hideWhenNotPlanning || isPlanning;

                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = isPlanning;
                canvasGroup.blocksRaycasts = isPlanning;
            }

            if (!isPlanning)
                return;

            if (!_dragging && !_executed && _progress > 0f)
            {
                float next = Mathf.MoveTowards(_progress, 0f, returnSpeed * Time.deltaTime);
                SetProgress(next);
            }

            UpdateLabel();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanUse())
                return;

            _dragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!CanUse())
                return;

            if (trackRect == null || handleRect == null)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    trackRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localPoint))
            {
                return;
            }

            float topY = GetTopY();
            float bottomY = GetBottomY();

            float progress = Mathf.InverseLerp(topY, bottomY, localPoint.y);
            SetProgress(progress);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!CanUse())
                return;

            _dragging = false;

            if (_progress >= executeThreshold)
            {
                _executed = true;
                SetProgress(1f);
                battleManager.ExecutePlanningPhase();
            }
        }

        private bool CanUse()
        {
            return battleManager != null
                && battleManager.IsPlanningPhase
                && !_executed;
        }

        private void SetProgress(float progress)
        {
            _progress = Mathf.Clamp01(progress);

            RectTransform pivot = leverPivotRect != null ? leverPivotRect : handleRect;

            if (pivot != null)
            {
                float angle = Mathf.Lerp(topTiltAngleX, bottomTiltAngleX, _progress);
                pivot.localRotation = Quaternion.Euler(angle, 0f, 0f);
            }
        }

        private void ResetLever()
        {
            _dragging = false;
            _executed = false;
            SetProgress(0f);
            UpdateLabel();
        }

        private float GetTopY()
        {
            if (trackRect == null || handleRect == null)
                return 0f;

            return trackRect.rect.height * 0.5f - handleRect.rect.height * 0.5f;
        }

        private float GetBottomY()
        {
            if (trackRect == null || handleRect == null)
                return 0f;

            return -trackRect.rect.height * 0.5f + handleRect.rect.height * 0.5f;
        }

        private void UpdateLabel()
        {
            if (label == null)
                return;

            if (_executed)
            {
                label.text = "戦闘開始";
                return;
            }

            if (_dragging && _progress >= executeThreshold)
            {
                label.text = "離して開始";
                return;
            }

            label.text = "下にドラッグ";
        }
    }
}