using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public int currentLevel = 1;
    public int currentXP = 0;

    public void AddExperience(int amount)
    {
        currentXP += amount;
        Debug.Log("當前總經驗: " + currentXP);
        
        // 簡單升級邏輯：滿 100 經驗升一級
        if (currentXP >= 100)
        {
            currentLevel++;
            currentXP -= 100;
            Debug.Log("恭喜升級！目前等級: " + currentLevel);
        }
    }
}