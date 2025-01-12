using Godot;
using System;
using System.Collections.Generic;

namespace HLNC.Editor
{
    [Tool]
    public partial class FrameContainer : HBoxContainer
    {
        [Export]
        public WorldDebug debugPanel;

        [Export]
        public CheckBox liveCheckbox;

        private PackedScene tickFrameScene = GD.Load<PackedScene>("res://addons/HLNC/Tools/Debugger/Ticks/tick_frame.tscn");
        private Control selectedTickFrame;
        private Control previousTickFrame;
        private ScrollContainer scrollContainer;
        private Dictionary<int, Control> tickFrames = new Dictionary<int, Control>();
        public override void _Ready() {
            scrollContainer = GetParent<ScrollContainer>();
        }
        public void _OnReceiveFrame(int id)
        {
            var activeTickFrame = tickFrameScene.Instantiate<Control>();
            activeTickFrame.Call("set_frame_size", 0);
            activeTickFrame.Set("tick_frame_id", id);
            if (previousTickFrame != null)
            {
                activeTickFrame.Set("previous_tick_frame", previousTickFrame);
                previousTickFrame.Set("next_tick_frame", activeTickFrame);
            }
            previousTickFrame = activeTickFrame;
            AddChild(activeTickFrame);
            if (liveCheckbox.ButtonPressed)
            {
                OnTickFrameSelected(activeTickFrame, false);
            }
            tickFrames[id] = activeTickFrame;
        }

        public void _OnFrameUpdated(int id) {
            var tickFrame = tickFrames[id];
            var frameData = debugPanel.GetFrameData(id);
            tickFrame.Call("set_frame_size", frameData["details"].AsGodotDictionary()["Tick"].AsGodotDictionary()["Greatest Size"].AsInt32());
            tickFrame.GetNode<Label>("LogCount").Text = frameData["logs"].AsGodotArray().Count.ToString();
        }

        
        private async void ScrollTo(Control tickFrame)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            var framePosition = tickFrame.GetRect().Position.X;
            if (framePosition + tickFrame.GetRect().Size.X > scrollContainer.GetHScrollBar().Value + scrollContainer.GetRect().Size.X)
            {
                scrollContainer.GetHScrollBar().Value = framePosition + tickFrame.GetRect().Size.X - scrollContainer.GetRect().Size.X;
            }
            else if (framePosition < scrollContainer.GetHScrollBar().Value)
            {
                scrollContainer.GetHScrollBar().Value = framePosition;
            }
        }

        private void OnTickFrameSelected(Control tickFrame, bool pause = false) {
            if (selectedTickFrame != null)
            {
                selectedTickFrame.Call("deselect");
            }
            selectedTickFrame = tickFrame;
            selectedTickFrame.Call("select");
            ScrollTo(selectedTickFrame);
            debugPanel.EmitSignal(WorldDebug.SignalName.TickFrameSelected, tickFrame);
            liveCheckbox.ButtonPressed = !pause;
        }
    }
}