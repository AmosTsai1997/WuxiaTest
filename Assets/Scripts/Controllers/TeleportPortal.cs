using UnityEngine;
using UnityEngine.SceneManagement; // 必須引用這個才能切換場景

public class TeleportPortal : MonoBehaviour
{
    [Header("目標場景名稱")]
    public string targetSceneName = "LargeMap"; 
   

    // 當有物件進入 Trigger 時觸發
    private void OnTriggerEnter(Collider other)
    {
        // 檢查進入的是不是玩家 (根據 Tag)
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家進入傳送門，準備切換場景...");
            SceneManager.LoadScene(targetSceneName);
            //出現點
            GameObject spawnPoint = GameObject.Find("SpawnPoint_Entrance");
        if (spawnPoint != null)
        {
            // 先關閉 Character Controller，否則座標可能拉不動
            CharacterController cc = GetComponent<CharacterController>();
            if(cc != null) cc.enabled = false;

            transform.position = spawnPoint.transform.position;

            if(cc != null) cc.enabled = true;
        }
        }
    }
}