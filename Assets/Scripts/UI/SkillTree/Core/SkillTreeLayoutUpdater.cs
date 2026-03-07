// Done
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UI.SkillTree.Connections;
using UnityEditor;
using UnityEngine;

namespace UI.SkillTree.Core
{
    [ExecuteAlways]
    public class SkillTreeLayoutUpdater : MonoBehaviour
    {
        [SerializeField] private bool refreshOnEnable = true;

#if UNITY_EDITOR
        [SerializeField] private bool autoRefreshWhileDragging = true;
        private bool _refreshQueued;

        private readonly List<TreeConnectHandler> _nodes = new();
        private readonly List<RectTransform> _nodeRects = new();
        private bool _cacheDirty = true;
#endif

        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.update += EditorTick;
            _cacheDirty = true;
#endif
            if (refreshOnEnable)
                RefreshAllConnections();
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorTick;
            EditorApplication.update -= PerformRefresh;
            _refreshQueued = false;

            _nodes.Clear();
            _nodeRects.Clear();
#endif
        }

        private void OnTransformChildrenChanged()
        {
#if UNITY_EDITOR
            _cacheDirty = true;
#endif
        }

#if UNITY_EDITOR
        private void EnsureCache()
        {
            if (!_cacheDirty)
                return;

            _cacheDirty = false;

            _nodes.Clear();
            _nodeRects.Clear();

            GetComponentsInChildren(includeInactive: true, result: _nodes);
            
            foreach (TreeConnectHandler handler in _nodes)
            {
                RectTransform rt = handler != null ? handler.GetComponent<RectTransform>() : null;
                _nodeRects.Add(rt);
            }
        }

        private void EditorTick()
        {
            if (!autoRefreshWhileDragging)
                return;

            if (Application.isPlaying)
                return;

            EnsureCache();

            bool anyChanged = false;
            
            foreach (RectTransform rt in _nodeRects)
            {
                if (rt == null)
                {
                    _cacheDirty = true;
                    continue;
                }

                if (!rt.hasChanged)
                    continue;

                rt.hasChanged = false;
                anyChanged = true;
            }

            if (anyChanged)
                QueueRefresh();
        }

        public void RequestRefresh()
        {
            if (Application.isPlaying)
                return;

            _cacheDirty = true;
            RefreshAllConnections();
        }
        
        private void QueueRefresh()
        {
            if (_refreshQueued)
                return;

            _refreshQueued = true;
            EditorApplication.update += PerformRefresh;
        }

        private void PerformRefresh()
        {
            EditorApplication.update -= PerformRefresh;
            _refreshQueued = false;

            if (this == null)
                return;

            if (Application.isPlaying)
                return;

            RefreshAllConnections();
        }
#endif

        [Button("Refresh All Connections")]
        public void RefreshAllConnections()
        {
            TreeConnection[] connections = GetComponentsInChildren<TreeConnection>(includeInactive: true);

            foreach (TreeConnection c in connections)
            {
                if (c != null)
                    c.UpdateLayout();
            }
        }
    }
}