using System.Collections.Generic;
using TurnBasedGame.Battle.StatusEffects.Actions;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.StatusEffects
{
    public static class StatusService
    {
        public static void Apply(UnitRuntime target, StatusEffectDefinition def, int stacksToAdd = 1)
        {
            if (target == null || def == null) return;
            if (stacksToAdd < 1) stacksToAdd = 1;

            var applyDef = ResolveAppliedDefinition(target, def);
            target.AddOrRefreshStatus(applyDef, applyDef.durationTurns, stacksToAdd);
        }

        private static StatusEffectDefinition ResolveAppliedDefinition(UnitRuntime target, StatusEffectDefinition incomingDef)
        {
            if (target == null || incomingDef == null) return incomingDef;
            if (incomingDef.family == StatusFamily.None) return incomingDef;

            var existing = target.FindStatusByFamily(incomingDef.family);
            if (existing == null) return incomingDef;
            if (existing.Def == null) return incomingDef;
            if (!existing.Def.keepCurrentVariantWhenApplied) return incomingDef;

            return existing.Def;
        }

        private static int NormalizeDurationTurns(int durationTurns)
        {
            if (durationTurns == -1) return -1; // 永続
            if (durationTurns <= 0) return 1;   // 0や不正値は1に補正
            return durationTurns;

        }

        public static void ExecuteTurnEndEffects(UnitRuntime target, StatusEffectApplyBuffer buffer)
        {
            if (target == null) return;

            var snapshot = new List<StatusEffectInstance>(target.Statuses);
            for (int i = 0; i < snapshot.Count; i++)
            {
                var inst = snapshot[i];
                ExecuteEffects(target, inst, inst.Def.onTurnEndEffects, buffer);
            }
        }

        public static void ExecuteTurnStartEffects(UnitRuntime target, StatusEffectApplyBuffer buffer)
        {
            if (target == null) return;

            var snapshot = new List<StatusEffectInstance>(target.Statuses);
            for (int i = 0; i < snapshot.Count; i++)
            {
                var inst = snapshot[i];
                ExecuteEffects(target, inst, inst.Def.onTurnStartEffects, buffer);
            }
        }

        public static void FlushPendingApplies(UnitRuntime target, StatusEffectApplyBuffer buffer)
        {
            if (target == null || buffer == null) return;

            for (int i = 0; i < buffer.Pending.Count; i++)
            {
                var p = buffer.Pending[i];
                Apply(target, p.Def, p.StacksToAdd);
            }
        }

        public static void ExecuteDamagedEffects(
    UnitRuntime target,
    UnitRuntime sourceUnit,
    int receivedDamage,
    bool isSkillDamage)
        {
            if (target == null) return;
            if (receivedDamage <= 0) return;

            var buffer = new StatusEffectApplyBuffer();
            var snapshot = new List<StatusEffectInstance>(target.Statuses);

            for (int i = 0; i < snapshot.Count; i++)
            {
                var inst = snapshot[i];
                ExecuteDamagedEffectList(
                    target,
                    inst,
                    inst.Def.onDamagedEffects,
                    sourceUnit,
                    receivedDamage,
                    isSkillDamage,
                    buffer);
            }

            FlushPendingApplies(target, buffer);
        }

        private static void ExecuteDamagedEffectList(
            UnitRuntime owner,
            StatusEffectInstance inst,
            List<StatusEffectActionDefinition> effects,
            UnitRuntime sourceUnit,
            int receivedDamage,
            bool isSkillDamage,
            StatusEffectApplyBuffer buffer)
        {
            if (effects == null) return;

            var ctx = new StatusEffectActionContext(
                owner,
                inst,
                sourceUnit: sourceUnit,
                receivedDamage: receivedDamage,
                isSkillDamage: isSkillDamage);

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == null) continue;
                effects[i].Execute(ctx, buffer);
            }
        }

        public static void ExecuteMovedEffects(UnitRuntime target, int movedSteps)
        {
            if (target == null) return;
            if (movedSteps <= 0) return;

            var buffer = new StatusEffects.Actions.StatusEffectApplyBuffer();
            var snapshot = new List<StatusEffectInstance>(target.Statuses);

            for (int i = 0; i < snapshot.Count; i++)
            {
                var inst = snapshot[i];
                ExecuteMovedEffectList(target, inst, inst.Def.onMovedEffects, movedSteps, buffer);
            }

            FlushPendingApplies(target, buffer);
        }

        private static void ExecuteMovedEffectList(
            UnitRuntime owner,
            StatusEffectInstance inst,
            List<StatusEffects.Actions.StatusEffectActionDefinition> effects,
            int movedSteps,
            StatusEffects.Actions.StatusEffectApplyBuffer buffer)
        {
            if (effects == null) return;

            var ctx = new StatusEffects.Actions.StatusEffectActionContext(
                owner,
                inst,
                movedSteps: movedSteps);

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == null) continue;
                effects[i].Execute(ctx, buffer);
            }
        }

        private static void ExecuteEffects(
            UnitRuntime owner,
            StatusEffectInstance inst,
            List<StatusEffectActionDefinition> effects,
            StatusEffectApplyBuffer buffer)
        {
            if (effects == null) return;

            var ctx = new StatusEffectActionContext(owner, inst);
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == null) continue;
                effects[i].Execute(ctx, buffer);
            }
        }
    }
}