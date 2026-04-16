using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject tutorialPanel; // 拖入你的 TutorialPanel
    public Button startButton;      // 拖入你的 StartButton
    public GameObject enemyPrefab;

    void Awake()
    {
        // 1. 遊戲一啟動就凍結時間
        Time.timeScale = 0f;
        
        // 2. 確保提示視窗是開啟的
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }

        // 3. 綁定按鈕點擊事件
        if (startButton != null)
        {
            startButton.onClick.AddListener(CloseTutorial);
        }
    }

    void CloseTutorial()
    {
        // 4. 恢復時間流逝
        Time.timeScale = 1f;

        // 5. 關閉視窗
        tutorialPanel.SetActive(false);
        
        Debug.Log("遊戲正式開始！");
    }
    //怪物生成
        void SpawnEnemy()
    {
        // 在 (0, 0, 0) 的位置複製一隻怪物
        Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
    }
}