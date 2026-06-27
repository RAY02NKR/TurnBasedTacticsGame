using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.StatusEffects.Actions
{
    public sealed class StatusEffectActionContext
    {
        public UnitRuntime Owner { get; }
        public StatusEffectInstance Instance { get; }

        public UnitRuntime SourceUnit { get; }
        public int ReceivedDamage { get; }
        public bool IsSkillDamage { get; }
        public int MovedSteps { get; }

        public int Stacks => Instance.Stacks;

        public StatusEffectActionContext(
            UnitRuntime owner,
            StatusEffectInstance instance,
            UnitRuntime sourceUnit = null,
            int receivedDamage = 0,
            bool isSkillDamage = false,
            int movedSteps = 0)
        {
            Owner = owner;
            Instance = instance;
            SourceUnit = sourceUnit;
            ReceivedDamage = receivedDamage;
            IsSkillDamage = isSkillDamage;
            MovedSteps = movedSteps;
        }
    }
}