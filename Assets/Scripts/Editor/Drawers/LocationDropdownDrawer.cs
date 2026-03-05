// Done
using Core.Attributes;
using Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Drawers
{
    [CustomPropertyDrawer(typeof(LocationDropdownAttribute))]
    public class LocationDropdownDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
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

            // Get locations via SerializedObject to access the private list
            var dbSerializedObject = new SerializedObject(database);
            var locationsProp = dbSerializedObject.FindProperty("locations");

            int count = locationsProp.arraySize;
            string[] displayNames = new string[count + 1];
            string[] sceneNames = new string[count + 1];
            
            displayNames[0] = "(None)";
            sceneNames[0] = "";

            int currentIndex = 0;

            for (int i = 0; i < count; i++)
            {
                var entry = locationsProp.GetArrayElementAtIndex(i);
                string sceneName = entry.FindPropertyRelative("sceneName").stringValue;
                string displayName = entry.FindPropertyRelative("displayName").stringValue;

                sceneNames[i + 1] = sceneName;
                displayNames[i + 1] = string.IsNullOrEmpty(displayName) ? sceneName : displayName;

                if (sceneName == property.stringValue)
                    currentIndex = i + 1;
            }

            EditorGUI.BeginProperty(position, label, property);
            
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, displayNames);
            
            if (newIndex != currentIndex)
                property.stringValue = sceneNames[newIndex];

            EditorGUI.EndProperty();
        }
    }
}