using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 掛在 ItemSlot Prefab 上：負責「一格」的顯示
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image icon;                 // 道具圖示
    public TextMeshProUGUI countText;  // 數量文字
    private ItemData _currentItem; // 記住這格目前放什麼（空格 = null）

    public void SetItem(InventorySystem.ItemStack stack)
    {
        SetItem(stack.data, stack.count); // 舊入口轉呼叫新版，避免重複邏輯
    }
    // 新版本：直接給道具和數量。count 可以是 0（快捷列綁著但背包缺貨）
    public void SetItem(ItemData data, int count)
    {
        _currentItem = data;
        icon.sprite = data.icon;
        icon.enabled = true;
        // 缺貨時圖示半透明，一眼看出「沒藥了」
        icon.color = count > 0 ? Color.white : new Color(1f, 1f, 1f, 0.3f);
        countText.text = count > 1 ? count.ToString() : (count == 0 ? "0" : "");
    }
    public void Clear()
    {
        _currentItem = null;
        icon.enabled = false; // 關掉 Image 而不是整個物件，格子底框還在
        countText.text = "";
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount != 2) return; // 只理「雙擊」
        if (_currentItem == null) return;      // 空格子沒事做

        // 「is」型別判斷：這個道具是不是裝備？
        // 是的話順便轉型成 equip 變數給我用
        if (_currentItem is EquipmentData equip)
        {
            EquipmentSystem.Instance.Equip(equip);
        }
        else if (_currentItem.isUsable)
        {
            // 順便送你的功能：雙擊藥水直接喝
            InventorySystem.Instance.UseItem(_currentItem);
        }
    }

}