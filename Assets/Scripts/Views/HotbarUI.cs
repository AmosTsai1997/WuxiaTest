using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI; // 要改 Image 顏色，所以引用 UI 命名空間
using TMPro;          // 要改 TextMeshPro 文字，所以引用 TMP 命名空間

public class HotbarUI : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject hotbarPanel; // 快捷欄面板
    public GameObject slotPrefab;

    [Header("快捷列設定")]
    public int slotCount = 5; // 格子數量
    public Color normalColor = new Color(1f, 1f, 1F, 0.4f);
    public Color selectedColor = new Color(1f, 0.9f, 0.3F, 0.8f);


    private Image[] _slotImages;
    private int _selectedIndex = -1; // 目前選中第幾格，-1 代表都沒選

    void Start() 
    {
        // 開一個能裝 5 個 Image 的陣列
        _slotImages = new Image[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, hotbarPanel.transform);
            slot.name = "HotbarSlot_" + (i + 1);
            // 把這格的 Image 存進陣列第 i 號位置，並設成平常色
            _slotImages[i] = slot.GetComponent<Image>();
            _slotImages[i].color = normalColor;

            // GetComponentInChildren = 「去子物件裡找」，
            // 找到我們在 Prefab 裡加的 KeyText，把文字改成按鍵編號。
            // i 從 0 開始，但鍵盤是 1~5，所以要 +1
            TextMeshProUGUI keyText = slot.GetComponentInChildren<TextMeshProUGUI>();
            keyText.text = (i + 1).ToString();
        }
    }
    void Update() 
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
    }
    void SelectSlot( int index )
    {
        // 先把每一格都恢復平常色（不然會同時亮兩格）
        for (int i = 0; i < slotCount; i++)
        {
            _slotImages[i].color = normalColor;
        }
        _slotImages[index].color = selectedColor;
        _selectedIndex = index;

        // 之後做「使用道具」功能時，就是在這裡呼叫，例如：
        // InventorySystem.Instance.UseItem(index);
    }
}
