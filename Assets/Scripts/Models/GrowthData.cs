using UnityEngine;

// 成長表：把「升級的所有數字與公式」從程式碼搬進資料資產。
// 玩家、BOSS、菁英怪可以各自拿一張不同的表，平衡調整不用改程式
[CreateAssetMenu(fileName = "GrowthData", menuName = "Character/成長表")]
public class GrowthData : ScriptableObject 
{
    [Header("經驗曲線")]
    public int baseXP = 100; // 升下一級所需 = baseXP × 目前等級

    [Header("血量成長")]
    public float baseHP = 100f;      // 基礎血量
    public float hpPerLevel = 20f;   // 每級增加

    [Header("攻擊成長")]
    public float attackPerLevel = 0.05f; // 每級 +5% 攻擊倍率

    [Header("技能點")]
    public int skillPointsPerLevel = 1; // 每升一級發幾點

    // 公式跟數據住在一起：所有讀這張表的人保證用同一套算法
    public int GetNextLevelXP(int level) => baseXP * level;
    public float GetMaxHP(int level) => baseHP + level * hpPerLevel;
    public float GetAttackMultiplier(int level) => 1f + (level - 1) * attackPerLevel;
}