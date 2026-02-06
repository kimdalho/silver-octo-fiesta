#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace StateTree.Editor
{
    public class StateTreeStepsPanel : VisualElement
    {
        private readonly Func<StateTreeAsset> getAsset;
        private readonly Func<string> getSelectedState;

        private IMGUIContainer imgui;
        private ReorderableList list;

        private SerializedObject so;
        private SerializedProperty statesProp;
        private SerializedProperty stepsProp;

        public StateTreeStepsPanel(Func<StateTreeAsset> getAsset, Func<string> getSelectedState)
        {
            this.getAsset = getAsset;
            this.getSelectedState = getSelectedState;

            style.flexGrow = 1;
            style.paddingLeft = 8;
            style.paddingRight = 8;

            imgui = new IMGUIContainer(Draw);
            imgui.style.flexGrow = 1;
            Add(imgui);
        }

        public void Refresh()
        {
            so = null; list = null; stepsProp = null;

            var asset = getAsset();
            if (asset == null) return;

            so = new SerializedObject(asset);
            statesProp = so.FindProperty("states");

            int idx = FindStateIndex(statesProp, getSelectedState());
            if (idx < 0) return;

            stepsProp = statesProp.GetArrayElementAtIndex(idx).FindPropertyRelative("steps");
            BuildList();
        }

        private void BuildList()
        {
            if (stepsProp == null) return;

            list = new ReorderableList(so, stepsProp, true, true, true, true);
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Sequence Steps (drag to reorder)");
            list.elementHeight = EditorGUIUtility.singleLineHeight * 6.2f;

            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var el = stepsProp.GetArrayElementAtIndex(index);

                var typeProp = el.FindPropertyRelative("type");
                var strProp = el.FindPropertyRelative("str");
                var f1Prop = el.FindPropertyRelative("f1");
                var f2Prop = el.FindPropertyRelative("f2");
                var b1Prop = el.FindPropertyRelative("b1");
                var noteProp = el.FindPropertyRelative("note");

                rect.y += 2;

                var r0 = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(r0, typeProp);

                var t = (StepType)typeProp.enumValueIndex;

                Rect R(int line) => new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * (1.2f * line), rect.width, EditorGUIUtility.singleLineHeight);

                switch (t)
                {
                    case StepType.AnimSetInt:
                        EditorGUI.PropertyField(R(1), strProp, new GUIContent("Param (int)"));
                        EditorGUI.PropertyField(R(2), f1Prop, new GUIContent("Value"));
                        EditorGUI.PropertyField(R(3), noteProp, new GUIContent("Note"));
                        break;

                    case StepType.AnimSetBool:
                        EditorGUI.PropertyField(R(1), strProp, new GUIContent("Param (bool)"));
                        EditorGUI.PropertyField(R(2), b1Prop, new GUIContent("Value"));
                        EditorGUI.PropertyField(R(3), noteProp, new GUIContent("Note"));
                        break;

                    case StepType.AnimTrigger:
                        EditorGUI.PropertyField(R(1), strProp, new GUIContent("Trigger"));
                        EditorGUI.PropertyField(R(2), noteProp, new GUIContent("Note"));
                        break;

                    case StepType.Wait:
                        EditorGUI.PropertyField(R(1), f1Prop, new GUIContent("Seconds"));
                        EditorGUI.PropertyField(R(2), noteProp, new GUIContent("Note"));
                        break;

                    case StepType.MoveToTarget:
                        EditorGUI.PropertyField(R(1), f1Prop, new GUIContent("Range"));
                        EditorGUI.PropertyField(R(2), f2Prop, new GUIContent("MaxTime (0=inf)"));
                        EditorGUI.PropertyField(R(3), noteProp, new GUIContent("Note"));
                        break;

                    case StepType.TurnToTarget:
                        EditorGUI.PropertyField(R(1), f1Prop, new GUIContent("MaxDegPerSec (0=use default)"));
                        EditorGUI.PropertyField(R(2), f2Prop, new GUIContent("MaxTime (0=inf)"));
                        EditorGUI.PropertyField(R(3), noteProp, new GUIContent("Note"));
                        break;

                    case StepType.AttackToken:
                        EditorGUI.PropertyField(R(1), strProp, new GUIContent("Attack Token"));
                        EditorGUI.PropertyField(R(2), noteProp, new GUIContent("Note"));
                        break;

                    default:
                        EditorGUI.PropertyField(R(1), noteProp, new GUIContent("Note"));
                        break;
                }
            };

            list.onAddCallback = l =>
            {
                stepsProp.arraySize++;
                so.ApplyModifiedProperties();
            };
        }

        private void Draw()
        {
            var asset = getAsset();
            var state = getSelectedState();

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("StateTree", EditorStyles.boldLabel);

            if (asset == null)
            {
                EditorGUILayout.HelpBox("No asset loaded. Use Load Selected/Load in the toolbar.", MessageType.Info);
                return;
            }

            if (string.IsNullOrEmpty(state))
            {
                EditorGUILayout.HelpBox("Select a state node on the left.", MessageType.Info);
                return;
            }

            if (so == null || stepsProp == null) Refresh();
            if (so == null || stepsProp == null)
            {
                EditorGUILayout.HelpBox("Selected state not found in asset.", MessageType.Warning);
                return;
            }

            so.Update();

            // rename state
            int idx = FindStateIndex(so.FindProperty("states"), state);
            if (idx >= 0)
            {
                var stateProp = so.FindProperty("states").GetArrayElementAtIndex(idx);
                var nameProp = stateProp.FindPropertyRelative("stateName");
                EditorGUILayout.PropertyField(nameProp, new GUIContent("State Name"));
            }

            EditorGUILayout.Space(4);
            list?.DoLayoutList();
            so.ApplyModifiedProperties();
        }

        private int FindStateIndex(SerializedProperty states, string stateName)
        {
            if (states == null || string.IsNullOrEmpty(stateName)) return -1;
            for (int i = 0; i < states.arraySize; i++)
            {
                var el = states.GetArrayElementAtIndex(i);
                var nameProp = el.FindPropertyRelative("stateName");
                if (nameProp != null && string.Equals(nameProp.stringValue, stateName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }
    }
}
#endif
