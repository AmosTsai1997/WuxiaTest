using UnityEngine;


// 武器種類：攻擊邏輯依這個自動適配（近戰=點擊即結算；弓=拉弓蓄力後放箭）
public enum WeaponKind { Melee, Bow }
//                       近戰    弓
// 裝備部位（獨立的 enum，之後很多地方會用到）
public enum EquipSlotType { Helmet, Body, Hands, Feet, Weapon , Weapon2 }
//                          頭盔     身體   手甲    鞋甲   武器   副武器

// itemName、icon、description 全部自動繼承，不用重寫
[CreateAssetMenu(fileName = "Equipment", menuName = "Item/裝備資料")]
public class EquipmentData : ItemData
{
    [Header("裝備設定")]
    public EquipSlotType slotType = EquipSlotType.Weapon;
    public int attackBonus = 0;   // 之後接屬性系統用，先存著
    public int defenseBonus = 0;

    [Header("握持校準（每把武器的模型軸心不同，各自微調）")]
    public Vector3 gripPosition = Vector3.zero; // 相對插槽的位置偏移
    public Vector3 gripRotation = Vector3.zero; // 相對插槽的旋轉偏移（歐拉角）

    [Header("武器動畫設定")]
    public WeaponKind weaponKind = WeaponKind.Melee; // 弓種武器改選 Bow
    public GameObject weaponPrefab; // 佩帶時顯示在手上的模型（乾淨原版，不是 Pickup 變體）
    public GameObject projectilePrefab; // 弓專用：發射的箭矢 Prefab
    public AnimationClip attackAnimation; // 這把武器自己的攻擊動作（留空 = 用預設動作）
    public float attackCooldown = 1f;     // 攻擊間隔（秒）：越小攻速越快
    public float attackRange = 2.5f;                 // 攻擊距離：近戰約 2.5、弓約 15


    [Header("防具設定")]
    public GameObject armorPrefab;
}