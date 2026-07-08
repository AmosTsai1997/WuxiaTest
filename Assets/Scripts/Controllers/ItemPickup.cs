using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("道具內容（二選一）")]
    public ItemData itemData;  // 一般道具：拖 ItemData 資產，金幣就留空
    public int goldAmount = 0; // 金幣：填數量（此時 itemData 留空）

    [Header("展示效果")]
    public float lifeTime = 30f;

    void Start() { Destroy(gameObject, lifeTime); }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (goldAmount > 0)
        {
            InventorySystem.Instance.AddGold(goldAmount);
        }
        else if (itemData != null)
        {
            bool success = InventorySystem.Instance.AddItem(itemData);
            if (!success) return; // 背包滿 → 不銷毀，道具留在地上等你清出空間
        }

        Destroy(gameObject);
    }
}