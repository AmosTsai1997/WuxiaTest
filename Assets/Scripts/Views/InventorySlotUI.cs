using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 掛在 ItemSlot Prefab 上：負責「一格」的顯示
public class InventorySlotUI : MonoBehaviour
{
    public Image icon;                 // 道具圖示
    public TextMeshProUGUI countText;  // 數量文字

    public void SetItem(InventorySystem.ItemStack stack)
    {
        icon.sprite = stack.data.icon;
        icon.enabled = true;
        // 只有 1 個就不顯示數字（畫面乾淨）
        countText.text = stack.count > 1 ? stack.count.ToString() : "";
    }

    public void Clear()
    {
        icon.enabled = false; // 關掉 Image 而不是整個物件，格子底框還在
        countText.text = "";
    }
}