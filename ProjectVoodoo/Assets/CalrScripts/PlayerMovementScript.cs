using UnityEngine;
using UnityEngine.UI;

public class PlayerMovementScrript : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 7f;
    public float gravity = -19.62f;
    public float jumpHeight = 2f;
    public float airControlMultiplier = 0.01f;

    public Transform groundcheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    Vector3 lastMoveDirection;

    public bool isGrounded;

    void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
    }

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundcheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = transform.right * x + transform.forward * z;

        if (isGrounded)
        {
            lastMoveDirection = inputDirection.normalized;
        }
        else
        {
            if (inputDirection.magnitude > 0.1f)
            {
                lastMoveDirection = Vector3.Lerp(
                    lastMoveDirection,
                    inputDirection.normalized,
                    airControlMultiplier * Time.deltaTime * 10f);
            }
            else
            {
                lastMoveDirection = Vector3.Lerp(
                    lastMoveDirection,
                    Vector3.zero,
                    airControlMultiplier * Time.deltaTime * 5f);
            }
        }

        Vector3 horizontalMove = lastMoveDirection * speed;

        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = horizontalMove + Vector3.up * velocity.y;

        controller.Move(finalMove * Time.deltaTime);
    }
}
