@tool
extends VBoxContainer

@onready var tree: Tree = $Tree
@export var world_debug: Control

func update_tree(frame_data: Dictionary) -> void:
    var world_state: Dictionary = frame_data.get("world_state")
    if world_state.is_empty():
        return
    
    # Create root node if it doesn't exist
    var root = tree.get_root()
    if not root:
        root = tree.create_item()
    
    # Update root node text
    root.set_text(0, world_state.get("nodeName", "Root"))
    
    # Reconcile children
    _reconcile_children(root, world_state.get("children", {}))

func _reconcile_children(parent_item: TreeItem, children: Dictionary) -> void:
    var existing_children = {}
    var child = parent_item.get_first_child()

    while child:
        existing_children[child.get_text(0)] = child
        child = child.get_next()

    for child_name in children:
        var child_data = children[child_name][0]
        var child_item: TreeItem
        
        if existing_children.has(child_name):
            child_item = existing_children[child_name]
            existing_children.erase(child_name)
        else:
            child_item = tree.create_item(parent_item)
        
        child_item.set_text(0, child_data.get("nodeName", child_name))
        
        if child_data.has("children"):
            _reconcile_children(child_item, child_data["children"])
    
    for child_item in existing_children.values():
        child_item.free()

func _on_world_debug_tick_frame_selected(tickFrame: TickFrameUI) -> void:
    var frame_data = world_debug.call("GetFrameData", tickFrame.tick_frame_id)
    update_tree(frame_data)

func _on_world_debug_tick_frame_updated(id:int) -> void:
    var frame_data = world_debug.call("GetFrameData", id)
    update_tree(frame_data)
