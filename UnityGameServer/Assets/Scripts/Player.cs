using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public CharacterController controller;
    public float gravity = -9.81f;
    public float jumpSpeed = 5f;
    public float moveSpeed = 500f;

    private bool[] _inputs;
    private float _yVelocity = 0;

    public void Initialize(int _id, string _username)
    {
        this.id = _id;
        this.username = _username;


        _inputs = new bool[5];
    }

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void FixedUpdate()
    {
        Vector2 inputDirection = Vector2.zero;

        if (_inputs[0])
        {
            inputDirection.y += 1;
        }

        if (_inputs[1])
        {
            inputDirection.y -= 1;
        }

        if (_inputs[2])
        {
            inputDirection.x -= 1;
        }

        if (_inputs[3])
        {
            inputDirection.x += 1;
        }

     
        Move(inputDirection);
    }


    private void Move(Vector2 inputDirection)
    {
        Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
        moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            _yVelocity = 0f;
            if (_inputs[4])
            {
                _yVelocity = jumpSpeed;
            }
        }

        _yVelocity += gravity;

        moveDirection.y = _yVelocity;
        
        controller.Move(moveDirection);
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }


    /// <summary>Updates the player input with newly received input.</summary>
    /// <param name="_inputs">The new key inputs.</param>
    /// <param name="_rotation">The new rotation.</param>
    public void SetInput(bool[] inputs, Quaternion _rotation)
    {
        this._inputs = inputs;
        transform.rotation = _rotation;
    }
}