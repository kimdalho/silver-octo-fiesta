using System;
using System.Collections.Generic;
using UnityEngine;

namespace StateTree
{
    public enum StepType
    {
        // Presentation(연출)
        AnimSetInt,
        AnimSetBool,
        AnimTrigger,

        // Simple movement (테스트용, 나중에 NavMesh로 교체 가능)
        MoveToTarget,
        TurnToTarget,

        // Timing
        Wait,

        // Combat hook
        AttackToken,
    }

    [CreateAssetMenu(menuName = "StateTree/State Tree Asset", fileName = "StateTreeAsset")]
    public class StateTreeAsset : ScriptableObject
    {
        public string defaultState = "Idle";
        public List<StateDef> states = new();

        public StateDef FindState(string stateName)
        {
            if (string.IsNullOrWhiteSpace(stateName)) return null;
            for (int i = 0; i < states.Count; i++)
            {
                if (string.Equals(states[i].stateName, stateName, StringComparison.OrdinalIgnoreCase))
                    return states[i];
            }
            return null;
        }

#if UNITY_EDITOR
        [Serializable] public struct StateNodePos { public string key; public Vector2 pos; }
        public List<StateNodePos> editorPositions = new();

        public bool TryGetPos(string key, out Vector2 pos)
        {
            for (int i = 0; i < editorPositions.Count; i++)
            {
                if (string.Equals(editorPositions[i].key, key, StringComparison.OrdinalIgnoreCase))
                {
                    pos = editorPositions[i].pos;
                    return true;
                }
            }
            pos = default;
            return false;
        }

        public void SetPos(string key, Vector2 pos)
        {
            for (int i = 0; i < editorPositions.Count; i++)
            {
                if (string.Equals(editorPositions[i].key, key, StringComparison.OrdinalIgnoreCase))
                {
                    editorPositions[i] = new StateNodePos { key = key, pos = pos };
                    return;
                }
            }
            editorPositions.Add(new StateNodePos { key = key, pos = pos });
        }
#endif
    }

    [Serializable]
    public class StateDef
    {
        public string stateName = "Idle";
        public List<StepDef> steps = new();
    }

    [Serializable]
    public class StepDef
    {
        public StepType type = StepType.Wait;

        // 공용 파라미터(간단/확장 쉬움)
        public string str;  // anim param / trigger / attack token
        public float f1;    // wait seconds / range / maxDegPerSec / int value (float로 저장)
        public float f2;    // maxTime
        public bool b1;     // bool value
        [TextArea] public string note;
    }
}
