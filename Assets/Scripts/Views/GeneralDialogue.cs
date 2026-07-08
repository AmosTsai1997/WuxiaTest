using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GeneralDialogue : MonoBehaviour
{
    [Header("UI 連結")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Button[] optionButtons;

    [Header("NPC 基本設定")]
    public string npcName = "神祕人"; // 直接在 Inspector 改名字
    [TextArea(3, 5)] // 讓文字框再大一點好輸入
    public string greetingText = "有甚麼事嗎？";

    [Header("選項回覆設定")]
    public GameObject OptionGroup; // 選項按鈕群組
    [TextArea(2, 4)] public string responseOption1 = "這裡是...我也說不清楚。";
    [TextArea(2, 4)] public string responseOption2 = "外面很危險，小心點。";
    [TextArea(2, 4)] public string responseOption3 = "保重，再見。";

    private bool isPlayerNearby = false;

    void Start()
    {
        // 自動嘗試抓取 NPCInteraction，如果抓不到也沒關係 (Null-safe)
        var missionScript = GetComponent<NPCInteraction>();
        if (missionScript != null)
        {
            npcName = missionScript.npcName; // 如果有任務腳本，就同步名字
        }

        dialoguePanel.SetActive(false);
        
        // 綁定事件
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i + 1; // 閉包問題，需用臨時變數
            optionButtons[i].onClick.RemoveAllListeners(); // 先清空避免重複綁定
            optionButtons[i].onClick.AddListener(() => OnSelectOption(index));
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            ToggleDialogue();
        }
    }

    void ToggleDialogue()
    {
        if (dialoguePanel == null) return;

        bool isActive = !dialoguePanel.activeSelf;
        dialoguePanel.SetActive(isActive);
        OptionGroup.SetActive(isActive);

        if (isActive)
        {
            dialogueText.text = $"【{npcName}】: {greetingText}";
            ShowOptions();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            CloseDialogue();
        }
    }

    public void OnSelectOption(int index)
    {
        // 隱藏按鈕
        foreach (Button btn in optionButtons) btn.gameObject.SetActive(false);

        // 根據變數顯示回覆
        switch (index)
        {
            case 1:
                dialogueText.text = $"【{npcName}】: {responseOption1}";
                Invoke("CloseDialogue", 3.0f); 
                break;
            case 2:
                dialogueText.text = $"【{npcName}】: {responseOption2}";
                Invoke("CloseDialogue", 3.0f);
                break;
            case 3:
                dialogueText.text = $"【{npcName}】: {responseOption3}";
                Invoke("CloseDialogue", 1.5f);
                break;
        }
    }

    void CloseDialogue()
    {
        CancelInvoke("CloseDialogue");
        dialoguePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ShowOptions()
    {
        foreach(Button btn in optionButtons) btn.gameObject.SetActive(true);
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
            CloseDialogue();
        }
    }
}