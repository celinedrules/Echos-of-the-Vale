// Done
using InventorySystem;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Utilities.Enums;

namespace Data.QuestData
{
    [CreateAssetMenu(fileName = "Quest - ", menuName = "Echos of the Vale/Quest Data/Quest")]
    public class QuestData : ScriptableObject
    {
        [HideInInspector]
        public string saveId;
     
        [TabGroup("Tabs", "Overview")]
        [HorizontalGroup("Tabs/Overview/Split", 64, LabelWidth = 67)]
        [SerializeField, HideLabel, PreviewField(64, ObjectFieldAlignment.Left)]
        private Sprite icon;
    
        [VerticalGroup("Tabs/Overview/Split/Meta"), Indent]
        [OnValueChanged(nameof(ChangeName))]
        [Delayed]
        [SerializeField, LabelText("Name"), LabelWidth(100)] private string questName;
    
        [VerticalGroup("Tabs/Overview/Split/Meta"), Indent]
        [SerializeField, LabelText("Type"), LabelWidth(100)] private QuestType questType;
    
        [VerticalGroup("Tabs/Overview/Split/Meta"), Indent]
        [MultiLineProperty(6), SerializeField, LabelWidth(85)] public string description;

    
        [TabGroup("Tabs", "Objectives")]
        [TextArea, SerializeField, Space] public string questGoal;
        [TabGroup("Tabs", "Objectives")]
        [SerializeField, Space] private string questTargetId;
        [TabGroup("Tabs", "Objectives")]
        [SerializeField] private string turnInTargetId;
        [HideIf("questType", QuestType.Deliver)]
        [TabGroup("Tabs", "Objectives")]
        [SerializeField] private int requiredAmount;
        [TabGroup("Tabs", "Objectives")]
        [ShowIf("questType", QuestType.Deliver)]
        [SerializeField, Space] private InventoryItem[] itemsToDeliver;

        [TabGroup("Tabs", "Dependencies")]
        [SerializeField] private bool sequentialSubQuests;
        [TabGroup("Tabs", "Dependencies")]
        [SerializeField] private QuestData[] subQuests;
    
        [TabGroup("Tabs", "Rewards")]
        [SerializeField] private RewardType rewardType;
        [TabGroup("Tabs", "Rewards")]
        [SerializeField, Space] private InventoryItem[] rewardItems;
        [TabGroup("Tabs", "Rewards")]
        [SerializeField, Space] private int goldAmount;
    
        [TabGroup("Tabs", "Debug")]
        [ReadOnly, HideLabel]
        [SerializeField] private string debugData = "Debug Data";
        
        public Sprite Icon => icon;
        public string QuestName => questName;
        public QuestType QuestType => questType;
        public InventoryItem[] RewardItems => rewardItems;
        public int RequiredAmount => requiredAmount;
        public string QuestTargetId => questTargetId;
        public InventoryItem[] ItemsToDeliver => itemsToDeliver;
        public RewardType RewardType => rewardType;
        public string TurnInTargetId => turnInTargetId;
        public QuestData[] SubQuests => subQuests;
        public bool HasSubQuests => subQuests != null && subQuests.Length > 0;
        public bool SequentialSubQuests => sequentialSubQuests;
        
        private void OnValidate()
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);
            saveId = AssetDatabase.AssetPathToGUID(path);
            
            if (questType == QuestType.Deliver)
                requiredAmount = 1;
            
            // if (questType == QuestType.Deliver && itemToDeliver != null)
            //     questTargetId = itemToDeliver.SaveId;
#endif
        }
        
        private void ChangeName()
        {
            name = questName;

#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            if (string.IsNullOrWhiteSpace(questName))
                return;

            string path = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(path))
                return;

            // Delay the rename until after the inspector GUI has finished this change event.
            EditorApplication.delayCall += () =>
            {
                if (this == null) return;

                string currentPath = AssetDatabase.GetAssetPath(this);
                if (string.IsNullOrEmpty(currentPath)) return;

                AssetDatabase.RenameAsset(currentPath, questName);
                AssetDatabase.SaveAssets();
            };
#endif
        }
    }
}