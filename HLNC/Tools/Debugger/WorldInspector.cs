using Godot;
using HLNC.Editor.DTO;
using System;

namespace HLNC.Editor
{

    [Tool]
    public partial class WorldInspector : Control
    {
        [Export] 
        public WorldDebug debugPanel;

        [Export]
        public VBoxContainer inspectorContainer;

        private PackedScene inspectorTitleScene = GD.Load<PackedScene>("res://addons/HLNC/Tools/Inspector/inspector_title.tscn");
        private PackedScene inspectorFieldScene = GD.Load<PackedScene>("res://addons/HLNC/Tools/Inspector/inspector_field.tscn");

        public void _OnTickFrameSelected(Control tickFrame) {

            foreach (var child in inspectorContainer.GetChildren()) {
                child.QueueFree();
            }

            var frame_id = tickFrame.Get("tick_frame_id").AsInt32();
            var frame_data = debugPanel.GetFrameData(frame_id);

            foreach (var category in frame_data["details"].AsGodotDictionary().Keys) {
                var title = inspectorTitleScene.Instantiate<Control>();
                title.GetNode<Label>("%Label").Text = category.AsString();
                inspectorContainer.AddChild(title);

                foreach (var kvp in frame_data["details"].AsGodotDictionary()[category].AsGodotDictionary()) {
                    var fieldName = kvp.Key.AsString();
                    var fieldValue = kvp.Value.AsString();
                    var field = inspectorFieldScene.Instantiate<Control>();
                    field.GetNode<Label>("%Label").Text = fieldName;
                    field.GetNode<RichTextLabel>("%Value").Text = fieldValue;
                    inspectorContainer.AddChild(field);
                }
            }
        }
    }
}