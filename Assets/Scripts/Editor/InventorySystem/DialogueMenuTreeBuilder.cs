// Done
using System;
using System.Collections.Generic;
using System.Linq;
using Data.DialogueData;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Editor.InventorySystem
{
    internal static class DialogueMenuTreeBuilder
    {
        public static void AddDialogueTables(OdinMenuTree tree, string rootLabel, string dialogueRootPath, Action<OdinMenuItem> onItemAdded = null)
        {
            string[] guids = AssetDatabase.FindAssets("t:DialogueTable", new[] { dialogueRootPath });

            List<DialogueTable> tables = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(p => AssetDatabase.LoadAssetAtPath<DialogueTable>(p))
                .Where(t => t != null)
                .OrderBy(t => t.name)
                .ToList();

            foreach (DialogueTable table in tables)
            {
                foreach (OdinMenuItem item in tree.Add($"{rootLabel}/{table.name}", table))
                    onItemAdded?.Invoke(item);
            }
        }
    }
}