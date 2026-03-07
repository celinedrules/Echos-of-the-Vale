// Done
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UI.SkillTree.Nodes;
using UnityEngine;
using UnityEngine.UI;

namespace UI.SkillTree.Connections
{
    [ExecuteInEditMode]
    [Serializable]
    public class TreeConnectHandler : MonoBehaviour
    {
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [SerializeField] private List<TreeConnection> connections = new List<TreeConnection>();

        [SerializeField] private GameObject connectionPrefab;
        [SerializeField] private GameObject treeNodePrefab;
        
        private RectTransform _myRect;
        private Image _connectionImage;
        private List<Image> _parentConnectionImages = new();

        private void OnEnable()
        {
            if (_myRect == null)
                _myRect = GetComponent<RectTransform>();
        }

        private void OnValidate()
        {
            if (_myRect == null)
                _myRect = GetComponent<RectTransform>();
        }
        
        private void Awake()
        {
            if (_myRect == null)
                _myRect = GetComponent<RectTransform>();
        }
        
        public UiTreeNode[] GetChildNodes()
        {
            List<UiTreeNode> result = new List<UiTreeNode>();

            foreach (TreeConnection node in connections)
            {
                if (node.ConnectionDetails.ChildNode != null)
                    result.Add(node.ConnectionDetails.ChildNode.GetComponent<UiTreeNode>());
            }

            return result.ToArray();
        }

        public void SetPosition(Vector2 position)
        {
            if (_myRect)
                _myRect.anchoredPosition = position;
        }

        public void SetConnectionImage(Image image)
        {
            if (image != null && !_parentConnectionImages.Contains(image))
            {
                _parentConnectionImages.Add(image);
            }
        }

        public void UnlockConnectionImage(bool unlocked)
        {
            // Update parent connections (connections leading TO this node)
            foreach (Image img in _parentConnectionImages)
            {
                if (img != null)
                {
                    img.color = unlocked ? Color.white : GetOriginalColor();
                }
            }
        }

        private Color GetOriginalColor()
        {
            // Return a default gray color for locked state
            return new Color(0.55f, 0.55f, 0.55f, 1f);
        }

        [Button("Add New Connection")]
        public void AddNewConnection()
        {
#if UNITY_EDITOR
            TreeGraphOperations.AddChild(this, treeNodePrefab, connectionPrefab, new Vector2(0f, -150f));
#endif
        }

        [Button("Remove Skill")]
        public void RemoveSkill()
        {
#if UNITY_EDITOR
            TreeGraphOperations.RemoveSubtree(this);
#endif
        }

#if UNITY_EDITOR
        internal void AddConnectionInternal(TreeConnection connection)
        {
            if (connection == null)
                return;

            connections ??= new List<TreeConnection>();

            if (!connections.Contains(connection))
                connections.Add(connection);
        }

        internal TreeConnection[] GetConnectionsInternalSnapshot()
        {
            return connections != null ? connections.ToArray() : Array.Empty<TreeConnection>();
        }

        internal void RemoveConnectionInternal(TreeConnection connection)
        {
            if (connections == null || connection == null)
                return;

            connections.Remove(connection);
        }
#endif
        
        public void UpdateAllConnections()
        {
            foreach (TreeConnection connection in connections)
            {
                if (connection != null)
                {
                    connection.UpdateLayout();

                    // Recursively update child nodes
                    if (connection.ConnectionDetails?.ChildNode != null)
                    {
                        connection.ConnectionDetails.ChildNode.UpdateAllConnections();
                    }
                }
            }
        }
    }
}