using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class NPCQuestGiver : MonoBehaviour
{
    [Header("NPC 設定")]
    public string npcName = "村長";
    public QuestData questData; // ★ 核心：每個 NPC 在 Inspector 拖「自己的」任務檔案

    [Header("UI 引用（共用同一組對話 UI）")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    private bool isPlayerNearby = false;
    private bool isTalking = false;

    void Update()
    {
        // F：開關對話（跟你原本 NPCInteraction 的習慣一致）
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F)) ToggleDialogue();

        // E：對話中按 E 執行「接任務」或「交任務」
        if (isTalking && Input.GetKeyDown(KeyCode.E)) HandleQuestAction();
    }

    void ToggleDialogue()
    {
        isTalking = !isTalking;
        dialoguePanel.SetActive(isTalking);
        if (isTalking) RefreshDialogue();
    }

    // 依照「這個 NPC 自己任務」的狀態，決定說什麼話
    void RefreshDialogue()
    {
        QuestManager.QuestState state = QuestManager.Instance.GetState(questData);

        switch (state)
        {
            case QuestManager.QuestState.NotAccepted:
                dialogueText.text = $"{npcName}：{questData.offerText}\n\n[按 E 接受任務]";
                break;

            case QuestManager.QuestState.InProgress:
                int now = QuestManager.Instance.GetKillCount(questData);
                dialogueText.text = $"{npcName}：{questData.progressText}\n\n進度：{now}/{questData.targetCount}";
                break;

            case QuestManager.QuestState.ReadyToTurnIn:
                dialogueText.text = $"{npcName}：{questData.completeText}\n\n[按 E 領取獎勵]";
                break;

            case QuestManager.QuestState.Completed:
                dialogueText.text = $"{npcName}：{questData.afterText}";
                break;
        }
    }

    void HandleQuestAction()
    {
        QuestManager.QuestState state = QuestManager.Instance.GetState(questData);

        if (state == QuestManager.QuestState.NotAccepted)
        {
            QuestManager.Instance.AcceptQuest(questData);
            RefreshDialogue(); // 接完馬上刷新對話內容
        }
        else if (state == QuestManager.QuestState.ReadyToTurnIn)
        {
            QuestManager.Instance.TurnInQuest(questData);
            RefreshDialogue();
        }
    }

    // 玩家靠近／離開的偵測（NPC 身上要有勾了 Is Trigger 的 Collider）
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            isTalking = false;
            dialoguePanel.SetActive(false); // 走遠自動關閉對話
        }
    }
}