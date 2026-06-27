using UnityEngine;

namespace TurnBasedGame.Battle.StatusEffects.Actions
{
    public abstract class StatusEffectActionDefinition : ScriptableObject
    {
        public abstract void Execute(StatusEffectActionContext ctx, StatusEffectApplyBuffer buffer);
    }
}