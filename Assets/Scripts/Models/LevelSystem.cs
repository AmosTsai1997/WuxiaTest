using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class LevelSystem : MonoBehaviour
{   
    [Header("等級資訊")]
    [SerializeField] // 讓 private 變數也能顯示在 Inspector 視窗中
    private int _currentLevel = 1;
    // 提供一個 public 的唯讀接口，讓其他腳本（如 NPC 或 UI）可以讀取等級
    public int currentLevel => _currentLevel;
    public int currentXP = 0;
    [Header("UI 連結")]
    public Slider expSlider;          // 拖入左上角的經驗值 Slider
    public TextMeshProUGUI levelText; // 顯示 "LV. 1" 的文字
    [Header("屬性加成")]
    public float attackMultiplier = 1.0f; // 攻擊倍率


    void Start()
    {   
        UpdateStats();
        UpdateUI();
    }
    // 當升級時，通知其他組件更新
    public void AddXP(int amount)
    {
      currentXP += amount;
        Debug.Log($"獲得經驗：{amount}，當前經驗：{currentXP}/{CalculateNextLevelXP()}");

        // 使用 while 是為了防止一次獲得大量經驗時可以「連升多等」
        while (currentXP >= CalculateNextLevelXP())
        {
            LevelUp();
        }
        UpdateUI();
    }
    void LevelUp()
    {
        currentXP -= CalculateNextLevelXP(); // 扣除升級所需經驗
        _currentLevel++; // 真正增加等級
        
        UpdateStats(); // 更新血量與攻擊力
        
        // 這裡可以呼叫 PlayerActions 檢查武器解鎖
        PlayerActions hero = GetComponent<PlayerActions>();
        if (hero != null)
        {
            // hero.CheckWeaponUnlock(currentLevel); // 假設你在 PlayerActions 有寫這行
        }

        Debug.Log($"<color=yellow>恭喜升級！當前等級：{currentLevel}</color>");
    }
   public void UpdateStats()
    {
        // 1. 更新血量 (如之前所述)
        Health h = GetComponent<Health>();
        if (h != null)
        {
            float newMax = 100f + (_currentLevel * 20f);
            // 改用 Function 呼叫，解決錯誤 2 (權限問題)
            h.SetHealthFromLevel(newMax);
        }
        // 2. 更新攻擊加成 (每升一等增加 10% 攻擊力)
        // 公式：1.0 + (等級-1) * 0.1
        attackMultiplier = 1.0f + (_currentLevel - 1) * 0.1f;
        
        Debug.Log($"等級 {_currentLevel}：血量上限為 {100 + (_currentLevel * 20)}，攻擊倍率為 {attackMultiplier}x");
    }
    public void SetLevel(int newLevel)
    {
        _currentLevel = newLevel;
        Debug.Log($"等級已更新為 {newLevel}，屬性已同步強化。");
    }
        public int GetLevel()
    {   
        Debug.Log($"等級資訊已更新。");
        return currentLevel;
       
    }
    int CalculateNextLevelXP() => _currentLevel * 100; // 簡單的經驗公式

    void UpdateUI()
    {
        if (expSlider != null)
        {
            expSlider.maxValue = CalculateNextLevelXP();
            expSlider.value = currentXP;
        }

        if (levelText != null)
        {
            levelText.text = "LV. " + currentLevel;
        }
    }
}