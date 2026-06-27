using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.Units;
using TurnBasedGame.Battle.StatusEffects;

namespace TurnBasedGame.Battle.Services
{
    public readonly struct DamageResult
    {
        public readonly int Amount;
        public readonly int TargetHPBefore;
        public readonly int TargetHPAfter;
        public readonly bool TargetDied;

        public DamageResult(int amount, int before, int after)
        {
            Amount = amount;
            TargetHPBefore = before;
            TargetHPAfter = after;
            TargetDied = (before > 0) && (after <= 0);
        }
    }

    public static class DamageService
    {
        // damage = Attack * SkillPower（SkillPower=1が等倍）
        public static int Calculate(UnitRuntime attacker, UnitRuntime target, SkillDefinition skill)
        {
            int power = (skill != null) ? skill.SkillPower : 1;
            var type = (skill != null) ? skill.damageType : DamageType.Slash;

            // target耐性を attackerAttack に加算（弱点=＋、耐性=−）
            float effectiveAtk = attacker.Attack + target.GetResistanceAdd(type);
            if (effectiveAtk < 0f) effectiveAtk = 0f;

            // 切り捨て（C#の (int) でOK）
            int damage = (int)(effectiveAtk * power);
            if (damage < 0) damage = 0;

            return damage;
        }

        public static DamageResult Deal(UnitRuntime attacker, UnitRuntime target, SkillDefinition skill)
        {
            int before = target.CurrentHP;
            int amount = Calculate(attacker, target, skill);

            target.TakeDamage(amount);

            int actualDamage = before - target.CurrentHP;

            if (actualDamage > 0)
            {
                StatusService.ExecuteDamagedEffects(
                    target,
                    attacker,
                    actualDamage,
                    isSkillDamage: true);
            }

            return new DamageResult(actualDamage, before, target.CurrentHP);
        }
    }
}