using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualFSM
{
    [CreateAssetMenu(menuName = "VisualFSM/Visual FSM Asset", fileName = "VisualFSMAsset")]
    public class VisualFSMAsset : ScriptableObject
    {
        [Tooltip("State to enter when runner starts (optional).")]
        public string defaultLogicState = "Idle";

        public List<VisualState> states = new();

        public VisualState FindByLogicState(string logicState)
        {
            if (string.IsNullOrWhiteSpace(logicState)) return null;
            for (int i = 0; i < states.Count; i++)
            {
                if (string.Equals(states[i].logicState, logicState, StringComparison.OrdinalIgnoreCase))
                    return states[i];
            }
            return null;
        }

#if UNITY_EDITOR
        // Editor-only: node positions
        public Dictionary<string, Vector2> editorNodePositions = new();
#endif
    }

    [Serializable]
    public class VisualState
    {
        [Tooltip("External logic state name (e.g., Idle/Chase/Attack). Case-insensitive.")]
        public string logicState = "Idle";

        [Header("Animator")]
        [Tooltip("Optional: set this int parameter to a hash/index per state.")]
        public string animatorIntParam = "";
        public int animatorIntValue = 0;

        [Tooltip("Optional: set a trigger on enter.")]
        public string animatorTrigger = "";

        [Tooltip("Optional: set this bool param on enter (and auto reset previous state's bool param if provided).")]
        public string animatorBoolParam = "";
        public bool animatorBoolValue = false;

        [Header("VFX/SFX")]
        [Tooltip("Spawn this prefab on enter (e.g., dust, aura).")]
        public GameObject enterVfxPrefab;

        [Tooltip("If true, VFX is parented to the actor.")]
        public bool parentVfxToOwner = true;

        [Tooltip("Enter SFX (one-shot).")]
        public AudioClip enterSfx;

        [Tooltip("Scale for VFX (optional).")]
        public Vector3 vfxLocalScale = Vector3.one;

        [Header("Notes")]
        [TextArea] public string note;
    }
}
