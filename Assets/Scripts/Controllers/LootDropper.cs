using UnityEngine;

// 掉落邏輯專用腳本：跟 Health 掛在同一隻怪身上，
// 透過「訂閱死亡事件」得知何時該掉寶
public class LootDropper : MonoBehaviour
{
    // 掉落表的「一列」：什麼東西、多少機率
    [System.Serializable]
    public class LootEntry
    {
        public GameObject prefab;          // 掉落物 Prefab
        [Range(0f, 1f)] public float chance; // 這一項的獨立機率
    }
    [Header("掉落表（每列獨立擲骰，可同時掉多樣）")]
    public LootEntry[] lootTable;

    private Health _health;

    void Awake()
    {
        _health = GetComponent<Health>();

        // 訂閱：把自己的 DropLoot 方法「接上」Health 的死亡頻道。
        // += 的意思是「加入收聽名單」（一個頻道可以有很多收聽者）
        if (_health != null) _health.OnDeath += DropLoot;
    }

    void OnDestroy()
    {
        // 退訂：物件被銷毀前把自己從名單移除。
        // 訂閱和退訂永遠成對出現 —— 這是使用事件的鐵律，
        // 忘記退訂會讓頻道記著一個已死的收聽者
        if (_health != null) _health.OnDeath -= DropLoot;
    }

    // 收到死亡通知時執行
    void DropLoot()
    {
        foreach (LootEntry entry in lootTable)
        {
            if (entry.prefab == null) continue;
            if (Random.value > entry.chance) continue; // 這項沒中獎，看下一項

            // 每件掉落物的位置隨機錯開一點，才不會全疊在同一個點
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, Random.Range(-0.5f, 0.5f));
            Instantiate(entry.prefab, transform.position + offset, Quaternion.identity);
        }
    }
}