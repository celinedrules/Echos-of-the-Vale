// Done
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Editor.InventorySystem
{
    public class ScriptableObjectSelector<T> : OdinSelector<Type> where T : ScriptableObject
    {
        private Action<T> _onScriptableObjectCreated;
        private string _defaultDestinationPath;

        public ScriptableObjectSelector(string defaultDestinationPath, Action<T> onScriptableObjectCreated = null)
        {
            this._onScriptableObjectCreated = onScriptableObjectCreated;
            this._defaultDestinationPath = defaultDestinationPath;
            SelectionConfirmed += ShowSaveFileDialog;
        }

        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            IEnumerable<Type> scriptableObjectTypes = AssemblyUtilities.GetTypes(AssemblyCategory.ProjectSpecific)
                .Where(x => x.IsClass && !x.IsAbstract && x.InheritsFrom(typeof(T)));

            tree.Selection.SupportsMultiSelect = false;
            tree.Config.DrawSearchToolbar = true;
            tree.AddRange(scriptableObjectTypes, x => x.GetNiceName())
                .AddThumbnailIcons();
        }

        protected override void DrawSelectionTree()
        {
            base.DrawSelectionTree();

            // Check if anything is selected and confirm it. 
            // To prevent the "double popup", we check if the selection is already being confirmed.
            if (SelectionTree.Selection.Any())
                SelectionTree.Selection.ConfirmSelection();
        }

        private void ShowSaveFileDialog(IEnumerable<Type> selection)
        {
            Type type = selection.FirstOrDefault();

            if (type == null)
                return;

            SelectionTree.Selection.Clear();

            string dest = _defaultDestinationPath.TrimEnd('/');

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
                AssetDatabase.Refresh();
            }

            dest = EditorUtility.SaveFilePanel("Save object as", dest, "New " + type.GetNiceName(), "asset");

            if (!string.IsNullOrEmpty(dest) &&
                PathUtilities.TryMakeRelative(Path.GetDirectoryName(Application.dataPath), dest, out dest))
            {
                T obj = ScriptableObject.CreateInstance(type) as T;
                AssetDatabase.CreateAsset(obj, dest);
                AssetDatabase.Refresh();

                if (this._onScriptableObjectCreated != null)
                {
                    this._onScriptableObjectCreated(obj);
                }
            }
        }
    }
}