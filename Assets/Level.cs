using UnityEngine;
using UnityEngine.UI;
public class LevelSystem : MonoBehaviour
{
    public int level = 1;
    public int currentXP = 0;

    // 當升級時，通知其他組件更新
    public void AddXP(int amount)
    {
        currentXP += amount;
        if (currentXP >= CalculateNextLevelXP())
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        // 核心邏輯：升級後更新血量上限
        Health h = GetComponent<Health>();
        if (h != null)
        {
            h.UpdateStats(level); // 告訴 Health 腳本：我升級了，請調高上限
        }
        Debug.Log($"等級提升至：{level}");
    }

    int CalculateNextLevelXP() => level * 100; // 簡單的經驗公式
}