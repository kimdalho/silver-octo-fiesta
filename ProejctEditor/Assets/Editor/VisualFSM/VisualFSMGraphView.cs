#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualFSM.Editor
{
    public class VisualFSMGraphView : GraphView
    {
        private readonly VisualFSMEditorWindow window;
        private VisualFSM.VisualFSMAsset asset;

        public VisualFSMGraphView(VisualFSMEditorWindow window)
        {
            this.window = window;

            style.flexGrow = 1;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            Insert(0, new GridBackground());

            // Right-click menu
            nodeCreationRequest = ctx =>
            {
                var pos = this.ChangeCoordinatesTo(contentViewContainer, ctx.screenMousePosition - window.position.position);
                CreateStateNode("NewState", pos);
            };
        }

        public void Load(VisualFSM.VisualFSMAsset asset)
        {
            this.asset = asset;
            DeleteElements(graphElements.ToList());

            if (asset == null) return;

            // Load nodes from asset.states
            foreach (var s in asset.states)
            {
                var pos = GetSavedPos(asset, s.logicState, new Vector2(200, 200));
                CreateStateNode(s.logicState, pos, fromAsset: true);
            }
        }

        public void Save(VisualFSM.VisualFSMAsset asset)
        {
            if (asset == null) return;

            // Ensure all nodes exist in asset.states and update positions
            var nodes = new List<VisualFSMStateNode>();
            foreach (var n in this.nodes)
            {
                if (n is VisualFSMStateNode sn) nodes.Add(sn);
            }

            // Rebuild states list based on nodes (preserve existing data where possible)
            var oldMap = new Dictionary<string, VisualFSM.VisualState>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var s in asset.states)
            {
                if (!string.IsNullOrWhiteSpace(s.logicState))
                    oldMap[s.logicState] = s;
            }

            asset.states.Clear();
#if UNITY_EDITOR
            asset.editorNodePositions ??= new Dictionary<string, Vector2>();
            asset.editorNodePositions.Clear();
#endif

            foreach (var node in nodes)
            {
                var key = node.LogicState;
                if (string.IsNullOrWhiteSpace(key)) continue;

                VisualFSM.VisualState state;
                if (!oldMap.TryGetValue(key, out state))
                {
                    state = new VisualFSM.VisualState { logicState = key };
                }
                else
                {
                    state.logicState = key;
                }

                // Copy edited fields from node inspector into state
                node.ApplyTo(state);

                asset.states.Add(state);

#if UNITY_EDITOR
                asset.editorNodePositions[key] = node.GetPosition().position;
#endif
            }
        }

        private Vector2 GetSavedPos(VisualFSM.VisualFSMAsset asset, string key, Vector2 fallback)
        {
#if UNITY_EDITOR
            if (asset.editorNodePositions != null &&
                !string.IsNullOrWhiteSpace(key) &&
                asset.editorNodePositions.TryGetValue(key, out var p))
                return p;
#endif
            return fallback;
        }

        private void CreateStateNode(string logicState, Vector2 position, bool fromAsset = false)
        {
            var node = new VisualFSMStateNode(logicState, asset, fromAsset);
            node.SetPosition(new Rect(position, new Vector2(260, 240)));
            AddElement(node);
        }
    }
}
#endif
