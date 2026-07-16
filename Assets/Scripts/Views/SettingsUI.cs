using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    public GameObject settingPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Toggle();
    }

    // 開關設定面板（開著時暫停遊戲）
    public void Toggle()
    {
        bool show = !settingPanel.activeSelf;
        settingPanel.SetActive(show);
        Time.timeScale = show ? 0f : 1f; // 0 = 世界凍結，選單照常運作
    }

    // 綁在「離開遊戲」按鈕上
    public void QuitGame()
    {
        Debug.Log("離開遊戲");
        Application.Quit(); // 注意：只在「打包後的遊戲」有效
#if UNITY_EDITOR
        // 在編輯器裡 Application.Quit 沒作用，用這行代替（打包時會自動剔除）
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}