using System.Collections.Generic;
using UnityEngine;

namespace TurnBasedGame.Battle.Planning
{
    public sealed class DebugManualPlanningProvider : MonoBehaviour
    {
        public static DebugManualPlanningProvider Instance { get; private set; }

        [SerializeField] private bool useManualPlanning = true;
        [SerializeField] private List<DebugManualPlanEntry> entries = new();

        public bool UseManualPlanning => useManualPlanning;
        public IReadOnlyList<DebugManualPlanEntry> Entries => entries;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}