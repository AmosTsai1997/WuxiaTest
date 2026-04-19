using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SimpleDialogue : MonoBehaviour
{
    [Header("UI 連結")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Button[] optionButtons; // 將 3 個按鈕拖入此陣列

    [Header("NPC 設定")]
    private NPCInteraction _npcInformation;
    private bool isPlayerNearby = false;

    void Start()
    {
        _npcInformation = GetComponent <NPCInteraction>();
        dialoguePanel.SetActive(false);
        
        // 為按鈕綁定點擊事件
        optionButtons[0].onClick.AddListener(() => OnSelectOption(1));
        optionButtons[1].onClick.AddListener(() => OnSelectOption(2));
        optionButtons[2].onClick.AddListener(() => OnSelectOption(3));
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

    if (isActive)
    {
        dialogueText.text = $"【{_npcInformation.npcName}】: 少俠，這兵荒馬亂的，有何貴幹？";
        
        // 【關鍵】確保開啟對話時，選項按鈕是顯示的
        foreach(Button btn in optionButtons) 
        {
            btn.gameObject.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    else
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    }

    public void OnSelectOption(int index)
{
    // 1. 先把選項按鈕隱藏，避免重複點擊或擋住文字
    foreach (Button btn in optionButtons)
    {
        btn.gameObject.SetActive(false);
    }

    // 2. 根據選項給予回覆
    switch (index)
    {
        case 1:
            dialogueText.text = $"【{_npcInformation.npcName}】: 這裡是大隱村，一個被世人遺忘的桃源。";
            // 讓玩家看完這句話，3秒後自動關閉
            Invoke("CloseDialogue", 3.0f); 
            break;
            
        case 2:
            dialogueText.text = $"【{_npcInformation.npcName}】: 村外的林子裡最近有狼群出沒，你要是能去清理一下就好了。";
            Invoke("CloseDialogue", 3.0f);
            break;
            
        case 3:
            dialogueText.text = $"【{_npcInformation.npcName}】: 慢走不送，少俠保重。";
            // 再見通常比較短，1.5秒後關閉即可
            Invoke("CloseDialogue", 1.5f);
            break;
    }
}

    // 建立一個專門關閉對話框的函式
    void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        
        // 恢復玩家的操作狀態
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 如果有使用第三方腳本控制玩家移動，記得在此處解除鎖定
        Debug.Log("對話結束，自動關閉面版。");
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
        
        // 【關鍵】取消所有的 Invoke 預約，避免時間到了亂跳
        CancelInvoke("CloseDialogue");
        
        dialoguePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}   
}