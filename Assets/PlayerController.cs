using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;
    private PlayerLevel playerLevel;

    void Start()
    {
        playerLevel = GetComponent<PlayerLevel>();
    }

    void Update()
    {
        // 1. 處理移動輸入 (WASD 或 方向鍵)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            // 移動位置
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

            // 讓角色轉向移動方向
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        // 2. 處理攻擊輸入 (滑鼠左鍵)
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void Attack()
    {
        Debug.Log("發動攻擊！");
        // 簡單的圓形範圍偵測
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, 1.5f);

        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<Enemy>().TakeDamage(20f, playerLevel);
            }
        }
    }

    // 在編輯器畫出攻擊範圍，方便你調試
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, 1.5f);
    }
}