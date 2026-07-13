using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    // 一個「堆疊」= 哪種道具 x 幾個。
    // [System.Serializable] 讓這個自訂類別能顯示在 Inspector（除錯方便）
    [System.Serializable]
    public class ItemStack
    {
        public ItemData data;
        public int count;
    }

    [Header("背包狀態")]
    public int gold = 0;       // 金幣不佔格子，直接是一個數字
    public int capacity = 50;  // 跟你的 5x10 道具欄一致

    private List<ItemStack> _items = new List<ItemStack>();
    public List<ItemStack> Items => _items; // 給 UI 讀取用的唯讀出口

    // 廣播電台：背包內容有變就通知（UI 會訂閱它來刷新畫面）
    // —— 跟 Health.OnDeath 是同一套模式
    public event System.Action OnInventoryChanged;

    void Awake()
    {
        // 單例查重放最前面，老規矩
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"獲得 {amount} 金幣，目前共 {gold}");
        OnInventoryChanged?.Invoke(); // 改完資料就廣播 —— 經驗條的教訓
    }

    // 回傳 bool：撿取方需要知道「背包滿了沒收成功」
    public bool AddItem(ItemData item)
    {
        // 先找同種道具的現有堆疊，有就 +1（疊加）
        foreach (ItemStack s in _items)
        {
            if (s.data == item)
            {
                s.count++;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        // 沒有現成堆疊 → 要開新格子，先檢查容量
        if (_items.Count >= capacity)
        {
            Debug.Log("背包滿了，撿不起來！");
            return false;
        }

        _items.Add(new ItemStack { data = item, count = 1 });
        OnInventoryChanged?.Invoke();
        return true;
    }
    // 從背包移除一個指定道具（裝備系統穿裝時用）
    public bool RemoveItem(ItemData item)
    {
        foreach (ItemStack s in _items)
        {
            if (s.data == item)
            {
                s.count--;
                if (s.count <= 0) _items.Remove(s); // 移除後立刻 return，
                OnInventoryChanged?.Invoke();        // 不會繼續迭代所以安全
                return true;
            }
        }
        return false; // 背包裡沒這個東西
    }
    // 查詢某種道具目前的庫存數量（快捷列顯示用）
    public int GetItemCount(ItemData item)
    {
        foreach (ItemStack s in _items)
        {
            if (s.data == item) return s.count;
        }
        return 0; // 沒找到 = 0 個
    }

    public void UseItem(ItemData item)
    {
        // 1. 找庫存
        if (item == null || !item.isUsable) return;
        ItemStack stack = null;
        foreach (ItemStack s in _items)
        {
            if (s.data == item) { stack = s; break; }
        }
        if (stack == null)
        {
            Debug.Log($"背包裡沒有 {item.itemName}！");
            return;
        }

        // 2. 發動效果（回血 / 攻速 Buff；以後加新藥水就在這裡擴充）
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // 2a. 回血類
        if (item.healAmount > 0)
        {
            Health h = player.GetComponent<Health>();
            if (h != null)
            {
                if (h.currentHP >= h.maxHP)
                {
                    Debug.Log("血量已滿，不浪費藥水");
                    return; // 直接離開 —— 不扣庫存
                }
                h.Heal(item.healAmount);
            }
        }

        // 2b. 攻速 Buff 類（攻速藥水：attackSpeedBonus 0.3 = +30%）
        if (item.attackSpeedBonus > 0)
        {
            HeroActions hero = player.GetComponent<HeroActions>();
            if (hero != null)
            {
                hero.ApplyAttackSpeedBuff(item.attackSpeedBonus, item.buffDuration);
            }
        }

        // 3. 扣庫存，用完移除堆疊
        stack.count--;
        if (stack.count <= 0) _items.Remove(stack);

        OnInventoryChanged?.Invoke(); // 廣播 → 背包和快捷列都會自動刷新
    }
}