// Done
using Data.SkillData;
using JetBrains.Annotations;
using Managers;
using Sirenix.OdinInspector;
using UI.SkillTree.Connections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Enums;

namespace UI.SkillTree.Nodes
{
    public class UiTreeNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [Header("Unlock Details")]
        [SerializeField] private UiTreeNode[] neededNodes;

        [SerializeField] private UiTreeNode[] conflictingNodes;

        [Header("Skill Details")]
        [OnValueChanged(nameof(SetSkillName))]
        [SerializeField] private SkillData skillData;

        [SerializeField, ReadOnly, UsedImplicitly]
        private string skillName;

        [SerializeField, ReadOnly, UsedImplicitly, PreviewField(64, ObjectFieldAlignment.Left)]
        private Sprite previewIcon;

        [SerializeField, ReadOnly, UsedImplicitly]
        private int cost;
        [SerializeField] private string lockedHexColor = "#8C8C8C";
        [field: SerializeField] public bool IsUnlocked { get; set; }
        [field: SerializeField] public bool IsLocked { get; set; }

        [SerializeField] private Image skillIcon;
        private Color _lastColor;
        private RectTransform _rect;
        private SkillTree.Core.SkillTree _skillTree;
        private TreeConnectHandler _treeConnectHandler;

        public SkillData SkillData
        {
            get => skillData;
            set => skillData = value;
        }

        public UiTreeNode[] NeededNodes => neededNodes;
        public UiTreeNode[] ConflictingNodes => conflictingNodes;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _skillTree = GetComponentInParent<SkillTree.Core.SkillTree>();
            _treeConnectHandler = GetComponent<TreeConnectHandler>();
            UpdateIconColor(GetColorByHex(lockedHexColor));
        }

        private void Start()
        {
            UpdateConnectionAvailability();
            
            if (skillData.UnlockedByDefault)
                Unlock();
        }

        public void SetSkillName()
        {
            if (skillData)
            {
                gameObject.name = "TreeNode - " + skillData.DisplayName;
                skillName = skillData.DisplayName;
                skillIcon.sprite = skillData.Icon;
                previewIcon = skillData.Icon;
                cost = skillData.Cost;
            }
            else
            {
                gameObject.name = "TreeNode - Empty";
                skillName = "";
                skillIcon.sprite = null;
                previewIcon = null;
                cost = 0;
            }
        }

        public void Refund()
        {
            if (!IsUnlocked || skillData.UnlockedByDefault)
                return;

            //_skillTree.AddSkillPoints(skillData.Cost);
            SkillManager.Instance.AddSkillPoints(skillData.Cost);

            IsUnlocked = false;
            IsLocked = false;
            UpdateIconColor(GetColorByHex(lockedHexColor));

            _treeConnectHandler.UnlockConnectionImage(false);
        }

        private void Unlock()
        {
            if(IsUnlocked)
                return;
            
            IsUnlocked = true;
            UpdateIconColor(Color.white);
            LockConflictingNodes();
            //_skillTree.RemoveSkillPoints(skillData.Cost);
            SkillManager.Instance.RemoveSkillPoints(skillData.Cost);
            _treeConnectHandler.UnlockConnectionImage(true);

            // After unlocking, update ALL nodes to refresh their states
            UpdateAllNodeAvailability();

            SkillManager.Instance.GetSkillByType(skillData.SkillType).UpgradeSkill(skillData);
        }

        public void UnlockWithSaveData()
        {
            if (IsUnlocked || skillData.UnlockedByDefault)
                return;

            IsUnlocked = true;
            UpdateIconColor(Color.white);
            LockConflictingNodes();
            _treeConnectHandler.UnlockConnectionImage(true);
        }
        
        private bool CanBeUnlocked()
        {
            if (IsLocked || IsUnlocked)
                return false;

            if (!_skillTree.HasEnoughSkillPoints(skillData.Cost))
                return false;

            foreach (UiTreeNode node in neededNodes)
            {
                if (!node.IsUnlocked)
                    return false;
            }

            foreach (UiTreeNode node in conflictingNodes)
            {
                if (node.IsUnlocked)
                    return false;
            }

            return true;
        }

        public void UpdateConnectionAvailability()
        {
            // Connection should be white if the node is unlocked OR can be unlocked
            bool shouldBeWhite = IsUnlocked || CanBeUnlocked();
            _treeConnectHandler.UnlockConnectionImage(shouldBeWhite);
        }

        private void UpdateAllNodeAvailability()
        {
            // Find all tree nodes and update their connection availability
            UiTreeNode[] allNodes = _skillTree.GetComponentsInChildren<UiTreeNode>();
            foreach (UiTreeNode node in allNodes)
            {
                node.UpdateConnectionAvailability();
            }
        }

        private void LockConflictingNodes()
        {
            foreach (UiTreeNode node in conflictingNodes)
            {
                node.IsLocked = true;
                node.LockChildNodes();
                // Update the locked node's connection to turn gray
                node.UpdateConnectionAvailability();
            }
        }

        public void LockChildNodes()
        {
            IsLocked = true;

            foreach (UiTreeNode node in _treeConnectHandler.GetChildNodes())
                node.LockChildNodes();
        }

        private void UpdateIconColor(Color color)
        {
            if (!skillIcon)
                return;

            _lastColor = skillIcon.color;
            skillIcon.color = color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UiManager.Instance.SkillTooltip.ShowTooltip(true, _rect, skillData, this);

            if (!IsUnlocked && !IsLocked)
                ToggleNodeHighlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UiManager.Instance.SkillTooltip.ShowTooltip(false, null);
            UiManager.Instance.SkillTooltip.StopLockedSkillEffect();

            if (!IsUnlocked && !IsLocked)
                ToggleNodeHighlight(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("OnPointerDown: " + skillData.DisplayName);
            if (CanBeUnlocked())
                Unlock();
            else if (IsLocked)
                UiManager.Instance.SkillTooltip.LockedSkillEffect();
        }

        private Color GetColorByHex(string hex) =>
            ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.white;

        private void ToggleNodeHighlight(bool highlight)
        {
            Color highlightColor = Color.white * 0.9f;
            highlightColor.a = 1f;
            Color colorToApply = highlight ? highlightColor : _lastColor;
            UpdateIconColor(colorToApply);
        }

        public void AlignToParent(NodeDirectionType direction)
        {
            // Ensure _rect is initialized (important for editor context menu calls)
            if (_rect == null)
                _rect = GetComponent<RectTransform>();

            // Ensure _treeConnectHandler is initialized
            if (_treeConnectHandler == null)
                _treeConnectHandler = GetComponent<TreeConnectHandler>();

            // Find the parent node by looking for a TreeConnection that points to this node
            TreeConnection[] allConnections = FindObjectsByType<TreeConnection>(FindObjectsSortMode.None);
            TreeConnection parentConnection = null;

            foreach (TreeConnection connection in allConnections)
            {
                if (connection.ConnectionDetails?.ChildNode == _treeConnectHandler)
                {
                    parentConnection = connection;
                    break;
                }
            }

            if (parentConnection == null)
            {
                Debug.LogWarning($"No parent connection found for {gameObject.name}");
                return;
            }

            // Get the parent node's RectTransform
            RectTransform parentRect = parentConnection.transform.parent as RectTransform;
            if (parentRect == null)
            {
                Debug.LogWarning($"Parent RectTransform not found for {gameObject.name}");
                return;
            }

            // Get current position
            Vector2 currentPosition = _rect.anchoredPosition;
            Vector2 parentPosition = parentRect.anchoredPosition;

            // Get the total distance from the connection (already calculated)
            float totalDistance = parentConnection.ConnectionDetails.Length;

            // Align based on direction
            switch (direction)
            {
                case NodeDirectionType.Left:
                    // Place to the left of parent, maintain total distance, align vertically
                    _rect.anchoredPosition = new Vector2(parentPosition.x - totalDistance, parentPosition.y);
                    break;

                case NodeDirectionType.Right:
                    // Place to the right of parent, maintain total distance, align vertically
                    _rect.anchoredPosition = new Vector2(parentPosition.x + totalDistance, parentPosition.y);
                    break;

                case NodeDirectionType.Up:
                    // Place above parent, maintain total distance, align horizontally
                    _rect.anchoredPosition = new Vector2(parentPosition.x, parentPosition.y + totalDistance);
                    break;

                case NodeDirectionType.Down:
                    // Place below parent, maintain total distance, align horizontally
                    _rect.anchoredPosition = new Vector2(parentPosition.x, parentPosition.y - totalDistance);
                    break;

                case NodeDirectionType.UpRight:
                    // 45-degree angle: split distance equally between X and Y
                    float diagonal = totalDistance / Mathf.Sqrt(2);
                    _rect.anchoredPosition = new Vector2(parentPosition.x + diagonal, parentPosition.y + diagonal);
                    break;

                case NodeDirectionType.UpLeft:
                    diagonal = totalDistance / Mathf.Sqrt(2);
                    _rect.anchoredPosition = new Vector2(parentPosition.x - diagonal, parentPosition.y + diagonal);
                    break;

                case NodeDirectionType.DownRight:
                    diagonal = totalDistance / Mathf.Sqrt(2);
                    _rect.anchoredPosition = new Vector2(parentPosition.x + diagonal, parentPosition.y - diagonal);
                    break;

                case NodeDirectionType.DownLeft:
                    diagonal = totalDistance / Mathf.Sqrt(2);
                    _rect.anchoredPosition = new Vector2(parentPosition.x - diagonal, parentPosition.y - diagonal);
                    break;
            }
        }
    }
}