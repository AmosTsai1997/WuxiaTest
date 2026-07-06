using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;

public class AIManager : MonoBehaviour
{
    private string apiKey = "你的_API_KEY_填在這裡"; 
    private string apiURL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=";

    public TextMeshProUGUI npcText; // 拖入 NPC 的對話文字框

    public void AskAI(string playerInput, string npcPersona)
    {
        StartCoroutine(PostToAI(playerInput, npcPersona));
    }

    IEnumerator PostToAI(string userInput, string persona)
    {
        npcText.text = "思考中...";

        // 建立 Prompt：結合 NPC 性格與玩家問題
        string prompt = $"你是個武俠遊戲中的NPC，設定是：{persona}。請用古風與玩家對話，字數50字內。玩家說：{userInput}";
        
        // 建立符合 Gemini 格式的 JSON
        string jsonData = "{\"contents\": [{\"parts\":[{\"text\":\"" + prompt + "\"}]}]}";

        using (UnityWebRequest request = new UnityWebRequest(apiURL + apiKey, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 這裡我們用簡單的字串分割來解析（之後可以用 JSONUtility 改進）
                string response = request.downloadHandler.text;
                Debug.Log("AI 回覆: " + response);
                
                // 簡易解析範例 (實際建議使用 JSON 庫)
                npcText.text = ParseGeminiResponse(response);
            }
            else
            {
                npcText.text = "（NPC 似乎不想理你...）";
                Debug.LogError(request.error);
            }
        }
    }

    string ParseGeminiResponse(string json)
    {
        // 為了教學快速，這裡先簡化處理。正式開發建議用 JsonUtility 或 Newtonsoft.Json
        // 尋找 JSON 中 text 欄位的內容
        try {
            int start = json.IndexOf("text\": \"") + 8;
            int end = json.IndexOf("\"", start);
            return json.Substring(start, end - start).Replace("\\n", "\n");
        } catch {
            return "我...我聽不懂你在說什麼。";
        }
    }
}