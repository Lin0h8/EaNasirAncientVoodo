using UnityEngine;
using UnityEngine.UI;

public class PlayerMovementScrript : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 7f;
    public float gravity = -19.62f;
    public float jumpHeight = 2f;

    public Transform groundcheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
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

        if (Input.GetButton("Jump") && isGrounded)
        {
            Jump();
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
