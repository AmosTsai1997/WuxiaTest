using UnityEngine;
using UnityEngine.UI; 
using TMPro;          

public class HotbarUI : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject hotbarPanel; // 快捷欄面板
    public GameObject slotPrefab;

    [Header("快捷列設定")]
    public int slotCount = 5; // 格子數量
    public Color normalColor = new Color(1f, 1f, 1f, 0.4f);
    public Color selectedColor = new Color(1f, 0.9f, 0.3f, 0.8f);

    [Header("每格綁定的道具（Inspector 拖 ItemData，空格留 None）")]
    public ItemData[] slotItems = new ItemData[5];
    private Image[] _slotImages;
    private InventorySlotUI[] _slotViews; // 圖示+數量顯示，跟背包格共用同一個腳本
    private int _selectedIndex = -1; // 目前選中第幾格，-1 代表都沒選

    void Start() 
    {
        // 開一個能裝 5 個 Image 的陣列
        _slotImages = new Image[slotCount];
        _slotViews = new InventorySlotUI[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, hotbarPanel.transform);
            slot.name = "HotbarSlot_" + (i + 1);
            // 把這格的 Image 存進陣列第 i 號位置，並設成平常色
            _slotImages[i] = slot.GetComponent<Image>();
            _slotImages[i].color = normalColor;
            _slotViews[i] = slot.GetComponent<InventorySlotUI>();

            // 格子裡現在有兩個文字（KeyText、CountText），
            // 原本的 GetComponentInChildren 只會抓「第一個找到的」，會抓錯對象 ——
            // 改用 transform.Find 按名字精準尋找
            TextMeshProUGUI keyText = slot.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            keyText.text = (i + 1).ToString();
        }
        // 訂閱背包廣播：撿到藥、用掉藥，快捷列數量自動同步
        InventorySystem.Instance.OnInventoryChanged += RefreshSlots;
        RefreshSlots();
    }

    void OnDestroy()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged -= RefreshSlots;
    }

    void Update() 
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                UseSlot(i);
            }
        }
    }
    void UseSlot(int index)
    {
        // 高亮效果照舊
        for (int i = 0; i < slotCount; i++) _slotImages[i].color = normalColor;
        _slotImages[index].color = selectedColor;
        _selectedIndex = index;

        // 這格有綁道具 → 使用它（能不能用、夠不夠，由 Model 層把關）
        if (slotItems[index] != null)
        {
            InventorySystem.Instance.UseItem(slotItems[index]);
        }
    }
    // 依背包庫存刷新每一格
    void RefreshSlots()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (slotItems[i] == null)
            {
                _slotViews[i].Clear(); // 沒綁道具的格子保持空白
            }
            else
            {
                int count = InventorySystem.Instance.GetItemCount(slotItems[i]);
                _slotViews[i].SetItem(slotItems[i], count);
            }
        }
    }
}
