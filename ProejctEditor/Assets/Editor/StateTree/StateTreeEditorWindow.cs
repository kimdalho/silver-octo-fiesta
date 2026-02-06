#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace StateTree.Editor
{
    public class StateTreeEditorWindow : EditorWindow
    {
        private StateTreeGraphView graphView;
        private StateTreeStepsPanel stepsPanel;

        private StateTreeAsset asset;
        private string selectedState;

        [MenuItem("Tools/StateTree/Editor")]
        public static void Open()
        {
            var wnd = GetWindow<StateTreeEditorWindow>();
            wnd.titleContent = new GUIContent("StateTree");
            wnd.Show();
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            var toolbar = new IMGUIContainer(() =>
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                asset = (StateTreeAsset)EditorGUILayout.ObjectField(asset, typeof(StateTreeAsset), false, GUILayout.Width(280));

                if (GUILayout.Button("Load Selected", EditorStyles.toolbarButton, GUILayout.Width(100)))
                {
                    var objs = Selection.GetFiltered(typeof(StateTreeAsset), SelectionMode.Assets);
                    asset = (objs != null && objs.Length > 0) ? (StateTreeAsset)objs[0] : null;
                    LoadAsset();
                }

                if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    LoadAsset();

                if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    SaveAsset();

                GUILayout.FlexibleSpace();
                GUILayout.Label("Selected:", EditorStyles.miniLabel);
                GUILayout.Label(string.IsNullOrEmpty(selectedState) ? "(none)" : selectedState, EditorStyles.miniLabel);

                EditorGUILayout.EndHorizontal();
            });
            toolbar.style.flexShrink = 0;
            rootVisualElement.Add(toolbar);

            var split = new TwoPaneSplitView(0, 520, TwoPaneSplitViewOrientation.Horizontal);
            split.style.flexGrow = 1;
            rootVisualElement.Add(split);

            graphView = new StateTreeGraphView(this);
            graphView.style.flexGrow = 1;
            split.Add(graphView);

            stepsPanel = new StateTreeStepsPanel(() => asset, () => selectedState);
            split.Add(stepsPanel);

            // auto load selected asset
            var sel = Selection.GetFiltered(typeof(StateTreeAsset), SelectionMode.Assets);
            if (asset == null && sel != null && sel.Length > 0) asset = (StateTreeAsset)sel[0];
            LoadAsset();
        }

        private void LoadAsset()
        {
            selectedState = null;
            graphView?.Load(asset);
            stepsPanel?.Refresh();
        }

        private void SaveAsset()
        {
            if (asset == null) return;
            graphView?.SaveNodePositions(asset);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        public void SelectState(string stateName)
        {
            selectedState = stateName;
            stepsPanel?.Refresh();
            Repaint();
        }
    }
}
#endif
