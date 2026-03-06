using UnityEngine;

namespace Data.QuestData
{
    [CreateAssetMenu(fileName = "List of Quests - ", menuName = "Echos of the Vale/Quest Data/Quest List", order = 0)]
    public class QuestListData : ScriptableObject
    {
        [SerializeField] protected QuestData[] questList;

        public QuestData[] QuestList => questList;
    }
}