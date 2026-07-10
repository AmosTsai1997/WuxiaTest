using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/道具資料")]
public class ItemData : ScriptableObject
{
    public string itemName = "紅藥水";
    public Sprite icon;                    // 道具欄裡顯示的圖示
    [TextArea(2, 3)] public string description;

    [Header("使用效果（素材類保持預設就好）")]
    public bool isUsable = false;   // 藥水勾這個；木棒這種素材不勾
    public float healAmount = 0f;   // 使用時回多少血

    [Header("增益效果（攻速藥水這類 Buff 藥水用）")]
    public float attackSpeedBonus = 0f; // 0.3 = 攻速提升 30%
    public float buffDuration = 0f;     // Buff 持續秒數
}
