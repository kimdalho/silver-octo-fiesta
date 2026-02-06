#if UNITY_EDITOR
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace StateTree.Editor
{
    public class StateTreeGraphView : GraphView
    {
        private readonly StateTreeEditorWindow window;
        private StateTreeAsset asset;

        public StateTreeGraphView(StateTreeEditorWindow window)
        {
            this.window = window;

            style.flexGrow = 1;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            Insert(0, new GridBackground());
            focusable = true;

            nodeCreationRequest = ctx =>
            {
                if (asset == null) return;
                var pos = this.ChangeCoordinatesTo(contentViewContainer, ctx.screenMousePosition - window.position.position);
                CreateState(pos);
            };
        }

        public void Load(StateTreeAsset asset)
        {
            this.asset = asset;
            DeleteElements(graphElements.ToList());

            if (asset == null) return;

            foreach (var s in asset.states)
            {
                var node = new StateNode(s.stateName, window);

                Vector2 p = new Vector2(200, 200);
                if (asset.TryGetPos(s.stateName, out var saved)) p = saved;

                node.SetPosition(new Rect(p, new Vector2(240, 120)));
                AddElement(node);
            }
        }

        private void CreateState(Vector2 pos)
        {
            string baseName = "State";
            int n = 1;
            string name = $"{baseName}{n}";
            while (asset.states.Any(x => string.Equals(x.stateName, name, System.StringComparison.OrdinalIgnoreCase)))
            {
                n++;
                name = $"{baseName}{n}";
            }

            asset.states.Add(new StateDef { stateName = name });

            var node = new StateNode(name, window);
            node.SetPosition(new Rect(pos, new Vector2(240, 120)));
            AddElement(node);

            // 선택 처리(이벤트 의존 없음)
            ClearSelection();
            AddToSelection(node);
            window.SelectState(name);
        }

        public void SaveNodePositions(StateTreeAsset asset)
        {
            if (asset == null) return;

            foreach (var n in nodes.ToList())
            {
                if (n is not StateNode sn) continue;
                asset.SetPos(sn.StateName, sn.GetPosition().position);
            }
        }

        private class StateNode : Node
        {
            public string StateName => stateField.value;

            private readonly StateTreeEditorWindow window;
            private TextField stateField;

            public StateNode(string name, StateTreeEditorWindow window)
            {
                this.window = window;

                title = "State";
                stateField = new TextField("Name") { value = name };
                mainContainer.Add(stateField);

                // ✅ 이게 핵심: 노드 클릭하면 선택된 상태 갱신
                RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.button != 0) return; // left click only
                    this.window.SelectState(StateName);
                });

                // 텍스트 변경할 때도 반영
                stateField.RegisterValueChangedCallback(_ =>
                {
                    this.window.SelectState(StateName);
                });
            }
        }
    }
}
#endif
