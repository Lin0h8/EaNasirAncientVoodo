using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform _player;
    public Transform _camera;
    public float _playerSpeed = 10f;
    public float JumpForce = 2.0f;
    public Vector3 Jump = new Vector3(0, 2.0f, 0);
    public Rigidbody rb;
    public bool IsGrounded = false;
    public Transform RaycastOrigin;
    void Start()
    {
        _player = transform;
        
        rb = GetComponent<Rigidbody>();
        
    }
    void OnCollisionEnter()
    {
        IsGrounded = true;
    }
    void OnCollisionExit()
    {
        IsGrounded = false;
    }
    // Update is called once per frame
    void Update()
    {
        PlayerInput();
        JumpLogic();
    }
    void PlayerInput()
    {
        Vector3 Movement = Vector3.zero;
        float y = rb.linearVelocity.y;
        rb.linearVelocity = new Vector3(0, y, 0);
        
        float MoveX = 0f;
        float MoveZ = 0f;
        Vector3 moveDir = new Vector3();
        Vector3 horizontalMovement = new Vector3();
        Vector3 verticalMovement = new Vector3();
        
            MoveX = Input.GetAxis("Horizontal");
            horizontalMovement = transform.right * MoveX;
        Debug.Log(MoveX);
        
            MoveZ = Input.GetAxis("Vertical");
            verticalMovement = transform.forward * MoveZ;
        Debug.Log(MoveZ);
        moveDir = horizontalMovement + verticalMovement;
        moveDir.y = 0;
        moveDir.Normalize();
        moveDir *= _playerSpeed;

        Vector3 MoveTo = new Vector3(MoveX, 0, MoveZ);
        
            //rb.MoveRotation(new Quaternion(0, _camera.localRotation.y , 0,0));


            //rb.AddForce(moveDir * _playerSpeed / 20, ForceMode.VelocityChange);
            if (moveDir.magnitude > 0.01f)
            {
                rb.linearVelocity = new Vector3(moveDir.x, rb.linearVelocity.y, moveDir.z);
                
            }
           
            

        
    }
    
        
    bool CheckIfGrounded()
    {

        return Physics.Raycast(RaycastOrigin.position, Vector3.down, 1, LayerMask.GetMask("Ground"));
    }
    void JumpLogic()
    {
        IsGrounded = CheckIfGrounded();
        if (IsGrounded && Input.GetButton("Jump"))
        {
            rb.AddForce(Jump, ForceMode.Impulse);
        }
    }
}
