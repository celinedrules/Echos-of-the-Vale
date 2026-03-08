// Done
using UnityEngine;

namespace Data.Audio
{
    [CreateAssetMenu(fileName = "AudioData", menuName = "Echos of the Vale/Audio Data/Audio Data", order = 0)]
    public class AudioData : ScriptableObject
    {
        [SerializeField] private string[] audioIds;
        
        public string[] AudioIds => audioIds;
    }
}