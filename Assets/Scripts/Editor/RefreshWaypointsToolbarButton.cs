// Done
using Data;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;

namespace Editor
{
    [Overlay(typeof(SceneView), "Refresh Waypoints", true)]
    public class RefreshWaypointsToolbarOverlay : ToolbarOverlay
    {
        public RefreshWaypointsToolbarOverlay() : base(RefreshWaypointsToolbarButton.Id) { }
    }

    [EditorToolbarElement(Id, typeof(SceneView))]
    public class RefreshWaypointsToolbarButton : EditorToolbarButton
    {
        public const string Id = "UltimateRpg/RefreshWaypoints";

        public RefreshWaypointsToolbarButton()
        {
            text = "🔄 Waypoints";
            tooltip = "Refresh all waypoints in the Location Database";
            clicked += OnClick;
        }

        private void OnClick()
        {
            // Find the LocationDatabase asset
            string[] guids = AssetDatabase.FindAssets("t:LocationDatabase");
            
            if (guids.Length == 0)
            {
                Debug.LogWarning("No LocationDatabase asset found in the project!");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var database = AssetDatabase.LoadAssetAtPath<LocationDatabase>(path);

            if (database == null)
            {
                Debug.LogWarning("Failed to load LocationDatabase asset!");
                return;
            }

            // Use the editor to refresh waypoints
            LocationDatabaseEditor.RefreshAllWaypoints(database);
        }
    }
}