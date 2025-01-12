@tool
class_name TickFrameUI extends Control

var bar_red_stylebox = preload("res://addons/HLNC/Tools/Debugger/Ticks/tick_frame_bar_color_red.tres")
var bar_yellow_stylebox = preload("res://addons/HLNC/Tools/Debugger/Ticks/tick_frame_bar_color_yellow.tres")
var bar_green_stylebox = preload("res://addons/HLNC/Tools/Debugger/Ticks/tick_frame_bar_color_green.tres")

var unselected_outline = preload("res://addons/HLNC/Tools/Debugger/Ticks/tick_frame_unselected.tres")
var selected_outline = preload("res://addons/HLNC/Tools/Debugger/Ticks/tick_frame_selected.tres")

const frame_height = 112
var previous_tick_frame: Control
var next_tick_frame: Control
var is_selected: bool = false

var tick_frame_id: int

func set_frame_size(size: int) -> void:
    var mtu = ProjectSettings.get_setting("HLNC/network/MTU", 1400)
    var tick_size_fraction = float(size) / float(mtu)
    var tick_frame_bar: Panel = $Bar
    tick_frame_bar.set_size(Vector2(0, frame_height * tick_size_fraction), false)
    tick_frame_bar.set_position(Vector2(0, frame_height - (frame_height * tick_size_fraction)), false)

    if tick_size_fraction > 0.75:
        tick_frame_bar.add_theme_stylebox_override("panel", bar_red_stylebox)
    elif tick_size_fraction > 0.5:
        tick_frame_bar.add_theme_stylebox_override("panel", bar_yellow_stylebox)
    else:
        tick_frame_bar.add_theme_stylebox_override("panel", bar_green_stylebox)

func select() -> void:
    $Outline.add_theme_stylebox_override("panel", selected_outline)
    is_selected = true

func deselect() -> void:
    $Outline.add_theme_stylebox_override("panel", unselected_outline)
    is_selected = false

func _unhandled_input(event: InputEvent) -> void:
    if is_selected:
        if event is InputEventKey and event.keycode == KEY_LEFT and event.pressed:
            if previous_tick_frame != null:
                get_parent().call("OnTickFrameSelected", previous_tick_frame, true)
                accept_event()
        elif event is InputEventKey and event.keycode == KEY_RIGHT and event.pressed:
            if next_tick_frame != null:
                get_parent().call("OnTickFrameSelected", next_tick_frame, true)
                accept_event()

func _on_gui_input(event:InputEvent) -> void:
    if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.is_pressed():
        get_parent().call("OnTickFrameSelected", self, true)
