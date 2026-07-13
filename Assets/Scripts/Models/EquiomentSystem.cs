using UnityEngine;
using System.Collections.Generic;

public class EquipmentSystem : MonoBehaviour
{ 
    public static EquipmentSystem Instance;

    private Dictionary<EquipSlotType, EquipmentData> _equipped = new Dictionary<EquipSlotType, EquipmentData>();

    public event System.Action OnEquipmentChanged; // 穿脫時廣播，UI 訂閱刷新

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    public EquipmentData GetEquipped(EquipSlotType slot)
    {
        if (_equipped.ContainsKey(slot)) return _equipped[slot];
        return null;
    }

    public void Equip(EquipmentData equip)
    {
        if (equip == null) return;

        // 1. 先從背包扣掉這件（不在背包裡就不給穿，防呆）
        bool removed = InventorySystem.Instance.RemoveItem(equip);
        if (!removed) return;

        // 2. 該部位已經有穿東西 → 舊的放回背包（換裝）
        EquipmentData old = GetEquipped(equip.slotType);
        if (old != null)
        {
            InventorySystem.Instance.AddItem(old);
        }

        // 3. 記帳 + 廣播
        _equipped[equip.slotType] = equip;
        OnEquipmentChanged?.Invoke();
        Debug.Log($"裝備了 {equip.itemName}（{equip.slotType}）");
    }

    // 脫裝備：放回背包
    public void Unequip(EquipSlotType slot)
    {
        EquipmentData old = GetEquipped(slot);
        if (old == null) return;
        bool ok = InventorySystem.Instance.AddItem(old);
        if (!ok) { Debug.Log("背包滿了，脫不下來！"); return; }

        _equipped[slot] = null;
        OnEquipmentChanged?.Invoke();
    }
}