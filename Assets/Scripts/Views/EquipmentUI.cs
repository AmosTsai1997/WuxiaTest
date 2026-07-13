using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject equipmentPanel;
    public EquipmentSlotUI[] slots; // 五個部位格，Inspector 拖入

    [Header("衝突防護")]
    public GameObject dialoguePanel; // 拖共用對話面板：對話中 E 鍵讓給 NPC

    void Start()
    {
        EquipmentSystem.Instance.OnEquipmentChanged += Refresh;
        Refresh();
        equipmentPanel.SetActive(false);
    }
    void OnDestroy()
    {
        if (EquipmentSystem.Instance != null)
            EquipmentSystem.Instance.OnEquipmentChanged -= Refresh;
    }
    void Update()
    {
        // 對話進行中不搶 E 鍵（E 在對話裡是接/交任務）
        if (dialoguePanel != null && dialoguePanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.E))
            equipmentPanel.SetActive(!equipmentPanel.activeSelf);
    }
    void Refresh()
    {
        foreach (EquipmentSlotUI s in slots) s.Refresh();
    }
}

