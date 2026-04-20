using UnityEngine;
using UnityEngine.UI;
public class LevelSystem : MonoBehaviour
{   
    private Health h ;
    public int level = 1;
    public int currentXP = 0;
    [Header("屬性加成")]
    public float attackMultiplier = 1.0f; // 攻擊倍率

    // 當升級時，通知其他組件更新
    public void AddXP(int amount)
    {
        currentXP += amount;
        if (currentXP >= CalculateNextLevelXP())
        {
            UpdateStats();
        }
    }

   public void UpdateStats(int currentLevel)
    {
        // 1. 更新血量 (如之前所述)
        Health h = GetComponent<Health>();
        if (h != null)
        {
            h.maxHP = 100f + (currentLevel * 20f);
            h.currentHP = h.maxHP;
        }

        // 2. 更新攻擊加成 (每升一等增加 10% 攻擊力)
        // 公式：1.0 + (等級-1) * 0.1
        attackMultiplier = 1.0f + (currentLevel - 1) * 0.1f;
        
        Debug.Log($"等級 {currentLevel}：血量上限為 {100 + (currentLevel * 20)}，攻擊倍率為 {attackMultiplier}x");
    }

    int CalculateNextLevelXP() => level * 100; // 簡單的經驗公式
}