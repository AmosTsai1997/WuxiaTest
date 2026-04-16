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
    }
    public void SetJumpVelocity(float jumpHeight)
    {
        verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity); 
    }
}