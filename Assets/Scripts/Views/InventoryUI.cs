using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public TextMeshProUGUI goldText; // 金幣數字（可先留空）

    [Header("格子設定")]
    public int slotCount = 50;

    private InventorySlotUI[] _slots; // 記住每一格，刷新時用

    void Start()
    {
        _slots = new InventorySlotUI[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, inventoryPanel.transform);
            slot.name = "Slot_" + i;
            _slots[i] = slot.GetComponent<InventorySlotUI>();
            _slots[i].Clear();
        }

        // 訂閱背包廣播：內容一變，自動刷新畫面
        InventorySystem.Instance.OnInventoryChanged += Refresh;
        Refresh();

        inventoryPanel.SetActive(false);
    }

    void OnDestroy()
    {
        // 退訂，成對出現的鐵律
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged -= Refresh;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    void Refresh()
    {
        var items = InventorySystem.Instance.Items;
        for (int i = 0; i < _slots.Length; i++)
        {
            if (i < items.Count) _slots[i].SetItem(items[i]);
            else _slots[i].Clear();
        }
        if (goldText != null) goldText.text = "金幣：" + InventorySystem.Instance.gold;
    }
}