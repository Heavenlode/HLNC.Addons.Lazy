using Godot;
using Godot.Collections;
using HLNC.Serialization;
using System;

namespace HLNC.Editor
{
    [Tool]
    public partial class NetworkSceneInspector : EditorInspectorPlugin
    {

        PackedScene inspectorScene = GD.Load<PackedScene>("res://addons/HLNC/Tools/Inspector/inspect_network_scene.tscn");
        private Tree editorSceneTree;
        private Tree GetEditorSceneTree()
        {
            if (editorSceneTree != null)
            {
                return editorSceneTree;
            }
            var baseControl = EditorInterface.Singleton.GetBaseControl();
            var sceneTreeDock = baseControl.FindChildren("Scene", "SceneTreeDock", true, false)[0];
            Control sceneTreeEditor = null;
            foreach (var child in sceneTreeDock.GetChildren())
            {
                if (child.Name.ToString().Contains("SceneTreeEditor"))
                {
                    sceneTreeEditor = child as Control;
                    break;
                }
            }
            if (sceneTreeEditor == null)
            {
                GD.PrintErr("HLNC: No scene tree found");
                return null;
            }
            Tree sceneTree = null;
            foreach (var child in sceneTreeEditor.GetChildren())
            {
                if (child.GetType() == typeof(Tree))
                {
                    sceneTree = child as Tree;
                    break;
                }
            }
            if (sceneTree == null)
            {
                GD.PrintErr("HLNC: No scene tree found");
                return null;
            }
            editorSceneTree = sceneTree;
            return editorSceneTree;
        }

        public override bool _CanHandle(GodotObject obj)
        {
            base._CanHandle(obj);
            var sceneRootItem = GetEditorSceneTree().GetRoot();
            var selectedNodeItem = GetEditorSceneTree().GetSelected();
            if (sceneRootItem == null || selectedNodeItem == null)
            {
                return false;
            }
            var sceneRootNode = GetEditorSceneTree().GetNodeOrNull(sceneRootItem.GetMetadata(0).AsString());
            var sceneSelectedNode = GetEditorSceneTree().GetNodeOrNull(selectedNodeItem.GetMetadata(0).AsString());
            if (sceneRootNode == null || sceneSelectedNode == null)
            {
                return false;
            }
            var relativeNodePath = sceneRootNode.GetPathTo(sceneSelectedNode);
            return NetworkScenesRegister.PackNode(sceneRootNode.SceneFilePath, relativeNodePath, out _);
        }

        public override void _ParseBegin(GodotObject obj)
        {
            base._ParseBegin(obj);
            try
            {
                var inspector = inspectorScene.Instantiate<Control>();
                var sceneRootItem = GetEditorSceneTree().GetRoot();
                var selectedNodeItem = GetEditorSceneTree().GetSelected();
                var sceneRootNode = GetEditorSceneTree().GetNodeOrNull(sceneRootItem.GetMetadata(0).AsString());
                var sceneSelectedNode = GetEditorSceneTree().GetNodeOrNull(selectedNodeItem.GetMetadata(0).AsString());
                if (sceneRootNode == null || sceneSelectedNode == null)
                {
                    return;
                }
                var relativeNodePath = sceneRootNode.GetPathTo(sceneSelectedNode);

                var properties = NetworkScenesRegister.ListProperties(sceneRootNode.SceneFilePath, relativeNodePath);
                foreach (var property in properties)
                {
                    inspector.Call("add_property", property.Name, property.Type.ToString());
                }
                inspector.Call("set_network_type", NetworkScenesRegister.IsNetworkScene(sceneSelectedNode.SceneFilePath) ? "Scene" : "Node");
                AddCustomControl(inspector);
            }
            catch (Exception _)
            {
                return;
            }
        }
    }
}