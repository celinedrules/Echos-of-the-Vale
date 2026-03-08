// Done
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class StateCreator
    {
        [MenuItem("Assets/Create/Player State", false, 80)]
        private static void CreatePlayerState()
        {
            string folderPath = GetSelectedFolderPath();
            string stateName = "NewState";

            stateName = EditorUtility.SaveFilePanelInProject(
                "Create New Player State",
                $"Player{stateName}State",
                "cs",
                "Enter a name for the new Player state script.",
                folderPath
            );

            if (string.IsNullOrEmpty(stateName))
                return;

            // Extract just the file name (without extension)
            string className = Path.GetFileNameWithoutExtension(stateName);

            // Template for the new state script
            string template = $@"using UnityEngine;

namespace UltimateRpg
{{
    public class {className} : EntityState
    {{
        public override void Enter()
        {{
            base.Enter();
            // TODO: Add enter logic here
        }}

        public override void Update()
        {{
            base.Update();
            // TODO: Add update logic here
        }}

        public override void Exit()
        {{
            base.Exit();
            // TODO: Add exit logic here
        }}
    }}
}}";

            File.WriteAllText(stateName, template);
            AssetDatabase.Refresh();

            Debug.Log($"Created new Player state: {className}");
        }

        private static string GetSelectedFolderPath()
        {
            string path = "Assets";
            foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                }

                break;
            }

            return path.Replace("\\", "/");
        }
    }
}