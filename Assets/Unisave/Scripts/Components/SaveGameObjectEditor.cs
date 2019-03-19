using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SaveGameObject))]
public class SaveGameObjectEditor : Editor
{
	private SaveGameObject script;
	private GameObject gameObject;

	void OnEnable()
	{
		script = this.target as SaveGameObject;

		if (script != null)
			gameObject = script.gameObject;
	}
	
	public override void OnInspectorGUI()
	{
		if (gameObject == null)
			return;

		foreach (Component component in gameObject.GetComponents<Component>())
		{
			EditorGUILayout.LabelField(component.GetType().ToString());

			EditorGUILayout.Toggle("Foo", false);
			EditorGUILayout.Toggle("Bar", false);
		}
	}
}
