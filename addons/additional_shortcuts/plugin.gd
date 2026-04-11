@tool
extends EditorPlugin

# Configurable shortcut definitions  
var shortcuts = [
	{
		"name": "Focus Scene Tree",
		"action": "additional_shortcuts_focus_scene_tree",
		"callback": "focus_scene_tree",
		"description": "Focus the Scene Tree dock for node navigation"
	},
	{
		"name": "Focus Inspector Filter", 
		"action": "additional_shortcuts_focus_inspector_filter",
		"callback": "focus_inspector_filter",
		"description": "Focus the Inspector properties filter input"
	}
]

# Default shortcuts (used if user hasn't customized)
var default_shortcuts = {
	"additional_shortcuts_focus_scene_tree": [KEY_T, true, true, false],  # Key, Ctrl, Shift, Alt
	"additional_shortcuts_focus_inspector_filter": [KEY_I, true, true, false]
}

func _enter_tree():
	# Register shortcuts in editor settings with defaults
	var editor_settings = EditorInterface.get_editor_settings()
	for shortcut in shortcuts:
		var action = shortcut.action
		
		# Create default shortcut if not exists
		if not editor_settings.has_setting("shortcuts/" + action):
			var default = default_shortcuts[action]
			var sc = Shortcut.new()
			var event = InputEventKey.new()
			event.keycode = default[0]
			event.ctrl_pressed = default[1] 
			event.shift_pressed = default[2]
			event.alt_pressed = default[3]
			sc.events = [event]
			editor_settings.set_setting("shortcuts/" + action, sc)
		
		# Add to Tools menu
		add_tool_menu_item(shortcut.name, Callable(self, shortcut.callback))
	
	print("[Additional Shortcuts] Registered ", shortcuts.size(), " configurable shortcuts")
	print("[Additional Shortcuts] Configure in: Editor → Editor Settings → Shortcuts")

func _exit_tree():
	for shortcut in shortcuts:
		remove_tool_menu_item(shortcut.name)

func _input(event: InputEvent):
	if not event is InputEventKey or not event.pressed:
		return
	
	# Check against configured shortcuts from editor settings
	var editor_settings = EditorInterface.get_editor_settings()
	for shortcut in shortcuts:
		var action = shortcut.action
		if editor_settings.has_setting("shortcuts/" + action):
			var sc = editor_settings.get_setting("shortcuts/" + action)
			if sc and sc.matches_event(event):
				call(shortcut.callback)
				get_viewport().set_input_as_handled()
				return

func focus_scene_tree():
	var base = Engine.get_singleton("EditorInterface").get_base_control()
	var scene_tree_editors = []
	find_scene_tree_editors(base, scene_tree_editors)
	
	for editor in scene_tree_editors:
		for child in editor.get_children():
			if child is Tree:
				var parent = child
				var in_script_editor = false
				while parent:
					if parent.get_class() == "ScriptEditor":
						in_script_editor = true
						break
					parent = parent.get_parent()
				
				if not in_script_editor:
					child.grab_focus()
					return

func focus_inspector_filter():
	var base = Engine.get_singleton("EditorInterface").get_base_control()
	var inspector_dock = base.find_child("Inspector", true, false)
	if inspector_dock:
		find_properties_filter(inspector_dock)

func find_properties_filter(node: Node):
	# Look for the Filter Properties input
	if node is LineEdit and node.placeholder_text == "Filter Properties":
		node.grab_focus()
		return
	
	for child in node.get_children():
		find_properties_filter(child)


func find_scene_tree_editors(node: Node, editors: Array):
	if node.get_class() == "SceneTreeEditor":
		editors.append(node)
	
	for child in node.get_children():
		find_scene_tree_editors(child, editors)
