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
        SetItem(stack.data, stack.count); // 舊入口轉呼叫新版，避免重複邏輯
    }
    // 新版本：直接給道具和數量。count 可以是 0（快捷列綁著但背包缺貨）
    public void SetItem(ItemData data, int count)
    {
        icon.sprite = data.icon;
        icon.enabled = true;
        // 缺貨時圖示半透明，一眼看出「沒藥了」
        icon.color = count > 0 ? Color.white : new Color(1f, 1f, 1f, 0.3f);
        countText.text = count > 1 ? count.ToString() : (count == 0 ? "0" : "");
    }
    public void Clear()
    {
        icon.enabled = false; // 關掉 Image 而不是整個物件，格子底框還在
        countText.text = "";
    }
}