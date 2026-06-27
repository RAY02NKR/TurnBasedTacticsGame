using TurnBasedGame.Battle.Skills;

namespace TurnBasedGame.Battle.Skills
{
    public readonly struct SkillUse
    {
        public readonly SkillDefinition Skill;
        public readonly int ActiveSlot; // -1=Basic, 0..=Active

        private SkillUse(SkillDefinition skill, int activeSlot)
        {
            Skill = skill;
            ActiveSlot = activeSlot;
        }

        public bool IsBasic => ActiveSlot < 0;
        public bool IsActive => ActiveSlot >= 0;

        public static SkillUse Basic(SkillDefinition skill) => new SkillUse(skill, -1);
        public static SkillUse Active(int slot, SkillDefinition skill) => new SkillUse(skill, slot);
    }
}