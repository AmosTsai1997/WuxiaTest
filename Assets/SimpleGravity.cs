using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleGravity : MonoBehaviour
{
    private CharacterController _controller;
    public float verticalVelocity;
    public float gravity = 9.81f;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (_controller.isGrounded)
        {
            verticalVelocity = -0.5f; // 貼地微推力
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime; // 模擬自由落體
        }
        // 改變位置
        _controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }
    public void SetJumpVelocity(float jumpHeight)
    {
        verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity); 
    }
}