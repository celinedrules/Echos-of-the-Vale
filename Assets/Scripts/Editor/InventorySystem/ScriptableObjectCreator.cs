// Done
using System;
using System.IO;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Editor.InventorySystem
{
    public static class ScriptableObjectCreator
    {
        public static void ShowDialog<T>(string defaultDestinationPath, Action<T> onScriptableObjectCreated = null)
            where T : ScriptableObject
        {
            ScriptableObjectSelector<T> selector = new(defaultDestinationPath, onScriptableObjectCreated);

            if (selector.SelectionTree.EnumerateTree().Count() == 1)
            {
                selector.SelectionTree.EnumerateTree().First().Select();
                selector.SelectionTree.Selection.ConfirmSelection();
            }
            else
            {
                selector.ShowInPopup(200);
            }
        }

        /// <summary>
        /// Creates a ScriptableObject of the exact type T without showing a type selector.
        /// Useful when you know the exact type and don't want subclass options.
        /// </summary>
        public static void CreateExact<T>(string defaultDestinationPath, Action<T> onCreated = null)
            where T : ScriptableObject
        {
            string dest = defaultDestinationPath.TrimEnd('/');

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
                AssetDatabase.Refresh();
            }

            dest = EditorUtility.SaveFilePanel("Save object as", dest, "New " + typeof(T).Name, "asset");

            if (string.IsNullOrEmpty(dest))
                return;

            if (PathUtilities.TryMakeRelative(Path.GetDirectoryName(Application.dataPath), dest, out dest))
            {
                T obj = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(obj, dest);
                AssetDatabase.Refresh();
                onCreated?.Invoke(obj);
            }
        }
    }
}