using UnityEngine;

// 裝備部位（獨立的 enum，之後很多地方會用到）
public enum EquipSlotType { Helmet, Body, Hands, Feet, Weapon , Weapon2 }
//                          頭盔     身體   手甲    鞋甲   武器   副武器

// 「: ItemData」= 繼承：裝備是一種道具，
// itemName、icon、description 全部自動繼承，不用重寫
[CreateAssetMenu(fileName = "Equipment", menuName = "Item/裝備資料")]
public class EquipmentData : ItemData
{
    [Header("裝備設定")]
    public EquipSlotType slotType = EquipSlotType.Weapon;
    public int attackBonus = 0;   // 之後接屬性系統用，先存著
    public int defenseBonus = 0;
    [Header("武器動畫設定")]
    public GameObject modelPrefab; // 佩帶時顯示在手上的模型（乾淨原版，不是 Pickup 變體）
    public AnimationClip attackAnimation; // 這把武器自己的攻擊動作（留空 = 用預設動作）
    public float attackCooldown = 1f;     // 攻擊間隔（秒）：越小攻速越快
}