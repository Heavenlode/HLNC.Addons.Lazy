@tool
extends EditorPlugin

const AUTOLOAD_RUNNER = "NetworkRunner"
const AUTOLOAD_ENV = "Env"

const DockNetworkScenes = preload("res://addons/HLNC/Tools/Dock/NetworkScenes/dock_network_scenes.tscn")
var dock_network_scenes_instance: Control

const ServerDebugClient = preload("res://addons/HLNC/Tools/Debugger/server_debug_client.tscn")
var server_debug_client_instance: Window

const NetworkSceneInspector = preload("res://addons/HLNC/Tools/Inspector/NetworkSceneInspector.cs")

var network_scene_inspector_instance: NetworkSceneInspector

func _get_plugin_name():
    return "HLNC"

func _enter_tree():
    add_autoload_singleton(AUTOLOAD_ENV, "res://addons/HLNC/Utils/Env/Env.cs")
    add_autoload_singleton(AUTOLOAD_RUNNER, "res://addons/HLNC/Core/NetworkRunner.cs")

    var network_transform_3d_icon = EditorInterface.get_editor_theme().get_icon("RemoteTransform3D", "EditorIcons")
    add_custom_type("NetworkTransform3D", "Node", preload("res://addons/HLNC/Core/Nodes/NetworkTransform/NetworkTransform3D.cs"), network_transform_3d_icon)
    var network_transform_2d_icon = EditorInterface.get_editor_theme().get_icon("RemoteTransform2D", "EditorIcons")
    add_custom_type("NetworkTransform2D", "Node", preload("res://addons/HLNC/Core/Nodes/NetworkTransform/NetworkTransform2D.cs"), network_transform_2d_icon)

    var project_settings_controller = ProjectSettingsController.new()
    add_child(project_settings_controller)

    dock_network_scenes_instance = DockNetworkScenes.instantiate()
    dock_network_scenes_instance.name = "Network Scenes"
    add_control_to_dock(DOCK_SLOT_LEFT_UR, dock_network_scenes_instance)

    server_debug_client_instance = ServerDebugClient.instantiate()
    server_debug_client_instance.hide()
    add_child(server_debug_client_instance)

    network_scene_inspector_instance = NetworkSceneInspector.new()
    add_inspector_plugin(network_scene_inspector_instance)

    # get_editor_interface().get_editor_main_screen().add_child(main_panel_instance)

func _exit_tree():

    remove_inspector_plugin(network_scene_inspector_instance)

    remove_control_from_docks(dock_network_scenes_instance)
    dock_network_scenes_instance.queue_free()

    remove_custom_type("NetworkTransform3D")
    remove_custom_type("NetworkTransform2D")

    remove_autoload_singleton(AUTOLOAD_RUNNER)
    remove_autoload_singleton(AUTOLOAD_ENV)

# func _has_main_screen():
#     return true

# func _make_visible(visible):
#     if main_panel_instance:
#         main_panel_instance.visible = visible

# func _get_plugin_icon():
#     # Must return some kind of Texture for the icon.
#     return get_editor_interface().get_base_control().get_theme_icon("Signals", "EditorIcons")
