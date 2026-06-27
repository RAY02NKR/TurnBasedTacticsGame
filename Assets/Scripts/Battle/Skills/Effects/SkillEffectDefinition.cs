using UnityEngine;

namespace TurnBasedGame.Battle.Skills.Effects
{
    public abstract class SkillEffectDefinition : ScriptableObject
    {
        public abstract void Apply(SkillEffectContext ctx);
    }
}