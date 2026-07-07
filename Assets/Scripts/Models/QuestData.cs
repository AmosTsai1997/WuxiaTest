using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// CreateAssetMenu：加了這行，就能在 Project 視窗右鍵 → Create → Quest/任務資料
// 直接「生產任務檔案」，一個檔案就是一個任務
[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/任務資料")]
public class QuestData : ScriptableObject
{
    [Header("任務基本資料")]
    public string questName = "討伐哥布林";

    [Header("各階段的對話文案")]
    [TextArea(2, 4)] public string offerText = "最近哥布林作亂，能幫我除掉牠們嗎？";
    [TextArea(2, 4)] public string progressText = "還沒解決完嗎?加油！";
    [TextArea(2, 4)] public string completeText = "太感謝了！這是你的報酬。";
    [TextArea(2, 4)] public string afterText = "多虧了你，村子安全多了。";

    [Header("討伐目標")]
    public string targetEnemyName = "Goblin"; // 用名字關鍵字比對要殺的怪
    public int targetCount = 3;               // 要殺幾隻

    [Header("獎勵")]
    public int expReward = 100;
}
