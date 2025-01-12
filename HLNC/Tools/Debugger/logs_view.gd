@tool
extends Window

@export var follow_check_box: CheckBox

func _on_close_requested() -> void:
    hide()

func _on_open() -> void:
    popup_centered()

func _on_world_debug_log(frameId:int, timestamp:String, level:String, message:String) -> void:
    $Panel/LogBox.text += "%s [Tick %d] %s: %s\n" % [timestamp, frameId, level, message]
    if follow_check_box.button_pressed:
        $Panel/LogBox.scroll_to_line($Panel/LogBox.get_line_count() - 1)