using UnityEngine;

public class KeepObject : MonoBehaviour
{
    void Awake()
    {
        // 告訴 Unity：切換場景時不要毀滅我
        DontDestroyOnLoad(gameObject);
    }
}