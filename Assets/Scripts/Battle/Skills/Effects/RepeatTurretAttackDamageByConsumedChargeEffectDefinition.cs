using System.Collections.Generic;
using UnityEngine;
using TurnBasedGame.Battle.Services;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Skills.Effects
{
    [CreateAssetMenu(
        fileName = "RepeatTurretAttackDamageByConsumedChargeEffect",
        menuName = "Battle/Skills/Effects/RepeatTurretAttackDamageByConsumedCharge")]
    public sealed class RepeatTurretAttackDamageByConsumedChargeEffectDefinition : SkillEffectDefinition
    {
        public override void Apply(SkillEffectContext ctx)
        {
            if (ctx == null) return;
            if (ctx.Actor == null) return;
            if (ctx.Battle == null) return;
            if (ctx.Skill == null) return;
            if (ctx.ConsumedCharge <= 0) return;

            int turretAttack = GetHighestAlliedTurretAttack(ctx);
            if (turretAttack <= 0) return;

            var targets = GetTargets(ctx);
            if (targets.Count == 0) return;

            for (int t = 0; t < targets.Count; t++)
            {
                var target = targets[t];

                if (target == null) continue;
                if (!target.IsAlive) continue;
                if (target.Team == ctx.Actor.Team) continue;

                for (int i = 0; i < ctx.ConsumedCharge; i++)
                {
                    if (!target.IsAlive) break;

                    int before = target.CurrentHP;
                    target.TakeDamage(turretAttack);
                    int actualDamage = before - target.CurrentHP;

                    if (actualDamage > 0)
                        ctx.LastDamageResult = new DamageResult(actualDamage, before, target.CurrentHP);
                }
            }
        }

        private List<UnitRuntime> GetTargets(SkillEffectContext ctx)
        {
            var targets = new List<UnitRuntime>();

            switch (ctx.Skill.targetMode)
            {
                case TargetMode.SingleEnemy:
                    if (ctx.Target != null)
                        targets.Add(ctx.Target);
                    break;

                case TargetMode.Position:
                    if (ctx.TargetPosition.HasValue &&
                        ctx.Battle.Grid.TryGetUnitAt(ctx.TargetPosition.Value, out var positionTarget) &&
                        positionTarget != null)
                    {
                        targets.Add(positionTarget);
                    }
                    break;

                case TargetMode.LineDirection:
                    {
                        var origin = ctx.Actor.Position;

                        var currentAction = ctx.Battle.GetPlannedActionOrWait(ctx.Actor);
                        if (currentAction.HasMove && currentAction.MovePath != null)
                            origin = currentAction.MovePath.End;

                        var lineTargets = DirectionalSkillTargetingService.GetUnitsOnSelectedLineFromPosition(
                            ctx.Battle,
                            ctx.Actor,
                            origin,
                            ctx.Skill.range,
                            ctx.TargetDirection);

                        for (int i = 0; i < lineTargets.Count; i++)
                            targets.Add(lineTargets[i]);

                        break;
                    }
            }

            return targets;
        }

        private int GetHighestAlliedTurretAttack(SkillEffectContext ctx)
        {
            int maxAttack = 0;

            for (int i = 0; i < ctx.Battle.AllUnits.Count; i++)
            {
                var unit = ctx.Battle.AllUnits[i];
                if (unit == null) continue;
                if (!unit.IsAlive) continue;
                if (unit.Team != ctx.Actor.Team) continue;
                if (!unit.IsTurret) continue;

                if (unit.Attack > maxAttack)
                    maxAttack = unit.Attack;
            }

            return maxAttack;
        }
    }
}