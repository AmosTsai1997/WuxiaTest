using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 等級系統：玩家和怪物都能掛。數字與公式都在 GrowthData 成長表裡
public class LevelSystem : MonoBehaviour
{
    [Header("成長表（玩家、BOSS 可以各用一張）")]
    public GrowthData growthData;

    [Header("等級資訊")]
    [SerializeField] private int _currentLevel = 1;
    public int currentLevel => _currentLevel; // 對外唯讀

    public int currentXP = 0;

    [Header("UI 連結（怪物留空即可）")]
    public Slider expSlider;
    public TextMeshProUGUI levelText;

    [Header("屬性加成")]
    public float attackMultiplier = 1.0f;

    // 升級廣播：之後升級特效、技能面板紅點都訂閱這裡
    public event System.Action<int> OnLevelUp;

    void Start()
    {
        UpdateStats();
        UpdateUI();
    }

    // 統一從成長表拿數字；忘了拖資產就用預設公式並警告，不讓遊戲爆掉
    int GetNextLevelXP()
    {
        if (growthData != null) return growthData.GetNextLevelXP(_currentLevel);
        Debug.LogWarning($"{gameObject.name} 的 LevelSystem 沒拖 GrowthData，先用預設公式");
        return _currentLevel * 100;
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"獲得經驗：{amount}，當前經驗：{currentXP}/{GetNextLevelXP()}");
        GameLog.Post($"獲得經驗 {amount}");

        // while：一次吃大量經驗可以連升多級
        while (currentXP >= GetNextLevelXP())
        {
            LevelUp();
        }
        UpdateUI();
    }

    void LevelUp()
    {
        currentXP -= GetNextLevelXP(); // 先扣這一級的需求
        _currentLevel++;
        UpdateStats();

        // 只有「玩家」升級才發技能點（怪物升級不發）
        if (CompareTag("Player") && SkillSystem.Instance != null && growthData != null)
        {
            SkillSystem.Instance.AddPoints(growthData.skillPointsPerLevel);
        }

        OnLevelUp?.Invoke(_currentLevel); // 通知所有關心升級的人
        Debug.Log($"<color=yellow>等級提升！ 現在 LV.：{_currentLevel}</color>");
        GameLog.Post($"<color=yellow>等級提升！ 現在 LV.{_currentLevel}</color>");

    }

    public void UpdateStats()
    {
        // 血量上限
        Health h = GetComponent<Health>();
        if (h != null)
        {
            float newMax = (growthData != null)
                ? growthData.GetMaxHP(_currentLevel)
                : 100f + _currentLevel * 20f;
            h.SetHealthFromLevel(newMax);
        }
        // 攻擊倍率
        attackMultiplier = (growthData != null)
            ? growthData.GetAttackMultiplier(_currentLevel)
            : 1.0f + (_currentLevel - 1) * 0.1f;
    }

    // 給 GameManager 同步 BOSS 等級用
    public void SetLevel(int newLevel)
    {
        _currentLevel = newLevel;
        UpdateStats(); // 修正：以前只改等級沒更新屬性，BOSS 的血量攻擊都沒真的變
        UpdateUI();
        Debug.Log($"{gameObject.name} 等級設為 {newLevel}，屬性已同步。");
    }

    void UpdateUI()
    {
        if (expSlider != null)
        {
            expSlider.maxValue = GetNextLevelXP();
            expSlider.value = currentXP;
        }
        if (levelText != null) levelText.text = "LV. " + _currentLevel;
    }
}