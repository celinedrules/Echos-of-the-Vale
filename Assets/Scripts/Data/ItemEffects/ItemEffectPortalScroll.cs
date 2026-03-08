using Interactables;
using Managers;
using UnityEngine;

namespace Data.ItemEffects
{
    [CreateAssetMenu(fileName = "Item Effect - Portal Scroll", menuName = "Echos of the Vale/Item Data/Item Effect/Portal Scroll", order = 0)]
    public class ItemEffectPortalScroll : ItemEffectData
    {
        public override void ExecuteEffect()
        {
            Player = GameManager.Instance.Player;
            Vector3 portalPosition = Player.transform.position + new Vector3((int)Player.FacingDirection * 1.5f, 0);
            
            Portal.Instance.ActivatePortal(portalPosition, Player.FacingDirection);
        }
    }
}