#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace VisualFSM.Editor
{
    public class VisualFSMStateNode : Node
    {
        public string LogicState => logicStateField.value;

        private TextField logicStateField;

        // editable fields
        private TextField intParamField;
        private IntegerField intValueField;
        private TextField triggerField;
        private TextField boolParamField;
        private Toggle boolValueToggle;
        private ObjectField vfxField;
        private Toggle parentVfxToggle;
        private Vector3Field vfxScaleField;
        private ObjectField sfxField;
        private TextField noteField;

        private readonly VisualFSM.VisualFSMAsset asset;

        public VisualFSMStateNode(string logicState, VisualFSM.VisualFSMAsset asset, bool fromAsset)
        {
            this.asset = asset;

            title = "Visual State";

            logicStateField = new TextField("LogicState") { value = logicState };
            mainContainer.Add(logicStateField);

            intParamField = new TextField("Anim Int Param");
            intValueField = new IntegerField("Int Value");

            triggerField = new TextField("Anim Trigger");

            boolParamField = new TextField("Anim Bool Param");
            boolValueToggle = new Toggle("Bool Value");

            vfxField = new ObjectField("Enter VFX") { objectType = typeof(GameObject), allowSceneObjects = false };
            parentVfxToggle = new Toggle("Parent VFX") { value = true };
            vfxScaleField = new Vector3Field("VFX Scale") { value = Vector3.one };

            sfxField = new ObjectField("Enter SFX") { objectType = typeof(AudioClip), allowSceneObjects = false };

            noteField = new TextField("Note") { multiline = true };

            extensionContainer.Add(intParamField);
            extensionContainer.Add(intValueField);
            extensionContainer.Add(triggerField);
            extensionContainer.Add(boolParamField);
            extensionContainer.Add(boolValueToggle);
            extensionContainer.Add(vfxField);
            extensionContainer.Add(parentVfxToggle);
            extensionContainer.Add(vfxScaleField);
            extensionContainer.Add(sfxField);
            extensionContainer.Add(noteField);

            RefreshExpandedState();
            RefreshPorts();

            // If loading from asset, pull existing values
            if (asset != null && fromAsset)
            {
                var s = asset.FindByLogicState(logicState);
                if (s != null)
                {
                    intParamField.value = s.animatorIntParam;
                    intValueField.value = s.animatorIntValue;
                    triggerField.value = s.animatorTrigger;
                    boolParamField.value = s.animatorBoolParam;
                    boolValueToggle.value = s.animatorBoolValue;
                    vfxField.value = s.enterVfxPrefab;
                    parentVfxToggle.value = s.parentVfxToOwner;
                    vfxScaleField.value = s.vfxLocalScale;
                    sfxField.value = s.enterSfx;
                    noteField.value = s.note;
                }
            }
        }

        public void ApplyTo(VisualFSM.VisualState s)
        {
            s.logicState = logicStateField.value?.Trim() ?? "Idle";
            s.animatorIntParam = intParamField.value?.Trim() ?? "";
            s.animatorIntValue = intValueField.value;

            s.animatorTrigger = triggerField.value?.Trim() ?? "";

            s.animatorBoolParam = boolParamField.value?.Trim() ?? "";
            s.animatorBoolValue = boolValueToggle.value;

            s.enterVfxPrefab = vfxField.value as GameObject;
            s.parentVfxToOwner = parentVfxToggle.value;
            s.vfxLocalScale = vfxScaleField.value;

            s.enterSfx = sfxField.value as AudioClip;

            s.note = noteField.value ?? "";
        }
    }
}
#endif
