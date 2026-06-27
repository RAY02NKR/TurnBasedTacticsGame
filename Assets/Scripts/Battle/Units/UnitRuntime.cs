// Assets/Scripts/Battle/Units/UnitRuntime.cs
using System;
using System.Collections.Generic;
using TurnBasedGame.Battle.Core;
using TurnBasedGame.Battle.Grid;
using TurnBasedGame.Battle.Skills;
using TurnBasedGame.Battle.StatusEffects;
using TurnBasedGame.Battle.StatusEffects.Actions;

namespace TurnBasedGame.Battle.Units
{
    public sealed class UnitRuntime
    {
        public string Name { get; }
        public Team Team { get; }
        private static int _nextRuntimeId = 1;
        public int RuntimeId { get; }
        public int MaxHP { get; }
        private readonly int _baseAttack;
        private readonly int _baseSpeed;
        private readonly int _baseStep;
        private readonly float _baseResistSlash;
        private readonly float _baseResistPierce;
        private readonly float _baseResistBlunt;

        private readonly List<StatusEffectInstance> _statuses = new();
        public IReadOnlyList<StatusEffectInstance> Statuses => _statuses;

        private readonly List<PendingStatusEffectInstance> _pendingStatuses = new();
        public IReadOnlyList<PendingStatusEffectInstance> PendingStatuses => _pendingStatuses;

        public int Attack => _baseAttack + GetStatAdd(StatType.Attack);
        public int Speed => _baseSpeed + GetStatAdd(StatType.Speed);
        public int Step => _baseStep + GetStatAdd(StatType.Step);

        public SkillDefinition BasicAttack { get; }
        public SkillDefinition[] ActiveSkills { get; }
        private readonly int[] _cooldownRemaining;

        public int CurrentHP { get; private set; }
        public bool IsAlive => CurrentHP > 0;
        public event Action<UnitRuntime, int, int, int> Damaged;
        public bool IsTurret { get; }
        public UnitGridState Grid { get; }
        public bool IsPlaced => Grid.IsPlaced;
        public GridPosition Position => Grid.Position;

        public UnitRuntime(
            string name, Team team, int maxHP, int attack, int speed, int step, SkillDefinition basicAttack, SkillDefinition[] activeSkills, float resistSlash, float resistPierce, float resistBlunt, bool isTurret = false)
        {
            Name = name;
            Team = team;
            RuntimeId = _nextRuntimeId++;
            MaxHP = maxHP;
            _baseAttack = attack;
            _baseSpeed = speed;
            _baseStep = step;
            _baseResistSlash = resistSlash;
            _baseResistPierce = resistPierce;
            _baseResistBlunt = resistBlunt;
            BasicAttack = basicAttack;
            ActiveSkills = activeSkills ?? new SkillDefinition[3];
            _cooldownRemaining = new int[ActiveSkills.Length];
            CurrentHP = maxHP;
            IsTurret = isTurret;

            Grid = new UnitGridState();
        }

        public void OnTurnStart()
        {
            var buffer = new StatusEffectApplyBuffer();

            StatusService.ExecuteTurnStartEffects(this, buffer);
            StatusService.FlushPendingApplies(this, buffer);

            for (int i = _pendingStatuses.Count - 1; i >= 0; i--)
            {
                var p = _pendingStatuses[i];
                p.DelayTurns--;

                if (p.DelayTurns <= 0)
                {
                    AddOrRefreshStatus(p.Def, p.Def.durationTurns, p.Stacks);
                    _pendingStatuses.RemoveAt(i);
                }
            }
        }

        public void OnTurnEnd()
        {
            var buffer = new StatusEffectApplyBuffer();

            StatusService.ExecuteTurnEndEffects(this, buffer);

            for (int i = 0; i < _cooldownRemaining.Length; i++)
                if (_cooldownRemaining[i] > 0) _cooldownRemaining[i]--;

            for (int i = _statuses.Count - 1; i >= 0; i--)
            {
                var s = _statuses[i];
                if (s.RemainingTurns < 0) continue;
                s.RemainingTurns--;
                if (s.RemainingTurns <= 0)
                    _statuses.RemoveAt(i);
            }

            StatusService.FlushPendingApplies(this, buffer);
        }

        public List<StatusDisplayInfo> GetStatusDisplayInfos()
        {
            var list = new List<StatusDisplayInfo>(_statuses.Count);

            for (int i = 0; i < _statuses.Count; i++)
            {
                var s = _statuses[i];
                string name = s.Def != null ? s.Def.effectName : "Unknown";
                list.Add(new StatusDisplayInfo(name, s.Stacks, s.RemainingTurns));
            }

            return list;
        }

        public List<PendingStatusDisplayInfo> GetPendingStatusDisplayInfos()
        {
            var list = new List<PendingStatusDisplayInfo>(_pendingStatuses.Count);

            for (int i = 0; i < _pendingStatuses.Count; i++)
            {
                var p = _pendingStatuses[i];
                string name = p.Def != null ? p.Def.effectName : "Unknown";
                list.Add(new PendingStatusDisplayInfo(name, p.Stacks, p.DelayTurns));
            }

            return list;
        }

        public bool CanUseSkill(SkillDefinition skill)
        {
            if (skill == null) return false;

            if (skill.requiredStatus != null)
            {
                if (GetStatusStacks(skill.requiredStatus) < skill.requiredMinStacks)
                    return false;
            }

            return true;
        }

        public bool CanUseActiveSkill(int slot)
        {
            if (slot < 0 || slot >= ActiveSkills.Length) return false;

            var skill = ActiveSkills[slot];
            if (skill == null) return false;

            if (_cooldownRemaining[slot] > 0) return false;

            return CanUseSkill(skill);
        }

        public void PutActiveSkillOnCooldown(int slot)
        {
            var def = ActiveSkills[slot];
            if (def == null) return;
            _cooldownRemaining[slot] = def.cooldownTurns;
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            if (amount < 0) amount = 0;

            int before = CurrentHP;
            CurrentHP = Math.Max(0, CurrentHP - amount);
            int actualDamage = before - CurrentHP;

            if (actualDamage > 0)
                Damaged?.Invoke(this, actualDamage, before, CurrentHP);
        }

        public void PlaceAt(GridPosition position)
        {
            Grid.PlaceAt(position);
        }

        public void MoveTo(GridPosition position)
        {
            Grid.MoveTo(position);
        }

        public void RemoveFromGrid()
        {
            Grid.RemoveFromGrid();
        }

        public override string ToString()
            => $"{Name}({Team}) HP:{CurrentHP}/{MaxHP} ATK:{Attack} SPD:{Speed} STEP:{Step} POS:{(IsPlaced ? Position.ToString() : "Unplaced")}";

        public void AddOrRefreshStatus(StatusEffectDefinition def, int durationTurns, int stacksToAdd)
        {
            if (def == null) return;

            durationTurns = NormalizeDurationTurns(durationTurns);

            if (stacksToAdd < 1) stacksToAdd = 1;

            for (int i = 0; i < _statuses.Count; i++)
            {
                var inst = _statuses[i];
                if (inst.Def != def) continue;

                inst.RemainingTurns = MergeDurationTurns(inst.RemainingTurns, durationTurns);

                if (def.stackable)
                    inst.Stacks = Math.Min(def.maxStacks, inst.Stacks + stacksToAdd);
                else
                    inst.Stacks = 1;

                return;
            }

            int stacks = def.stackable ? Math.Min(def.maxStacks, stacksToAdd) : 1;
            _statuses.Add(new StatusEffectInstance(def, durationTurns, stacks));
        }

        public StatusEffectInstance FindStatusByFamily(StatusFamily family)
        {
            for (int i = 0; i < _statuses.Count; i++)
            {
                var inst = _statuses[i];
                if (inst.Def == null) continue;
                if (inst.Def.family != family) continue;

                return inst;
            }

            return null;
        }

        public bool RemoveStatusByFamily(StatusFamily family)
        {
            for (int i = _statuses.Count - 1; i >= 0; i--)
            {
                var inst = _statuses[i];
                if (inst.Def == null) continue;
                if (inst.Def.family != family) continue;

                _statuses.RemoveAt(i);
                return true;
            }

            return false;
        }

        public int GetStatusStacksByFamily(StatusFamily family)
        {
            var inst = FindStatusByFamily(family);
            return inst != null ? inst.Stacks : 0;
        }

        public void ReserveStatus(StatusEffectDefinition def, int delayTurns, int stacksToAdd = 1)
        {
            if (def == null) return;

            if (delayTurns < 1) delayTurns = 1;
            if (stacksToAdd < 1) stacksToAdd = 1;

            for (int i = 0; i < _pendingStatuses.Count; i++)
            {
                var inst = _pendingStatuses[i];
                if (inst.Def != def) continue;
                if (inst.DelayTurns != delayTurns) continue;

                if (def.stackable)
                    inst.Stacks = Math.Min(def.maxStacks, inst.Stacks + stacksToAdd);
                else
                    inst.Stacks = 1;

                return;
            }

            int stacks = def.stackable ? Math.Min(def.maxStacks, stacksToAdd) : 1;
            _pendingStatuses.Add(new PendingStatusEffectInstance(def, delayTurns, stacks));
        }

        private int NormalizeDurationTurns(int durationTurns)
        {
            if (durationTurns == -1) return -1;
            if (durationTurns <= 0) return 1;
            return durationTurns;
        }

        private int MergeDurationTurns(int currentTurns, int newTurns)
        {
            if (currentTurns == -1 || newTurns == -1) return -1;
            return newTurns;
        }

        private int GetStatAdd(StatType stat)
        {
            int sum = 0;
            for (int i = 0; i < _statuses.Count; i++)
            {
                var inst = _statuses[i];
                var mods = inst.Def.modifiers;
                for (int m = 0; m < mods.Count; m++)
                {
                    if (mods[m].stat == stat)
                        sum += mods[m].add * inst.Stacks;
                }
            }
            return sum;
        }

        public float GetResistanceAdd(DamageType type)
        {
            float sum = type switch
            {
                DamageType.Slash => _baseResistSlash,
                DamageType.Pierce => _baseResistPierce,
                DamageType.Blunt => _baseResistBlunt,
                _ => 0f
            };

            for (int i = 0; i < _statuses.Count; i++)
            {
                var inst = _statuses[i];
                var mods = inst.Def.resistanceModifiers;
                for (int m = 0; m < mods.Count; m++)
                {
                    if (mods[m].type == type)
                        sum += mods[m].add * inst.Stacks;
                }
            }

            return sum;
        }

        public int GetStatusStacks(StatusEffectDefinition def)
        {
            if (def == null) return 0;

            for (int i = 0; i < _statuses.Count; i++)
            {
                if (_statuses[i].Def == def)
                    return _statuses[i].Stacks;
            }

            return 0;
        }

        public int ConsumeStatusStacks(StatusEffectDefinition def, int amount)
        {
            if (def == null || amount <= 0) return 0;

            for (int i = 0; i < _statuses.Count; i++)
            {
                var inst = _statuses[i];
                if (inst.Def != def) continue;

                int consumed = Math.Min(inst.Stacks, amount);
                inst.Stacks -= consumed;

                if (inst.Stacks <= 0)
                    _statuses.RemoveAt(i);

                return consumed;
            }

            return 0;
        }

        public bool CanUseElectromagneticCannon(SkillDefinition skill, BattleContext battle)
        {
            if (!CanUseSkill(skill)) return false;
            if (battle == null) return false;

            for (int i = 0; i < battle.AllUnits.Count; i++)
            {
                var unit = battle.AllUnits[i];
                if (!unit.IsAlive) continue;
                if (unit.Team != Team) continue;
                if (!unit.IsTurret) continue;
                return true;
            }

            return false;
        }
    }
}