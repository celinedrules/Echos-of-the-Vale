// Done
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.EntityData
{
    public abstract class EntityData : SerializedScriptableObject
    {
        [HorizontalGroup("Split", 64, LabelWidth = 67)]
        [SerializeField, HideLabel, PreviewField(64, ObjectFieldAlignment.Left)]
        private Sprite icon;

        [VerticalGroup("Split/Meta")]
        [OnValueChanged(nameof(ChangeName))]
        [Delayed]
        [SerializeField, LabelText("Name")] private string entityName;

        [FormerlySerializedAs("entityStatsData")]
        [HideInInspector]
        [SerializeField] private EntityStatsData statsData;

        public virtual EntityStatsData StatsData => statsData;
        
        public Sprite Icon => icon;
        public string EntityName
        {
            get => entityName;
            set => entityName = value;
        }
        

        private void ChangeName()
        {
            name = entityName;

#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            if (string.IsNullOrWhiteSpace(entityName))
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

                AssetDatabase.RenameAsset(currentPath, entityName);
                AssetDatabase.SaveAssets();
            };
#endif
        }
    }
}