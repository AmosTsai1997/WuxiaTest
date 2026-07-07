using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public  class InventoryUI : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject inventoryPanel; // 背包面板
    public GameObject slotPrefab;

    [Header("格子設定")]
    public int slotCount = 50; // 格子數量

    void Start()
    {   
        // 生成50個格子
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, inventoryPanel.transform);

            // 幫每個格子命名編號（例如 Slot_7），
            // 之後做「把道具放進第幾格」的功能時會很好用
            slot.name = "Slot_" + i;
        }
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        // 按 I 鍵開關道具欄（I = Inventory，慣例用法）
        if (Input.GetKeyDown(KeyCode.I))
        {
            // activeSelf 是「目前開著嗎」，取反(!)就達成切換效果：
            // 開著 -> 關掉，關著 -> 打開
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }

}

