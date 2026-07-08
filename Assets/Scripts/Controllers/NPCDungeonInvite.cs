using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 切換場景用
using TMPro;

public class NPCDungeonInvite : MonoBehaviour
{
    [Header("NPC 設定")]
    public string npcName = "Furfur";
    [TextArea(2, 4)]
    public string inviteText = "地下城深處出現了強大的魔物，冒險者，能協助我們討伐牠嗎？";
    public string dungeonSceneName = "Dungeon"; // 要跟場景檔名一字不差

    [Header("UI 引用（拖跟任務 NPC 同一組）")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject actionButtons;
    public Button acceptButton;
    public Button declineButton;

    private bool isPlayerNearby = false;
    private bool isTalking = false;

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F)) ToggleDialogue();
    }

    void ToggleDialogue()
    {
        isTalking = !isTalking;
        dialoguePanel.SetActive(isTalking);

        if (isTalking)
        {
            dialogueText.gameObject.SetActive(true); // 文字物件保險，之前的教訓
            dialogueText.text = $"{npcName}：{inviteText}";

            if (actionButtons != null)
            {
                actionButtons.SetActive(true);
                // 共用按鈕：先清舊綁定再綁自己的（跟任務 NPC 同一套規矩）
                acceptButton.onClick.RemoveAllListeners();
                acceptButton.onClick.AddListener(OnAccept);
                declineButton.onClick.RemoveAllListeners();
                declineButton.onClick.AddListener(OnDecline);
            }
        }
    }

    void OnAccept()
    {
        Debug.Log($"接受討伐委託，傳送至 {dungeonSceneName}...");
        SceneManager.LoadScene(dungeonSceneName);
        // 出生位置交給 GameManager.OnSceneLoaded 處理，這裡不用管
    }

    void OnDecline()
    {
        ToggleDialogue(); // 關閉對話，下次再問
    }

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
            dialoguePanel.SetActive(false);
        }
    }
}