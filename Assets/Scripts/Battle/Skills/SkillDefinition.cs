using System.Collections.Generic;
using UnityEngine;
using TurnBasedGame.Battle.Skills.Effects;
using TurnBasedGame.Battle.StatusEffects;

namespace TurnBasedGame.Battle.Skills
{
    public enum TargetMode
    {
        SingleEnemy,
        Position,
        LineDirection
    }

    public enum DamageType
    {
        Slash,
        Pierce,
        Blunt
    }

    [CreateAssetMenu(menuName = "TurnBasedGame/SkillDefinition")]
    public sealed class SkillDefinition : ScriptableObject
    {
        public string skillName = "Skill";

        [Header("Power")]
        [Min(0)] public int SkillPower = 1;

        [Header("Damage Type")]
        public DamageType damageType = DamageType.Slash;

        [Header("Planning Move")]
        public bool modifiesStepDuringPlanning = false;
        public int planningStepAdd = 0;

        [Header("Cooldown")]
        [Min(0)] public int cooldownTurns = 0;

        [Header("Use Requirement")]
        public StatusEffectDefinition requiredStatus;
        [Min(0)] public int requiredMinStacks = 0;

        [Header("Target")]
        public TargetMode targetMode = TargetMode.SingleEnemy;
        [Min(0)] public int range = 1;

        [Header("Effects")]
        public List<SkillEffectDefinition> effects = new();
    }
}