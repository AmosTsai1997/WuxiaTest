using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // 要操作 Button 元件

public class NPCQuestGiver : MonoBehaviour
{
    [Header("NPC 設定")]
    public string npcName = "村長";
    public QuestData questData; // ★ 核心：每個 NPC 在 Inspector 拖「自己的」任務檔案

    [Header("UI 引用（共用同一組對話 UI）")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("接受/取消按鈕")]
    public GameObject actionButtons; // ActionButtons 整個群組
    public Button acceptButton;      // 接受按鈕
    public Button declineButton;     // 取消按鈕

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
        //  防呆兼功能：如果這個 NPC 沒有任務，或是任務檔案沒拖，顯示預設對話
        if (questData == null)
        {
            if (actionButtons != null) actionButtons.SetActive(false);
            dialogueText.text = $"{npcName}：你好啊，旅行者，祝你旅途平安。";
            return;
        }
        QuestManager.QuestState state = QuestManager.Instance.GetState(questData);

        if (actionButtons != null) actionButtons.SetActive(false);

        switch (state)
        {
            case QuestManager.QuestState.NotAccepted:
                dialogueText.text = $"{npcName}：{questData.offerText}";
                if (actionButtons != null)
                {
                    actionButtons.SetActive(true);

                    // 按鈕是所有 NPC「共用」的，所以每次都要先清掉
                    // 上一個 NPC 綁的事件、再綁上自己的 ——
                    // 不清的話，按一次按鈕會同時觸發好幾個 NPC 的任務
                    acceptButton.onClick.RemoveAllListeners();
                    acceptButton.onClick.AddListener(OnAcceptClicked);
                    declineButton.onClick.RemoveAllListeners();
                    declineButton.onClick.AddListener(OnDeclineClicked);
                }

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
        if (questData == null) return;

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

    // 按下「接受」
    void OnAcceptClicked()
    {
        QuestManager.Instance.AcceptQuest(questData);
        RefreshDialogue(); // 狀態已變成 InProgress，刷新後按鈕自動隱藏
    }

    // 按下「取消」：關閉對話。下次再來對話，NPC 會再次詢問
    void OnDeclineClicked()
    {
        ToggleDialogue();
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