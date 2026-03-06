// Done
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Data.QuestData
{
    [CreateAssetMenu(fileName = "Quest Database", menuName = "Echos of the Vale/Quest Data/Quest Database")]
    public class QuestDatabase : QuestListData
    {
        private Dictionary<string, QuestData> _questLookup;

        public QuestData GetQuestByID(string saveId)
        {
            if (_questLookup == null)
                BuildLookup();

            return _questLookup.GetValueOrDefault(saveId);
        }

        private void BuildLookup()
        {
            _questLookup = questList
                .Where(q => q != null && !string.IsNullOrEmpty(q.saveId))
                .ToDictionary(q => q.saveId);
        }

        private void OnEnable() => _questLookup = null;

#if UNITY_EDITOR
        [ContextMenu("Auto-fill will all QuestData")]
        [PropertyOrder(-1)]
        [Button("Auto-fill will all QuestData", ButtonSizes.Gigantic)]
        public void CollectQuestsData()
        {
            string[] guids = AssetDatabase.FindAssets("t:QuestData");

            questList = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<QuestData>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(questData => questData).ToArray();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}