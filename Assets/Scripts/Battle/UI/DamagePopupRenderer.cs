using System.Collections.Generic;
using UnityEngine;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.UI
{
    public sealed class DamagePopupRenderer : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private float yOffset = 0.75f;
        [SerializeField] private float zOffset = -0.3f;
        [SerializeField] private float moveSpeed = 0.8f;
        [SerializeField] private float lifeTime = 0.8f;
        [SerializeField] private float characterSize = 0.18f;
        [SerializeField] private int fontSize = 64;
        [SerializeField] private Color damageColor = Color.red;

        private readonly HashSet<UnitRuntime> _subscribedUnits = new();
        private readonly List<Popup> _popups = new();

        private void Awake()
        {
            if (battleManager == null)
                battleManager = FindFirstObjectByType<BattleManager>();
        }

        private void OnDestroy()
        {
            foreach (var unit in _subscribedUnits)
            {
                if (unit != null)
                    unit.Damaged -= OnUnitDamaged;
            }

            _subscribedUnits.Clear();
        }

        private void Update()
        {
            SubscribeUnits();
            UpdatePopups();
        }

        private void SubscribeUnits()
        {
            if (battleManager == null || battleManager.Context == null)
                return;

            var units = battleManager.Context.AllUnits;

            for (int i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                if (unit == null) continue;
                if (_subscribedUnits.Contains(unit)) continue;

                unit.Damaged += OnUnitDamaged;
                _subscribedUnits.Add(unit);
            }
        }

        private void OnUnitDamaged(UnitRuntime unit, int damage, int beforeHp, int afterHp)
        {
            if (unit == null) return;

            Vector3 position = Vector3.zero;

            if (unit.IsPlaced && BattleGridView.Instance != null)
            {
                position = BattleGridView.Instance.GetCellCenter(unit.Position);
            }

            position += new Vector3(0f, yOffset, zOffset);

            CreatePopup(position, damage);
        }

        private void CreatePopup(Vector3 position, int damage)
        {
            var obj = new GameObject($"DamagePopup_{damage}");
            obj.transform.SetParent(transform, false);
            obj.transform.position = position;

            var text = obj.AddComponent<TextMesh>();
            text.text = damage.ToString();
            text.characterSize = characterSize;
            text.fontSize = fontSize;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = damageColor;

            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sortingOrder = 100;

            _popups.Add(new Popup(obj, text, lifeTime));
        }

        private void UpdatePopups()
        {
            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                var popup = _popups[i];

                if (popup.Object == null)
                {
                    _popups.RemoveAt(i);
                    continue;
                }

                popup.Elapsed += Time.deltaTime;

                popup.Object.transform.position += Vector3.up * moveSpeed * Time.deltaTime;

                float rate = Mathf.Clamp01(popup.Elapsed / popup.LifeTime);
                float alpha = 1f - rate;

                var color = popup.Text.color;
                color.a = alpha;
                popup.Text.color = color;

                if (popup.Elapsed >= popup.LifeTime)
                {
                    Destroy(popup.Object);
                    _popups.RemoveAt(i);
                }
                else
                {
                    _popups[i] = popup;
                }
            }
        }

        private struct Popup
        {
            public GameObject Object;
            public TextMesh Text;
            public float LifeTime;
            public float Elapsed;

            public Popup(GameObject obj, TextMesh text, float lifeTime)
            {
                Object = obj;
                Text = text;
                LifeTime = lifeTime;
                Elapsed = 0f;
            }
        }
    }
}