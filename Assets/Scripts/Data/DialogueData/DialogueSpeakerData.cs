// Done
using UnityEngine;

namespace Data.DialogueData
{
    [CreateAssetMenu(fileName = "Speaker - ", menuName = "Echos of the Vale/Dialogue Data/Speaker Data")]
    public class DialogueSpeakerData : ScriptableObject
    {
        [SerializeField] private Sprite speakerPortrait;
        [SerializeField] private string speakerName;
        
        public Sprite SpeakerPortrait { get => speakerPortrait; set => speakerPortrait = value; }
        public string SpeakerName { get => speakerName; set => speakerName = value; }
        
    }
}