using UnityEngine;
using System.Collections.Generic;

// 技能系統（玩家專屬）：管技能點、已學技能。
// 跟 InventorySystem 同款：單例 + 廣播
public class SkillSystem : MonoBehaviour
{
    public static SkillSystem Instance;

    public int skillPoints = 0; // 目前可用的技能點

    [SerializeField] // 顯示在 Inspector 方便你偵錯觀察
    private List<SkillData> _learned = new List<SkillData>();

    public event System.Action OnSkillChanged; // 點數或已學清單變動時廣播

    void Awake()
    {
        // 查重複
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    public bool HasLearned(SkillData skill) => _learned.Contains(skill);

    // 升級時由 LevelSystem 呼叫發點
    public void AddPoints(int amount)
    {
        skillPoints += amount;
        OnSkillChanged?.Invoke();
        Debug.Log($"<color=yellow>獲得 {amount} 技能點，目前共 {skillPoints} 點</color>");
    }

    // 檢查能不能學：不能學時回報原因（之後 UI 可以直接顯示這個字串）
    public bool CanLearn(SkillData skill, out string reason)
    {
        reason = "";
        if (skill == null) { reason = "技能資料是空的"; return false; }
        if (HasLearned(skill)) { reason = "已經學會了"; return false; }
        if (skillPoints < skill.cost) { reason = $"技能點不足（需要 {skill.cost} 點）"; return false; }

        // 等級門檻：去玩家身上查目前等級
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        LevelSystem lv = (player != null) ? player.GetComponent<LevelSystem>() : null;
        if (lv != null && lv.currentLevel < skill.requiredLevel)
        {
            reason = $"等級不足（需要 LV.{skill.requiredLevel}）"; return false;
        }

        // 前置技能
        if (skill.prerequisite != null && !HasLearned(skill.prerequisite))
        {
            reason = $"要先學會【{skill.prerequisite.skillName}】"; return false;
        }
        return true;
    }

    // 學習技能：成功回傳 true
    public bool Learn(SkillData skill)
    {
        if (!CanLearn(skill, out string reason))
        {
            Debug.Log($"無法學習 {skill?.skillName}：{reason}");
            return false;
        }
        skillPoints -= skill.cost; // 扣點
        _learned.Add(skill);       // 記帳
        OnSkillChanged?.Invoke();  // 廣播（之後技能面板訂閱刷新）
        Debug.Log($"<color=green>學會了【{skill.skillName}】！</color>剩餘 {skillPoints} 點");
        return true;
    }
}