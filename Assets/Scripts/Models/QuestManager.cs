using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class QuestManager :MonoBehaviour
{
    public static QuestManager Instance;
    // 任務的四種狀態：未接受 → 進行中 → 可交付 → 已完成
    public enum QuestState { NotAccepted, InProgress, ReadyToTurnIn, Completed }

    // 每個任務的「進度紀錄卡」
    private class QuestProgress
    {
        public QuestState state = QuestState.InProgress; // 建卡時就是已接受
        public int killCount = 0;
    }

    // 字典（Dictionary）：像一本帳簿，用「任務資料」當索引去查「進度卡」。
    private Dictionary<QuestData, QuestProgress> _progress = new Dictionary<QuestData, QuestProgress>();
    void Awake()
    {
        // 單例查重放最前面 —— GameManager 那次的教訓
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨場景保留，去大地圖殺怪也算數
        }
        else { Destroy(gameObject); return; }
    }

    public QuestState GetState(QuestData quest)
    {
        if (!_progress.ContainsKey(quest)) return QuestState.NotAccepted;
        return _progress[quest].state;
    }

    public int GetKillCount(QuestData quest)
    {
        if (!_progress.ContainsKey(quest)) return 0;
        return _progress[quest].killCount;
    }

    // 接受任務：在帳簿裡開一張新卡
    public void AcceptQuest(QuestData quest)
    {
        if (_progress.ContainsKey(quest)) return; // 防止重複接
        _progress[quest] = new QuestProgress();
        Debug.Log($"接受任務：{quest.questName}");
    }

    // 有怪被殺時，由 Health 通報這裡。巡一遍所有進行中的任務，比對名字
    public void NotifyEnemyKilled(string enemyName)
    {
        foreach (KeyValuePair<QuestData, QuestProgress> entry in _progress)
        {
            QuestData quest = entry.Key;
            QuestProgress prog = entry.Value;

            if (prog.state != QuestState.InProgress) continue; // 只管進行中的

            // Contains：怪物實際名字是 "Goblin Variant(Clone)" 這種，
            // 用「包含關鍵字」比對，不用剛好全等
            if (enemyName.Contains(quest.targetEnemyName))
            {
                prog.killCount++;
                Debug.Log($"任務進度 [{quest.questName}]：{prog.killCount}/{quest.targetCount}");

                if (prog.killCount >= quest.targetCount)
                {
                    prog.state = QuestState.ReadyToTurnIn;
                    Debug.Log($"任務 [{quest.questName}] 目標達成！回去找 NPC 交任務吧");
                }
            }
        }
    }
    // 交任務領獎勵
    public void TurnInQuest(QuestData quest)
    {
        if (GetState(quest) != QuestState.ReadyToTurnIn) return;
        _progress[quest].state = QuestState.Completed;

        // 發經驗值給玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            LevelSystem ls = player.GetComponent<LevelSystem>();
            if (ls != null) ls.AddXP(quest.expReward);
        }
        Debug.Log($"完成任務 [{quest.questName}]，獲得 {quest.expReward} 經驗！");
    }
}
