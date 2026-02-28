using UnityEngine;

namespace Data.WeaponData
{
    [CreateAssetMenu(fileName = "Sword Data - ", menuName = "Echos of the Vale/Weapon Data/Sword", order = 0)]
    public class SwordData : ScriptableObject
    {
        [field: SerializeField] public string SwordName { get; private set; }
        [field: SerializeField] public RuntimeAnimatorController AnimatorController { get; private set; }
    }
}