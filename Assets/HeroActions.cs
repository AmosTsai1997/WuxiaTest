using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class HeroActions : MonoBehaviour
{
    private Animator _animator;
    private CharacterController _controller;
    private SimpleGravity _gravityScript;
    private LevelSystem _levelSystem; // 引用新的等級系統

    [Header("基礎攻擊力與範圍")]
    public float playerDamage = 30f;    
    public float playerAttackRange = 2f;

    [Header("移動與跳躍")]
    public float speed = 5f;            
    public float jumpHeight = 2.0f; // 修正：0.1f 太低了，改回 2.0f 較合適     
    public float turnSpeed = 720f;      

    [Header("武器系統")]
    public GameObject swordModel; 
    public GameObject bowModel;   
    public string currentWeapon = "None";

    [Header("戰鬥節奏設定")]
    public float nextAttackTime = 0.8f; // 下一次可以攻擊的時間
    public float swordCooldown = 1f; // 劍的間隔 (秒)
    public float bowCooldown = 1.2f;   // 弓的間隔 (秒)

    void Start()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _gravityScript = GetComponent<SimpleGravity>();
        _levelSystem = GetComponent<LevelSystem>(); // 初始化引用
    }

    void Update()
    {
        Health health = GetComponent<Health>();
        if (health != null && health.isDead)
        {
            if (Input.GetKeyDown(KeyCode.R)) Respawn();
            return; 
        }

        // 1. 處理跳躍
        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
        {
            if (_gravityScript != null)
            {
                _gravityScript.SetJumpVelocity(jumpHeight); 
                _animator.SetTrigger("Jump");
            }
        }
        // 2. 處理移動
        HandleMovement();

        // 3. 檢查武器解鎖 (配合 LevelSystem)
        if (_levelSystem != null)
        {
            CheckWeaponUnlock(_levelSystem.currentLevel);
        }

        // 4按鍵切換武器 (鍵盤 1 和 2)
        HandleWeaponSwitch();

        // 5. 攻擊邏輯
        if (Input.GetMouseButtonDown(0))
        {
            // 檢查現在時間是否已經超過了「下次可攻擊時間」
            if (Time.time >= nextAttackTime)
            {
                DoAttack();
            }
            else
            {
                Debug.Log("攻擊冷卻中...");
            }
        }
    }
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;
        Vector3 finalVelocity = Vector3.zero;

        if (inputDir.magnitude >= 0.1f)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0; camRight.y = 0;
            camForward.Normalize(); camRight.Normalize();

            Vector3 moveDir = (camForward * v + camRight * h);
            finalVelocity = moveDir * speed;

            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            _animator.SetFloat("Speed", speed);
        }
        else
        {
            _animator.SetFloat("Speed", 0f);
        }

        if (_gravityScript != null)
        {
            finalVelocity.y = _gravityScript.verticalVelocity;
        }

        _controller.Move(finalVelocity * Time.deltaTime);
    }
    void HandleWeaponSwitch()
    {
        if (_levelSystem == null) return;

        // 按下 1：切換到劍 (需 LV 10)
        if (Input.GetKeyDown(KeyCode.Alpha1) && _levelSystem.currentLevel >= 10)
        {
            EquipWeapon("Sword");
        }
        
        // 按下 2：切換到弓 (需 LV 20)
        if (Input.GetKeyDown(KeyCode.Alpha2) && _levelSystem.currentLevel >= 20)
        {
            EquipWeapon("Bow");
        }
    }

    void EquipWeapon(string weaponName)
    {
        if (weaponName == "Sword")
        {
            currentWeapon = "Sword";
            if (swordModel != null) swordModel.SetActive(true);
            if (bowModel != null) bowModel.SetActive(false);
            
            _animator.SetInteger("WeaponType", 1); // 告訴動畫師：現在拿劍
            playerDamage = 50f;
            playerAttackRange = 2f;
        }
        else if (weaponName == "Bow")
        {
            currentWeapon = "Bow";
            if (swordModel != null) swordModel.SetActive(false);
            if (bowModel != null) bowModel.SetActive(true);
            
            _animator.SetInteger("WeaponType", 2); // 告訴動畫師：現在拿弓
            playerDamage = 40f;
            playerAttackRange = 15f;
        }
        Debug.Log($"切換武器為：{weaponName}");
    }
    void DoAttack()
    {
        // 1. 根據武器設定冷卻時間
        float cooldown = (currentWeapon == "Bow") ? bowCooldown : swordCooldown;
        nextAttackTime = Time.time + cooldown; // 更新下次可攻擊的時間點

        // 2. 傷害計算邏輯 (維持不變)
        float multiplier = (_levelSystem != null) ? _levelSystem.attackMultiplier : 1.0f;
        float finalDamage = playerDamage * multiplier;

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, playerAttackRange);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Health eHealth = enemy.GetComponent<Health>();
                if (eHealth != null) eHealth.TakeDamage(finalDamage);
            }
        }
    }

    // 配合 LevelSystem 的等級來解鎖武器
    public void CheckWeaponUnlock(int currentLevel)
    {
        if (currentLevel >= 10 && currentWeapon == "None")
        {
            UnlockWeapon("Sword");
        }
        if (currentLevel >= 20 && currentWeapon != "Bow")
        {
            UnlockWeapon("Bow");
        }
    }

    void UnlockWeapon(string type)
    {
        currentWeapon = type;
        if (type == "Sword")
        {
            if (swordModel != null) swordModel.SetActive(true);
            if (bowModel != null) bowModel.SetActive(false);
            playerDamage = 50f;
            Debug.Log("解鎖劍：基礎傷害提升至 50");
        }
        else if (type == "Bow")
        {
            if (swordModel != null) swordModel.SetActive(false);
            if (bowModel != null) bowModel.SetActive(true);
            playerAttackRange = 15f;
            playerDamage = 40f;
            Debug.Log("解鎖弓：攻擊距離大幅提升");
        }
    }

    void Respawn()
    {
        Health health = GetComponent<Health>();
        if (health != null) health.ResetHealth();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (health.deathPanel != null) health.deathPanel.SetActive(false);

        _controller.enabled = false;
        transform.position = new Vector3(0, 5, 0); 
        _controller.enabled = true;

        _animator.Rebind();
        _animator.Update(0f);
    }
}