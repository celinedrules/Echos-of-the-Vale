// Done
using System;
using System.Collections.Generic;
using System.Linq;
using Data.QuestData;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Editor.InventorySystem
{
    internal static class QuestMenuTreeBuilder
    {
        public static void AddQuests(OdinMenuTree tree, string rootLabel, string questsRootPath,
            Action<OdinMenuItem> onItemAdded = null)
        {
            string[] guids = AssetDatabase.FindAssets("t:QuestData", new[] { questsRootPath });

            List<QuestData> quests = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(p => AssetDatabase.LoadAssetAtPath<QuestData>(p))
                .Where(q => q != null)
                .ToList();

            Dictionary<QuestData, List<QuestData>> childrenByParent = new Dictionary<QuestData, List<QuestData>>();
            HashSet<QuestData> referencedAsChild = new HashSet<QuestData>();

            foreach (QuestData q in quests)
            {
                List<QuestData> children = GetSubQuestsViaSerializedObject(q)
                    .Where(c => c)
                    .Distinct()
                    .ToList();

                if (children.Count == 0)
                    continue;

                childrenByParent[q] = children;
                foreach (QuestData child in children)
                    referencedAsChild.Add(child);
            }

            List<QuestData> roots = quests
                .Where(q => !referencedAsChild.Contains(q))
                .OrderBy(q => q.name)
                .ToList();

            HashSet<QuestData> added = new HashSet<QuestData>();

            foreach (QuestData root in roots)
            {
                AddQuestRecursive(
                    tree,
                    $"{rootLabel}/{root.name}",
                    root,
                    childrenByParent,
                    added,
                    pathVisited: new HashSet<QuestData>(),
                    onItemAdded);
            }

            List<QuestData> unlinked = quests.Where(q => !added.Contains(q)).OrderBy(q => q.name).ToList();
            foreach (QuestData q in unlinked)
            {
                foreach (OdinMenuItem item in tree.Add($"{rootLabel}/(Unlinked)/{q.name}", q))
                    onItemAdded?.Invoke(item);
            }
        }

        private static void AddQuestRecursive(
            OdinMenuTree tree,
            string menuPath,
            QuestData quest,
            Dictionary<QuestData, List<QuestData>> childrenByParent,
            HashSet<QuestData> added,
            HashSet<QuestData> pathVisited,
            Action<OdinMenuItem> onItemAdded)
        {
            if (!quest) return;

            if (pathVisited.Contains(quest))
            {
                foreach (OdinMenuItem item in tree.Add($"{menuPath}/(Cycle Detected)/{quest.name}", quest))
                    onItemAdded?.Invoke(item);
                return;
            }

            foreach (OdinMenuItem item in tree.Add(menuPath, quest))
                onItemAdded?.Invoke(item);
            added.Add(quest);

            if (!childrenByParent.TryGetValue(quest, out var children) || children == null || children.Count == 0)
                return;

            pathVisited.Add(quest);

            foreach (QuestData child in children)
            {
                AddQuestRecursive(
                    tree,
                    $"{menuPath}/{child.name}",
                    child,
                    childrenByParent,
                    added,
                    pathVisited,
                    onItemAdded);
            }

            pathVisited.Remove(quest);
        }

        private static IEnumerable<QuestData> GetSubQuestsViaSerializedObject(QuestData quest)
        {
            if (!quest)
                yield break;

            SerializedObject so = new(quest);

            SerializedProperty prop =
                so.FindProperty("SubQuests") ??
                so.FindProperty("subQuests") ??
                so.FindProperty("_subQuests");

            if (prop == null)
            {
                SerializedProperty it = so.GetIterator();
                bool enterChildren = true;

                while (it.NextVisible(enterChildren))
                {
                    enterChildren = false;

                    if (!it.isArray) continue;
                    if (it.propertyType == SerializedPropertyType.String) continue;
                    if (it.arraySize <= 0) continue;

                    SerializedProperty first = it.GetArrayElementAtIndex(0);
                    if (first != null &&
                        first.propertyType == SerializedPropertyType.ObjectReference &&
                        (!first.objectReferenceValue || first.objectReferenceValue is QuestData))
                    {
                        prop = it.Copy();
                        break;
                    }
                }
            }

            if (prop == null || !prop.isArray)
                yield break;

            for (int i = 0; i < prop.arraySize; i++)
            {
                SerializedProperty element = prop.GetArrayElementAtIndex(i);
                if (element == null) continue;

                if (element.propertyType == SerializedPropertyType.ObjectReference)
                    yield return element.objectReferenceValue as QuestData;
            }
        }
    }
}