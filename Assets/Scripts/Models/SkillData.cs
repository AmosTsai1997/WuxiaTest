using UnityEngine;

// 一個技能一張資料卡（跟 ItemData、QuestData 同思想）。
// 實作欄位（動畫、傷害、冷卻）之後做技能時再補
[CreateAssetMenu(fileName = "NewSkill", menuName = "Character/技能資料")]
public class SkillData : ScriptableObject
{
    public string skillName = "新技能";
    public Sprite icon;
    [TextArea(2, 3)] public string description;

    [Header("學習條件")]
    public int cost = 1;           // 要花幾點技能點
    public int requiredLevel = 1;  // 等級門檻
    public SkillData prerequisite; // 前置技能（做技能樹用，可留空）
}