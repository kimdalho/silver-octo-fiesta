#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualFSM.Editor
{
    public class VisualFSMEditorWindow : EditorWindow
    {
        private VisualFSMGraphView graphView;
        private VisualFSM.VisualFSMAsset asset;

        [MenuItem("Tools/VisualFSM/Editor")]
        public static void Open()
        {
            var wnd = GetWindow<VisualFSMEditorWindow>();
            wnd.titleContent = new GUIContent("VisualFSM");
            wnd.Show();
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            // 1) IMGUI Toolbar (always visible)
            var toolbar = new IMGUIContainer(() =>
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                asset = (VisualFSM.VisualFSMAsset)EditorGUILayout.ObjectField(
                    asset,
                    typeof(VisualFSM.VisualFSMAsset),
                    false,
                    GUILayout.Width(260)
                );

                if (GUILayout.Button("Load Selected", EditorStyles.toolbarButton, GUILayout.Width(100)))
                {
                    var objs = Selection.GetFiltered(typeof(VisualFSM.VisualFSMAsset), SelectionMode.Assets);
                    asset = (objs != null && objs.Length > 0) ? (VisualFSM.VisualFSMAsset)objs[0] : null;
                    graphView?.Load(asset);
                }

                if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    graphView?.Load(asset);
                }

                if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    if (asset != null && graphView != null)
                    {
                        graphView.Save(asset);
                        EditorUtility.SetDirty(asset);
                        AssetDatabase.SaveAssets();
                    }
                }

                GUILayout.FlexibleSpace();

                GUILayout.Label("Loaded:", EditorStyles.miniLabel);
                GUILayout.Label(asset ? asset.name : "(none)", EditorStyles.miniLabel);

                EditorGUILayout.EndHorizontal();
            });

            toolbar.style.flexShrink = 0; // <- 툴바가 눌려서 사라지는 것 방지
            rootVisualElement.Add(toolbar);

            // 2) Graph container
            var graphContainer = new VisualElement();
            graphContainer.style.flexGrow = 1;
            graphContainer.style.flexBasis = 0;
            rootVisualElement.Add(graphContainer);

            graphView = new VisualFSMGraphView(this);
            graphView.style.flexGrow = 1;
            graphContainer.Add(graphView);

            // auto-load selected asset if any
            if (asset == null)
            {
                var objs = Selection.GetFiltered(typeof(VisualFSM.VisualFSMAsset), SelectionMode.Assets);
                asset = (objs != null && objs.Length > 0) ? (VisualFSM.VisualFSMAsset)objs[0] : null;
            }
            graphView.Load(asset);
        }
    }
}
#endif
