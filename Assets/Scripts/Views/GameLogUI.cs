using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameLogUI : MonoBehaviour
{
    [Header("UI 引用")]
    public TextMeshProUGUI logText;
    public ScrollRect scrollRect;

    [Header("設定")]
    public int maxLines = 50; // 最多保留的訊息數量

    private readonly Queue<string> _lines = new Queue<string>();
    void Awake()
    {
        GameLog.OnMessage += AddLine;
    }
    void OnDestroy()
    {
        GameLog.OnMessage -= AddLine;
    }
    void AddLine(string message)
    {
        _lines.Enqueue($"<color=black>[{System.DateTime.Now:HH:mm}]</color> {message}");
        while (_lines.Count > maxLines) _lines.Dequeue(); // 超量就淘汰最舊的
        logText.text = string.Join("\n", _lines.ToArray());
        Canvas.ForceUpdateCanvases(); // 先強制更新 UI
        if (scrollRect != null)  scrollRect.verticalNormalizedPosition = 0; // 滾到最下面
    }
}