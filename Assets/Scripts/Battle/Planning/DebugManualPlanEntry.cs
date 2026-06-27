using System;
using System.Collections.Generic;
using UnityEngine;
using TurnBasedGame.Battle.Grid;

namespace TurnBasedGame.Battle.Planning
{
    public enum DebugManualTargetGroup
    {
        Enemy,
        Ally
    }

    [Serializable]
    public sealed class DebugManualPlanEntry
    {
        public int planningTurn = 1;
        public int allyIndex;
        public bool enabled = true;

        [Header("Move")]
        public List<GridPosition> movePath = new();

        [Header("Skill")]
        public bool useSkill = false;
        public bool useBasicAttack = true;
        public int activeSkillSlot = 0;

        [Header("Target Unit")]
        public bool useTargetUnit = false;
        public DebugManualTargetGroup targetGroup = DebugManualTargetGroup.Enemy;
        public int targetUnitIndex = 0;

        [Header("Target Position")]
        public bool useTargetPosition = false;
        public GridPosition targetPosition;
    }
}