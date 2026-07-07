using UnityEngine;

public class KeepObject : MonoBehaviour
{
    void Awake()
    {
        // 場景重載時，新生出來的複製品會執行這個 Awake（舊的不會重跑 Awake），
        // 所以在這裡檢查：場上是不是已經有一個「同名、也掛著 KeepObject」的前輩？
        // 有的話，代表我是重載時多出來的複製品 → 自我銷毀、讓前輩繼續當班
        KeepObject[] all = FindObjectsByType<KeepObject>();

        foreach (KeepObject k in all)
        {
            // k != this：排除自己（自己一定在名單裡）
            // 名字相同：用物件名稱判斷是不是「同一種東西」
            //（Player 和 Canvas 都掛 KeepObject，不能互相誤殺）
            if (k != this && k.gameObject.name == gameObject.name)
            {
                Destroy(gameObject);
                return; // 我要被銷毀了，下面的程式不能再執行
            }
        }

        // 沒有前輩 → 我就是第一個，登記為跨場景保留
        DontDestroyOnLoad(gameObject);
    }
}