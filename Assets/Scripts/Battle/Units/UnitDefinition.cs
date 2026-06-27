// Assets/Scripts/Battle/Units/UnitDefinition.cs
using UnityEngine;
using TurnBasedGame.Battle.Skills;

namespace TurnBasedGame.Battle.Units
{
    public enum Team { Ally, Enemy }

    [CreateAssetMenu(menuName = "TurnBasedGame/UnitDefinition")]
    public sealed class UnitDefinition : ScriptableObject
    {
        public string unitName = "Unit";
        public Team team = Team.Ally;

        [Header("Base Stats")]
        [Min(1)] public int maxHP = 10;
        [Min(0)] public int attack = 5;
        [Min(0)] public int speed = 5;
        [Min(0)] public int step = 3; // 経路の最大長（繰り越しなし）

        [Header("Base Resist Add (target adds to attacker ATK)")]
        public float resistSlash = 0f;
        public float resistPierce = 0f;
        public float resistBlunt = 0f;

        [Header("Skills")]
        public SkillDefinition basicAttack;                  // 通常攻撃（近接・CT0）
        public SkillDefinition[] activeSkills = new SkillDefinition[3]; // スキル3枠
    }
}