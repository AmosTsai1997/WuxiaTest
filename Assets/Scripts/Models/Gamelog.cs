// 遊戲事件日誌的「廣播電台」。
// static class = 不用掛在任何物件上、全遊戲直接用類別名呼叫。
// 任何系統想報告事件：GameLog.Post("撿到 木棒");
// 它不認識 UI，UI 自己來訂閱 —— 和 OnInventoryChanged 同一個解耦思想
public static class GameLog
{
    public static event System.Action<string> OnMessage;

    public static void Post(string message)
    {
        OnMessage?.Invoke(message);
    }
}