using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("開門設定")]
    public float openAngle = 90f; // 打開的角度，負數就是往另一邊開
    public float speed = 3f;      // 開關速度

    private Quaternion _closedRot; // 關門時的角度
    private Quaternion _openRot;   // 開門時的角度
    private bool _isOpen = false;

    void Start()
    {
        // 記住初始角度當作「關門狀態」
        _closedRot = transform.localRotation;

        // 開門狀態 = 關門角度再多轉 openAngle 度（繞 Y 軸）
        _openRot = _closedRot * Quaternion.Euler(0f, openAngle, 0f);
    }

    void Update()
    {
        // 目標角度：開著就朝 _openRot 轉，關著就朝 _closedRot 轉
        Quaternion target = _isOpen ? _openRot : _closedRot;

        // Slerp = 每一幀朝目標「滑」過去一小段，做出平滑的開關動畫。
        // 你的 NPCInteraction 鏡頭轉向用的就是同一招
        transform.localRotation = Quaternion.Slerp(transform.localRotation, target, speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) _isOpen = true; // 玩家靠近 → 開
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) _isOpen = false; // 玩家走遠 → 自動關上
    }
}