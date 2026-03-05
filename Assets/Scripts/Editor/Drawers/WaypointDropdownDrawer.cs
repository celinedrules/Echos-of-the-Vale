// Done
using Core.Attributes;
using Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Drawers
{
    [CustomPropertyDrawer(typeof(WaypointDropdownAttribute))]
    public class WaypointDropdownDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var attr = (WaypointDropdownAttribute)attribute;
            
            // Get the scene name from the sibling field
            var sceneProperty = property.serializedObject.FindProperty(attr.SceneFieldName);
            string targetScene = sceneProperty?.stringValue ?? "";

            if (string.IsNullOrEmpty(targetScene))
            {
                EditorGUI.LabelField(position, label.text, "(Select a scene first)");
                return;
            }

            // Find the LocationDatabase asset
            string[] guids = AssetDatabase.FindAssets("t:LocationDatabase");
            if (guids.Length == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var database = AssetDatabase.LoadAssetAtPath<LocationDatabase>(path);

            if (database == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            // Get waypoints for the target scene
            var waypointsInScene = database.GetWaypointsForScene(targetScene);

            if (waypointsInScene.Count == 0)
            {
                EditorGUI.LabelField(position, label.text, $"(No waypoints in {targetScene})");
                return;
            }

            string[] displayNames = new string[waypointsInScene.Count + 1];
            string[] waypointIds = new string[waypointsInScene.Count + 1];

            displayNames[0] = "(None)";
            waypointIds[0] = "";

            int currentIndex = 0;

            for (int i = 0; i < waypointsInScene.Count; i++)
            {
                var entry = waypointsInScene[i];
                waypointIds[i + 1] = entry.waypointId;
                displayNames[i + 1] = string.IsNullOrEmpty(entry.displayName) 
                    ? entry.waypointId 
                    : entry.displayName;

                if (entry.waypointId == property.stringValue)
                    currentIndex = i + 1;
            }

            EditorGUI.BeginProperty(position, label, property);
            
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, displayNames);
            
            if (newIndex != currentIndex)
                property.stringValue = waypointIds[newIndex];

            EditorGUI.EndProperty();
        }
    }
}