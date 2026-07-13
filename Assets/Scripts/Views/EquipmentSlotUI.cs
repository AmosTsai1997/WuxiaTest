using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class EquipmentSlotUI:MonoBehaviour, IPointerClickHandler
{
    public EquipSlotType slotType; // 這格是哪個部位
    public Image icon;

    public void Refresh()
    { 
        EquipmentData equip = EquipmentSystem.Instance.GetEquipped(slotType);
        if (equip != null)
        {
            icon.sprite = equip.icon;
            icon.enabled = true;
        }
        else 
        {
            icon.enabled =false;    // 空部位不顯示圖示
        }

    }
    public void OnPointerClick(PointerEventData eventData) 
    {
        if (eventData.clickCount != 2) return;
        EquipmentSystem.Instance.Unequip(slotType);
    }
}
