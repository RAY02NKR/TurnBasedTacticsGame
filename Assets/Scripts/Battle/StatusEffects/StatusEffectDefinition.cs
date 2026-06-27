using System;
using System.Collections.Generic;
using UnityEngine;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.StatusEffects.Actions;

namespace TurnBasedGame.Battle.StatusEffects
{
    public enum StatType { Attack = 0, Speed = 1, Step = 2 }

    [System.Serializable]
    public struct ResistanceModifier
    {
        public DamageType type;
        public float add; // 例：-0.7（耐性） / +1.2（弱点）
    }

    [Serializable]
    public struct StatModifier
    {
        public StatType stat;
        public int add; // 加算（マイナスでデバフ）
    }

    [CreateAssetMenu(menuName = "TurnBasedGame/StatusEffectDefinition")]
    public sealed class StatusEffectDefinition : ScriptableObject
    {
        public string effectName = "Effect";

        [Header("Family")]
        public StatusFamily family = StatusFamily.None;

        [Header("Stacking")]
        public bool stackable = false;
        [Header("Family Apply Rule")]
        public bool keepCurrentVariantWhenApplied = false;
        [Min(1)] public int maxStacks = 1;
        [Min(-1)] public int durationTurns = 1;
        public List<StatusEffectActionDefinition> onApplyEffects = new();
        public List<StatusEffectActionDefinition> onTurnStartEffects = new();
        public List<StatusEffectActionDefinition> onTurnEndEffects = new();
        public List<StatusEffectActionDefinition> onExpireEffects = new();
        public List<StatusEffectActionDefinition> onDamagedEffects = new();
        public List<TurnBasedGame.Battle.StatusEffects.Actions.StatusEffectActionDefinition> onMovedEffects = new();

        [Header("Stat Modifiers")]
        public List<StatModifier> modifiers = new();

        [Header("Resistance Modifiers")]
        public List<ResistanceModifier> resistanceModifiers = new();

        [Header("Turn End")]
        [Min(0)] public int turnEndDamagePerStack = 0;

    }
}