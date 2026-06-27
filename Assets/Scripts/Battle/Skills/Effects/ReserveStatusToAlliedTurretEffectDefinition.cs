using System.Linq;
using UnityEngine;
using TurnBasedGame.Battle.StatusEffects;

namespace TurnBasedGame.Battle.Skills.Effects
{
    [CreateAssetMenu(
        fileName = "ReserveStatusToAlliedTurretsEffect",
        menuName = "Battle/Skills/Effects/ReserveStatusToAlliedTurrets")]
    public sealed class ReserveStatusToAlliedTurretsEffectDefinition : SkillEffectDefinition
    {
        public StatusEffectDefinition statusToReserve;
        [Min(1)] public int delayTurns = 1;
        [Min(1)] public int stacksToAdd = 1;

        public override void Apply(SkillEffectContext ctx)
        {
            if (ctx == null) return;
            if (ctx.Actor == null) return;
            if (ctx.Battle == null) return;
            if (statusToReserve == null) return;

            var turrets = ctx.Battle.AllUnits
                .Where(u => u.IsAlive && u.Team == ctx.Actor.Team && u.IsTurret)
                .ToList();

            for (int i = 0; i < turrets.Count; i++)
            {
                turrets[i].ReserveStatus(statusToReserve, delayTurns, stacksToAdd);
            }
        }
    }
}