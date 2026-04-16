using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;        
    public Vector3 offsetDirection = new Vector3(0, 1.5f, -1f); // 偏移的方向向量
    public float sensitivity = 5f;  
    
    [Header("Zoom Settings")]
    public float distance = 7f;         // 當前距離
    public float minDistance = 2f;      // 最近距離
    public float maxDistance = 15f;     // 最遠距離
    public float zoomSpeed = 200f;      // 縮放靈敏度

    private float _currentX = 0f;
    private float _currentY = 20f;  

    void LateUpdate() 
    {
        if (target == null) return;

        // 1. 處理滑鼠右鍵旋轉
        if (Input.GetMouseButton(1))
        {
            _currentX += Input.GetAxis("Mouse X") * sensitivity;
            _currentY -= Input.GetAxis("Mouse Y") * sensitivity;
            _currentY = Mathf.Clamp(_currentY, 10f, 80f);
        }

        // 2. 處理滾輪縮放 (加上平滑處理)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // 滾輪往上是正，距離要變小；滾輪往下是負，距離要變大
            distance -= scroll * zoomSpeed * Time.deltaTime;
            // 限制縮放範圍，防止穿過大俠的身體或退到外太空
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // 3. 計算旋轉與最終位置
        Quaternion rotation = Quaternion.Euler(_currentY, _currentX, 0);
        
        // 最終位置 = 目標位置 + (旋轉矩陣 * 方向向量 * 縮放距離)
        Vector3 finalPosition = target.position + rotation * (offsetDirection.normalized * distance);

        transform.position = finalPosition;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}