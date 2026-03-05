// Done
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Data;
using Interactables;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    [CustomEditor(typeof(LocationDatabase))]
    public class LocationDatabaseEditor : UnityEditor.Editor
    {
        private SerializedProperty _locations;
        private HashSet<int> _expandedEntries = new();

        private void OnEnable() => _locations = serializedObject.FindProperty("locations");

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Location Mappings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Refresh All button
            if (GUILayout.Button("🔄 Refresh All Waypoints", GUILayout.Height(25)))
                RefreshAllWaypoints();

            EditorGUILayout.Space();

            for (int i = 0; i < _locations.arraySize; i++)
            {
                SerializedProperty entry = _locations.GetArrayElementAtIndex(i);
                SerializedProperty sceneNameProp = entry.FindPropertyRelative("sceneName");
                SerializedProperty displayNameProp = entry.FindPropertyRelative("displayName");
                SerializedProperty waypointsProp = entry.FindPropertyRelative("waypoints");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();

                // Foldout for waypoints
                bool isExpanded = _expandedEntries.Contains(i);
                bool newExpanded = GUILayout.Toggle(isExpanded, "", EditorStyles.foldout, GUILayout.Width(15));

                if (newExpanded != isExpanded)
                {
                    if (newExpanded)
                        _expandedEntries.Add(i);
                    else
                        _expandedEntries.Remove(i);
                }

                // Scene asset picker
                SceneAsset currentScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    GetScenePath(sceneNameProp.stringValue));

                string previousSceneName = sceneNameProp.stringValue;
                SceneAsset newScene = (SceneAsset)EditorGUILayout.ObjectField(
                    currentScene, typeof(SceneAsset), false, GUILayout.Width(130));

                if (newScene != currentScene)
                {
                    sceneNameProp.stringValue = newScene != null ? newScene.name : "";
                    ((LocationDatabase)target).InvalidateCache();

                    // Auto-scan waypoints when a new scene is assigned
                    if (newScene && newScene.name != previousSceneName)
                    {
                        serializedObject.ApplyModifiedProperties();
                        ScanWaypointsForEntry(i, newScene.name);
                        serializedObject.Update();
                    }
                }

                // Display name field
                displayNameProp.stringValue = EditorGUILayout.TextField(displayNameProp.stringValue);

                // Waypoint count indicator
                int waypointCount = waypointsProp.arraySize;
                EditorGUILayout.LabelField($"[{waypointCount}]", GUILayout.Width(30));

                // Refresh single scene button
                if (GUILayout.Button("↻", GUILayout.Width(25)))
                {
                    serializedObject.ApplyModifiedProperties();
                    ScanWaypointsForEntry(i, sceneNameProp.stringValue);
                    serializedObject.Update();
                }

                // Use current scene button
                if (GUILayout.Button("Current", GUILayout.Width(55)))
                {
                    Scene activeScene = SceneManager.GetActiveScene();
                    sceneNameProp.stringValue = activeScene.name;
                    ((LocationDatabase)target).InvalidateCache();

                    serializedObject.ApplyModifiedProperties();
                    ScanWaypointsForEntry(i, activeScene.name);
                    serializedObject.Update();
                }

                // Remove button
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    _locations.DeleteArrayElementAtIndex(i);
                    ((LocationDatabase)target).InvalidateCache();
                    _expandedEntries.Remove(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                // Show waypoints when expanded
                if (newExpanded && waypointsProp.arraySize > 0)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Waypoints:", EditorStyles.miniBoldLabel);

                    for (int w = 0; w < waypointsProp.arraySize; w++)
                    {
                        SerializedProperty waypointEntry = waypointsProp.GetArrayElementAtIndex(w);
                        SerializedProperty wpId = waypointEntry.FindPropertyRelative("waypointId");
                        SerializedProperty wpDisplay = waypointEntry.FindPropertyRelative("displayName");

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(wpId.stringValue, GUILayout.Width(150));
                        wpDisplay.stringValue = EditorGUILayout.TextField(wpDisplay.stringValue);
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Add Location"))
            {
                _locations.InsertArrayElementAtIndex(_locations.arraySize);
                SerializedProperty newEntry = _locations.GetArrayElementAtIndex(_locations.arraySize - 1);
                newEntry.FindPropertyRelative("sceneName").stringValue = "";
                newEntry.FindPropertyRelative("displayName").stringValue = "";
                newEntry.FindPropertyRelative("waypoints").ClearArray();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void RefreshAllWaypoints()
        {
            for (int i = 0; i < _locations.arraySize; i++)
            {
                SerializedProperty entry = _locations.GetArrayElementAtIndex(i);
                string sceneName = entry.FindPropertyRelative("sceneName").stringValue;

                if (!string.IsNullOrEmpty(sceneName))
                    ScanWaypointsForEntry(i, sceneName);
            }

            serializedObject.Update();
            Debug.Log("All waypoints refreshed!");
        }

        private void ScanWaypointsForEntry(int entryIndex, string sceneName)
        {
            string scenePath = GetScenePath(sceneName);

            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogWarning($"Could not find scene: {sceneName}");
                return;
            }

            // Check if this is the current scene - if so, scan live objects
            Scene currentScene = SceneManager.GetActiveScene();

            if (currentScene.name == sceneName)
            {
                ScanWaypointsInCurrentScene(entryIndex, currentScene);
                return;
            }

            // Otherwise, parse the scene file directly without loading it
            ScanWaypointsFromSceneFile(entryIndex, scenePath);
        }

        private void ScanWaypointsInCurrentScene(int entryIndex, Scene scene)
        {
            LocationDatabase database = (LocationDatabase)target;
            SerializedObject so = new(database);
            SerializedProperty locations = so.FindProperty("locations");
            SerializedProperty entry = locations.GetArrayElementAtIndex(entryIndex);
            SerializedProperty waypointsProp = entry.FindPropertyRelative("waypoints");

            // Preserve existing display names
            Dictionary<string, string> existingDisplayNames = GetExistingDisplayNames(waypointsProp);
            waypointsProp.ClearArray();

            // Find all waypoints in the current scene
            GameObject[] rootObjects = scene.GetRootGameObjects();
            List<Waypoint> waypoints = new List<Waypoint>();

            foreach (GameObject root in rootObjects)
                waypoints.AddRange(root.GetComponentsInChildren<Waypoint>(true));

            foreach (Waypoint waypoint in waypoints)
            {
                SerializedObject wpSo = new(waypoint);
                SerializedProperty waypointIdProp = wpSo.FindProperty("waypointId");
                SerializedProperty waypointNameProp = wpSo.FindProperty("waypointName");

                string waypointId = waypointIdProp?.stringValue;
                string waypointName = waypointNameProp?.stringValue;

                if (string.IsNullOrEmpty(waypointId))
                    waypointId = waypoint.gameObject.name;

                AddWaypointEntry(waypointsProp, waypointId, waypointName, existingDisplayNames);
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);

            Debug.Log($"Found {waypoints.Count} waypoints in {scene.name}");
        }

        private void ScanWaypointsFromSceneFile(int entryIndex, string scenePath)
        {
            LocationDatabase database = (LocationDatabase)target;
            SerializedObject so = new(database);
            SerializedProperty locations = so.FindProperty("locations");
            SerializedProperty entry = locations.GetArrayElementAtIndex(entryIndex);
            SerializedProperty waypointsProp = entry.FindPropertyRelative("waypoints");

            // Preserve existing display names
            Dictionary<string, string> existingDisplayNames = GetExistingDisplayNames(waypointsProp);
            waypointsProp.ClearArray();

            // Read and parse the scene file
            string sceneContent = File.ReadAllText(scenePath);

            // Find all Waypoint MonoBehaviour blocks and extract waypointId and waypointName
            MatchCollection waypointMatches = Regex.Matches(sceneContent,
                @"MonoBehaviour:.*?m_Script:.*?guid: ([a-f0-9]+).*?(?=---|\z)",
                RegexOptions.Singleline);

            // Get the Waypoint script GUID
            string waypointScriptGuid = GetScriptGuid<Waypoint>();
            int waypointCount = 0;

            foreach (Match match in waypointMatches)
            {
                string guid = match.Groups[1].Value;

                if (guid != waypointScriptGuid)
                    continue;

                string block = match.Value;

                // Extract waypointId
                Match idMatch = Regex.Match(block, @"waypointId:\s*(.+)");
                string waypointId = idMatch.Success ? idMatch.Groups[1].Value.Trim() : "";

                // Extract waypointName
                Match nameMatch = Regex.Match(block, @"waypointName:\s*(.+)");
                string waypointName = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : "";

                if (string.IsNullOrEmpty(waypointId))
                    continue;

                AddWaypointEntry(waypointsProp, waypointId, waypointName, existingDisplayNames);
                waypointCount++;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);

            Debug.Log($"Found {waypointCount} waypoints in {Path.GetFileNameWithoutExtension(scenePath)}");
        }

        private Dictionary<string, string> GetExistingDisplayNames(SerializedProperty waypointsProp)
        {
            Dictionary<string, string> existingDisplayNames = new();

            for (int i = 0; i < waypointsProp.arraySize; i++)
            {
                SerializedProperty wp = waypointsProp.GetArrayElementAtIndex(i);
                string id = wp.FindPropertyRelative("waypointId").stringValue;
                string display = wp.FindPropertyRelative("displayName").stringValue;

                if (!string.IsNullOrEmpty(display))
                    existingDisplayNames[id] = display;
            }

            return existingDisplayNames;
        }

        private void AddWaypointEntry(SerializedProperty waypointsProp, string waypointId,
            string waypointName, Dictionary<string, string> existingDisplayNames)
        {
            waypointsProp.InsertArrayElementAtIndex(waypointsProp.arraySize);
            SerializedProperty newWp = waypointsProp.GetArrayElementAtIndex(waypointsProp.arraySize - 1);
            newWp.FindPropertyRelative("waypointId").stringValue = waypointId;

            // Use existing display name, or waypointName, or waypointId
            string displayName = existingDisplayNames.TryGetValue(waypointId, out string existing)
                ? existing
                : (!string.IsNullOrEmpty(waypointName) ? waypointName : waypointId);

            newWp.FindPropertyRelative("displayName").stringValue = displayName;
        }

        private string GetScriptGuid<T>() where T : MonoBehaviour
        {
            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {typeof(T).Name}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (monoScript && monoScript.GetClass() == typeof(T))
                    return guid;
            }

            return "";
        }

        private string GetScenePath(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return "";

            string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (Path.GetFileNameWithoutExtension(path) == sceneName)
                    return path;
            }

            return "";
        }

        /// <summary>
        /// Static method to refresh all waypoints, callable from toolbar buttons or other editors.
        /// </summary>
        public static void RefreshAllWaypoints(LocationDatabase database)
        {
            SerializedObject so = new(database);
            SerializedProperty locations = so.FindProperty("locations");

            for (int i = 0; i < locations.arraySize; i++)
            {
                SerializedProperty entry = locations.GetArrayElementAtIndex(i);
                string sceneName = entry.FindPropertyRelative("sceneName").stringValue;

                if (!string.IsNullOrEmpty(sceneName))
                    ScanWaypointsForEntryStatic(so, locations, i, sceneName);
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            Debug.Log("All waypoints refreshed from toolbar!");
        }

        private static void ScanWaypointsForEntryStatic(SerializedObject so, SerializedProperty locations,
            int entryIndex, string sceneName)
        {
            string scenePath = GetScenePathStatic(sceneName);

            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogWarning($"Could not find scene: {sceneName}");
                return;
            }

            Scene currentScene = SceneManager.GetActiveScene();

            if (currentScene.name == sceneName)
            {
                ScanWaypointsInCurrentSceneStatic(so, locations, entryIndex, currentScene);
                return;
            }

            ScanWaypointsFromSceneFileStatic(so, locations, entryIndex, scenePath);
        }

        private static void ScanWaypointsInCurrentSceneStatic(SerializedObject so, SerializedProperty locations,
            int entryIndex, Scene scene)
        {
            SerializedProperty entry = locations.GetArrayElementAtIndex(entryIndex);
            SerializedProperty waypointsProp = entry.FindPropertyRelative("waypoints");

            Dictionary<string, string> existingDisplayNames = GetExistingDisplayNamesStatic(waypointsProp);
            waypointsProp.ClearArray();

            GameObject[] rootObjects = scene.GetRootGameObjects();
            List<Waypoint> waypoints = new List<Waypoint>();

            foreach (GameObject root in rootObjects)
                waypoints.AddRange(root.GetComponentsInChildren<Waypoint>(true));

            foreach (Waypoint waypoint in waypoints)
            {
                SerializedObject wpSo = new(waypoint);
                SerializedProperty waypointIdProp = wpSo.FindProperty("waypointId");
                SerializedProperty waypointNameProp = wpSo.FindProperty("waypointName");

                string waypointId = waypointIdProp?.stringValue;
                string waypointName = waypointNameProp?.stringValue;

                if (string.IsNullOrEmpty(waypointId))
                    waypointId = waypoint.gameObject.name;

                AddWaypointEntryStatic(waypointsProp, waypointId, waypointName, existingDisplayNames);
            }

            so.ApplyModifiedProperties();
            Debug.Log($"Found {waypoints.Count} waypoints in {scene.name}");
        }

        private static void ScanWaypointsFromSceneFileStatic(SerializedObject so, SerializedProperty locations,
            int entryIndex, string scenePath)
        {
            SerializedProperty entry = locations.GetArrayElementAtIndex(entryIndex);
            SerializedProperty waypointsProp = entry.FindPropertyRelative("waypoints");

            Dictionary<string, string> existingDisplayNames = GetExistingDisplayNamesStatic(waypointsProp);
            waypointsProp.ClearArray();

            string sceneContent = File.ReadAllText(scenePath);

            MatchCollection waypointMatches = Regex.Matches(sceneContent,
                @"MonoBehaviour:.*?m_Script:.*?guid: ([a-f0-9]+).*?(?=---|\z)",
                RegexOptions.Singleline);

            string waypointScriptGuid = GetScriptGuidStatic<Waypoint>();
            int waypointCount = 0;

            foreach (Match match in waypointMatches)
            {
                string guid = match.Groups[1].Value;

                if (guid != waypointScriptGuid)
                    continue;

                string block = match.Value;

                Match idMatch = Regex.Match(block, @"waypointId:\s*(.+)");
                string waypointId = idMatch.Success ? idMatch.Groups[1].Value.Trim() : "";

                Match nameMatch = Regex.Match(block, @"waypointName:\s*(.+)");
                string waypointName = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : "";

                if (string.IsNullOrEmpty(waypointId))
                    continue;

                AddWaypointEntryStatic(waypointsProp, waypointId, waypointName, existingDisplayNames);
                waypointCount++;
            }

            so.ApplyModifiedProperties();
            Debug.Log($"Found {waypointCount} waypoints in {Path.GetFileNameWithoutExtension(scenePath)}");
        }

        private static Dictionary<string, string> GetExistingDisplayNamesStatic(SerializedProperty waypointsProp)
        {
            Dictionary<string, string> existingDisplayNames = new Dictionary<string, string>();
            for (int i = 0; i < waypointsProp.arraySize; i++)
            {
                SerializedProperty wp = waypointsProp.GetArrayElementAtIndex(i);
                string id = wp.FindPropertyRelative("waypointId").stringValue;
                string display = wp.FindPropertyRelative("displayName").stringValue;

                if (!string.IsNullOrEmpty(display))
                    existingDisplayNames[id] = display;
            }

            return existingDisplayNames;
        }

        private static void AddWaypointEntryStatic(SerializedProperty waypointsProp, string waypointId,
            string waypointName, Dictionary<string, string> existingDisplayNames)
        {
            waypointsProp.InsertArrayElementAtIndex(waypointsProp.arraySize);
            SerializedProperty newWp = waypointsProp.GetArrayElementAtIndex(waypointsProp.arraySize - 1);
            newWp.FindPropertyRelative("waypointId").stringValue = waypointId;

            string displayName = existingDisplayNames.TryGetValue(waypointId, out string existing)
                ? existing
                : (!string.IsNullOrEmpty(waypointName) ? waypointName : waypointId);

            newWp.FindPropertyRelative("displayName").stringValue = displayName;
        }

        private static string GetScriptGuidStatic<T>() where T : MonoBehaviour
        {
            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {typeof(T).Name}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (monoScript && monoScript.GetClass() == typeof(T))
                    return guid;
            }

            return "";
        }

        private static string GetScenePathStatic(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return "";

            string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (Path.GetFileNameWithoutExtension(path) == sceneName)
                    return path;
            }

            return "";
        }
    }
}