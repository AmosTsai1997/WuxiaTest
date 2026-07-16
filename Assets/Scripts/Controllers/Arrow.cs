using UnityEngine;

// 箭矢投射物：直線飛行，射線步進防穿牆，命中才結算傷害，並插在目標上
public class Arrow : MonoBehaviour
{
    [Header("飛行設定")]
    public float speed = 25f;        // 飛行速度（公尺/秒）
    public float embedDepth = 0.25f; // 命中後箭頭沒入的深度（看起來像插進去）
    public float stickTime = 10f;    // 插在目標上多久後消失

    private float _damage;      // 傷害由發射者決定，不寫死在箭上
    private float _maxDistance; // 射程（來自弓的資料卡）
    private float _traveled;    // 已飛行距離
    private bool _flying = true;

    // 發射者在生成箭的當下呼叫，把傷害和射程「裝填」進來
    public void Init(float damage, float maxDistance)
    {
        _damage = damage;
        _maxDistance = maxDistance;
    }

    void Awake()
    {
        // 箭模型如果帶著素材包的碰撞體，一律拆掉 ——
        // 我們用射線偵測命中，碰撞體留著只會誤傷：被別支箭射中、插在怪物身上時擋住玩家的攻擊判定
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            Destroy(c);
        }
    }
    void Update()
    {
        if (!_flying) return;
        float step = speed * Time.deltaTime; // 這一幀要前進的距離

        // 射線步進：從目前位置往前掃 step 長度，路上有東西就是命中。
        // QueryTriggerInteraction.Ignore = 無視觸發器（掉落物、對話感應圈這些隱形區域）
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit,
            step + 0.05f, ~0, QueryTriggerInteraction.Ignore))
        {
            HandleHit(hit);
            return;
        }
        // 沒撞到東西 → 真的前進
        transform.position += transform.forward * step;
        _traveled += step;
        if (_traveled >= _maxDistance) Destroy(gameObject); // 超出射程，箭落地消失
    }

    void HandleHit(RaycastHit hit)
    {
        _flying = false;

        // 1. 把箭擺到命中點，往前多推一點製造「沒入」感
        transform.position = hit.point + transform.forward * embedDepth;

        // 2. 黏住：把箭變成命中物的子物件 —— 怪物移動、播動畫，箭跟著走
        transform.SetParent(hit.transform, true); // true = 保持世界座標不跳動

        // 3. 是怪物才結算傷害（射中牆壁地板就只是插著）
        if (hit.collider.CompareTag("Enemy"))
        {
            Health h = hit.collider.GetComponent<Health>();
            if (h != null) h.TakeDamage(_damage);
        }

        Destroy(gameObject, stickTime); // 插一陣子再自動清理
    }
}