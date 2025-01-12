@tool
extends VBoxContainer

@export var properties_parent: Control
@export var properties_container: VBoxContainer
@export var title_label: Label

var property_row_scene = preload("res://addons/HLNC/Tools/Inspector/property_row.tscn")

func set_network_type(type: String) -> void:
    title_label.text = "Network %s" % type

func add_property(name: String, type: String) -> void:
    var property_row = property_row_scene.instantiate()
    property_row.get_node("Name").text = name
    property_row.get_node("Type").text = type
    properties_container.add_child(property_row)
    properties_parent.visible = true