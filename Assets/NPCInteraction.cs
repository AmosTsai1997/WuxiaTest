using UnityEngine;
using TMPro; // 記得引入

public class NPCInteraction : MonoBehaviour
{
    [Header("數值設定")]
    private Health healthScript; // 引用 Health 腳本
    public int xpValue = 20;

    [Header("UI 元件")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TMP_InputField playerInput;

    [Header("NPC 設定")]
    public string npcName = "神祕劍客";
    public string defaultGreeting = "這位少俠，找在下有何貴幹？";

    private bool isPlayerNearby = false;

    void Start()
    {   
        healthScript = GetComponent<Health>();
        dialoguePanel.SetActive(false); // 預設關閉對話框
    }

    void Update()
    {
        // 玩家在附近且按下 F 鍵觸發對話
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            ToggleDialogue();
        }
        // 檢查是否死亡（直接問 Health 腳本）
        if (healthScript != null && healthScript.currentHP <= 0)
        {
            PerformDeath();
        }
    }

    void ToggleDialogue()
    {
        bool isActive = !dialoguePanel.activeSelf;
        dialoguePanel.SetActive(isActive);

        if (isActive)
        {
            dialogueText.text = $"【{npcName}】: {defaultGreeting}";
            // 開啟對話時解鎖滑鼠，方便輸入
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // 觸發偵測
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("偵測到物體進入範圍：" + other.name);
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("靠近 NPC，按 F 對話");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("你按下了 F 鍵！");
            isPlayerNearby = false;
            dialoguePanel.SetActive(false);
        }
    }
    public void TakeDamage(float amount)
    {
        // 不要自己扣血，叫 Health 腳本去扣
        if (healthScript != null)
        {
            healthScript.TakeDamage(amount); 
        }
    }
    void PerformDeath()
        {
            // 處理 NPC 死亡邏輯（例如播動畫、消失）
            Debug.Log(gameObject.name + " 倒下了");
            Destroy(gameObject, 2f); // 2秒後移除
        }
}