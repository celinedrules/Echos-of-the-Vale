// Done
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UI.SkillTree.Core;
using UI.SkillTree.Nodes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.SkillTree.Connections
{
    /// <summary>
    /// Editor-only operations on the skill-tree graph stored as scene GameObjects/components.
    /// Keeps destructive algorithms (delete/unlink) out of MonoBehaviours.
    /// </summary>
    public static class TreeGraphOperations
    {
        public static TreeConnectHandler AddChild(
            TreeConnectHandler parent,
            GameObject treeNodePrefab,
            GameObject connectionPrefab,
            Vector2 anchoredOffsetFromParent)
        {
            if (parent == null)
                return null;

            if (Application.isPlaying)
            {
                Debug.LogWarning($"{nameof(AddChild)} is intended for edit-time use.");
                return null;
            }

            if (treeNodePrefab == null || connectionPrefab == null)
            {
                Debug.LogError($"{nameof(TreeGraphOperations)}.{nameof(AddChild)}: Prefabs not assigned on '{parent.name}'.");
                return null;
            }

            Transform nodeParent = parent.transform.parent != null ? parent.transform.parent : parent.transform;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Add Skill Tree Child");
            int undoGroup = Undo.GetCurrentGroup();

            GameObject newNode =
                PrefabUtility.InstantiatePrefab(treeNodePrefab, nodeParent) as GameObject
                ?? UnityEngine.Object.Instantiate(treeNodePrefab, nodeParent);
            
            GameObject newConnection =
                PrefabUtility.InstantiatePrefab(connectionPrefab, parent.transform) as GameObject
                ?? UnityEngine.Object.Instantiate(connectionPrefab, parent.transform);

            Undo.RegisterCreatedObjectUndo(newNode, "Add Skill Tree Node");
            Undo.RegisterCreatedObjectUndo(newConnection, "Add Skill Tree Connection");
            Undo.RecordObject(parent, "Add Skill Tree Connection (List)");

            newConnection.transform.SetAsFirstSibling();

            // UI-friendly positioning when possible
            RectTransform parentRect = parent.GetComponent<RectTransform>();
            RectTransform newNodeRect = newNode.GetComponent<RectTransform>();
            if (parentRect != null && newNodeRect != null)
            {
                newNodeRect.anchoredPosition = parentRect.anchoredPosition + anchoredOffsetFromParent;
            }
            else
            {
                newNode.transform.position = parent.transform.position + new Vector3(anchoredOffsetFromParent.x, anchoredOffsetFromParent.y, 0f);
            }

            TreeConnection connectionComp = newConnection.GetComponent<TreeConnection>();
            TreeConnectHandler childHandler = newNode.GetComponent<TreeConnectHandler>();

            if (connectionComp == null || childHandler == null)
            {
                Debug.LogError($"{nameof(AddChild)}: Prefab is missing required components (TreeConnection / TreeConnectHandler).");
                Undo.CollapseUndoOperations(undoGroup);
                return null;
            }

            // IMPORTANT: record modifications on the TreeConnection itself before changing its serialized data
            Undo.RecordObject(connectionComp, "Wire Skill Tree Connection");
            connectionComp.ConnectionDetails.ChildNode = childHandler;

            // Persist the nested serialized change on prefab instances
            EditorUtility.SetDirty(connectionComp);
            PrefabUtility.RecordPrefabInstancePropertyModifications(connectionComp);

            parent.AddConnectionInternal(connectionComp);
            EditorUtility.SetDirty(parent);
            PrefabUtility.RecordPrefabInstancePropertyModifications(parent);
            
            UiTreeNode parentUiNode = parent.GetComponent<UiTreeNode>();
            UiTreeNode childUiNode = childHandler.GetComponent<UiTreeNode>();
            if (parentUiNode != null && childUiNode != null && parentUiNode.SkillData != null)
            {
                Undo.RecordObject(childUiNode, "Copy SkillData to Child");
                childUiNode.SkillData = parentUiNode.SkillData;
                childUiNode.SetSkillName();  // Update the display name/icon
                EditorUtility.SetDirty(childUiNode);
                PrefabUtility.RecordPrefabInstancePropertyModifications(childUiNode);
            }

            // Force one layout pass immediately (optional but nice)
            connectionComp.InitializeAfterWiring();

            // Mark scene dirty so the edit is committed (this is the part recompilation was effectively doing for you)
            if (!EditorApplication.isPlaying)
            {
                Scene scene = parent.gameObject.scene;
                if (scene.IsValid())
                    EditorSceneManager.MarkSceneDirty(scene);
            }

            EditorUtility.SetDirty(parent);
            EditorUtility.SetDirty(newConnection);
            EditorUtility.SetDirty(newNode);

            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = newNode;

            SkillTreeLayoutUpdater updater = parent.GetComponentInParent<SkillTreeLayoutUpdater>();
            if (updater != null)
                updater.RequestRefresh();
            
            return childHandler; 
        }
        
        public static void RemoveSubtree(TreeConnectHandler root)
        {
            if (root == null)
                return;

            if (Application.isPlaying)
            {
                Debug.LogWarning($"{nameof(RemoveSubtree)} is intended for edit-time use.");
                return;
            }

            // Choose something sensible to select after deletion
            GameObject selectionAfter = FindParentNodeGameObject(root);

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Remove Skill Node (Recursive)");
            int undoGroup = Undo.GetCurrentGroup();

            HashSet<TreeConnectHandler> visited = new HashSet<TreeConnectHandler>();
            DeleteSubtreeRecursive(root, visited);

            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = selectionAfter;
        }

        private static void DeleteSubtreeRecursive(TreeConnectHandler node, HashSet<TreeConnectHandler> visited)
        {
            if (node == null)
                return;

            // Prevent infinite loops if someone accidentally creates cycles.
            if (!visited.Add(node))
                return;

            // 1) Detach & destroy outgoing connections first, then recurse into children.
            // This avoids the "child deletes incoming parent-connection while parent is iterating" MissingReferenceException.
            TreeConnection[] myConnections = node.ConnectionsSnapshot();

            foreach (TreeConnection connection in myConnections)
            {
                if (connection == null)
                    continue;

                TreeConnectHandler child = connection.ConnectionDetails?.ChildNode;

                // Remove from list before destroying the object
                Undo.RecordObject(node, "Remove Connection (List)");
                node.RemoveConnectionReference(connection);
                EditorUtility.SetDirty(node);

                Undo.DestroyObjectImmediate(connection.gameObject);

                if (child != null)
                    DeleteSubtreeRecursive(child, visited);
            }

            // 2) Unlink node from any parent connections pointing TO this node
            // (removes it from parent's list and destroys the parent->node connection object).
            TreeConnection[] allConnections = UnityEngine.Object.FindObjectsByType<TreeConnection>(FindObjectsSortMode.None);
            foreach (TreeConnection parentConnection in allConnections)
            {
                if (parentConnection == null)
                    continue;

                if (parentConnection.ConnectionDetails?.ChildNode != node)
                    continue;

                TreeConnectHandler parentHandler = parentConnection.GetComponentInParent<TreeConnectHandler>();
                if (parentHandler != null)
                {
                    Undo.RecordObject(parentHandler, "Remove Connection (From Parent List)");
                    parentHandler.RemoveConnectionReference(parentConnection);
                    EditorUtility.SetDirty(parentHandler);
                }

                Undo.DestroyObjectImmediate(parentConnection.gameObject);
            }

            // 3) Finally delete the node itself
            Undo.DestroyObjectImmediate(node.gameObject);
        }

        private static GameObject FindParentNodeGameObject(TreeConnectHandler node)
        {
            if (node == null)
                return null;

            TreeConnection[] allConnections = UnityEngine.Object.FindObjectsByType<TreeConnection>(FindObjectsSortMode.None);
            foreach (TreeConnection connection in allConnections)
            {
                if (connection == null)
                    continue;

                if (connection.ConnectionDetails?.ChildNode != node)
                    continue;

                TreeConnectHandler parentHandler = connection.GetComponentInParent<TreeConnectHandler>();
                if (parentHandler != null)
                    return parentHandler.gameObject;
            }

            return null;
        }
    }

    internal static class TreeConnectHandlerExtensions
    {
        /// <summary>Returns a safe snapshot of outgoing connections to iterate over.</summary>
        internal static TreeConnection[] ConnectionsSnapshot(this TreeConnectHandler handler)
        {
            if (handler == null)
                return Array.Empty<TreeConnection>();

            // The handler owns the serialized list; snapshot avoids mutation issues during deletion.
            return handler.GetConnectionsInternalSnapshot();
        }

        internal static void RemoveConnectionReference(this TreeConnectHandler handler, TreeConnection connection)
        {
            if (handler == null || connection == null)
                return;

            handler.RemoveConnectionInternal(connection);
        }
    }
}
#endif