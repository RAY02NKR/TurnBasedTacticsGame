using UnityEngine;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Skills.Effects
{
    [CreateAssetMenu(
        fileName = "SummonTurretEffect",
        menuName = "Battle/Skills/Effects/SummonTurret")]
    public sealed class SummonTurretEffectDefinition : SkillEffectDefinition
    {
        [Header("Turret Status")]
        [Min(1)] public int turretAttack = 1;
        [Min(0)] public int turretSpeed = 0;
        [Min(0)] public int turretStep = 0;

        [Header("Turret HP")]
        [Range(0f, 1f)] public float hpRateFromCaster = 0.3f;

        [Header("Turret Skills")]
        public SkillDefinition turretBasicAttack;
        public SkillDefinition[] turretActiveSkills;

        [Header("Limit")]
        [Min(1)] public int maxTurretCount = 2;

        [Header("Summon")]
        [Min(1)] public int summonRange = 1;

        public override void Apply(SkillEffectContext ctx)
        {
            if (ctx == null) return;
            if (ctx.Actor == null) return;
            if (ctx.Battle == null) return;
            if (ctx.Battle.Grid == null) return;
            if (!ctx.TargetPosition.HasValue) return;

            if (BattleGridSummonService.CountPlacedTurrets(ctx.Battle.Grid, ctx.Actor.Team) >= maxTurretCount)
            {
                ctx.Battle.Logger.Log($"[Summon Failed] {ctx.Actor.Name} : turret limit reached.");
                return;
            }

            int turretHp = Mathf.Max(1, Mathf.FloorToInt(ctx.Actor.MaxHP * hpRateFromCaster));

            var turret = new UnitRuntime(
                name: $"{ctx.Actor.Name}_Turret",
                team: ctx.Actor.Team,
                maxHP: turretHp,
                attack: turretAttack,
                speed: turretSpeed,
                step: turretStep,
                basicAttack: turretBasicAttack,
                activeSkills: turretActiveSkills,
                resistSlash: 0f,
                resistPierce: 0f,
                resistBlunt: 0f,
                isTurret: true
            );

            var summonPosition = ctx.TargetPosition.Value;

            if (!BattleGridSummonService.TrySummonUnit(
                    ctx.Battle.Grid,
                    ctx.Actor,
                    turret,
                    summonPosition,
                    summonRange,
                    out var reason))
            {
                ctx.Battle.Logger.Log($"[Summon Failed] {ctx.Actor.Name} -> {summonPosition} : {reason}");
                return;
            }

            ctx.Battle.AddUnit(turret);
            ctx.Battle.Logger.Log($"[Summon] {ctx.Actor.Name} -> {summonPosition} : {turret.Name}");
        }
    }
}