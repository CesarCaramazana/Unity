using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]

public class FirstPersonController : MonoBehaviour
{
    [Header("Player")]

    public float MoveSpeed = 4.0f;
    public float SprintSpeed = 6.0f;
    public float SpeedChangeRate = 10.0f;

    [Space(10)]
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;
    public int numberOfExtraJumps = 2;
    public int remainingJumps;

    public float JumpTimeout = 0.1f;
    public float FallTimeout = 0.15f;

    [Header("Grounded")]
    public bool Grounded = true;
    public float GroundedOffset = -0.2f;
    public float GroundedRadius = 0.5f;
    public LayerMask GroundLayers;

    [Header("Cinemachine camera")]
    public GameObject cameraTarget;
    public float xClamp = 85.0f;
    public float sensivityX = 1.0f;
    public float sensivityY = 1.0f;

    private float _cameraPitch;
    private float _speed;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity;

    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;


    private PlayerInput _playerInput;
    private CharacterController _controller;
    private InputManager _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<InputManager>();
        _playerInput = GetComponent<PlayerInput>();


        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
        
    }

    // Update is called once per frame
    void Update()
    {
        JumpAndGravity();
        //DoubleJumpAndGravity();
        GroundedCheck();
        Move();

        Debug.Log("Input jump "+ _input.jump);
    }
    private void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        if (_input.look.sqrMagnitude >= _threshold)
        {
            //Don't multiply mouse input by Time.deltaTime
            float deltaTimeMultiplier = 1.0f;

            _cameraPitch += _input.look.y * sensivityX * deltaTimeMultiplier;
            _rotationVelocity = _input.look.x * sensivityY * deltaTimeMultiplier;

            // clamp our pitch rotation
            _cameraPitch = Mathf.Clamp(_cameraPitch, -xClamp, xClamp);

            // Update Cinemachine camera target pitch
            cameraTarget.transform.localRotation = Quaternion.Euler(_cameraPitch, 0.0f, 0.0f);

            // rotate the player left and right
            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    private void JumpAndGravity()
    {

        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                Debug.Log("On update _input.jump = " + _input.jump);
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }

            // if we are not grounded, do not jump
            _input.jump = false;


        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void DoubleJumpAndGravity()
    {
        //If on the ground, we can jump
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;


            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            //Reset remaining jumps
            remainingJumps = numberOfExtraJumps;

            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                _input.jump = false;
                _jumpTimeoutDelta = JumpTimeout;

            }

        }

        //If not on the ground, we can jump if we haven't double jumped
        else // If !Grounded
        {

            if (remainingJumps > 0)
            {
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    remainingJumps--;

                    _input.jump = false;

                    _jumpTimeoutDelta = JumpTimeout;
                }
            }
            else
            {
                _input.jump = false;
            }

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
        }

        // jump timeout
        if (_jumpTimeoutDelta >= 0.0f)
        {
            _jumpTimeoutDelta -= Time.deltaTime;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }

    }

    private void Move()
    {
        // Set speed based on sprint
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;

        float inputMagnitude = 1.0f; // This is to differenciate between digital (keyboard) and analog (joystick) input

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }


        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            // Get direction
            inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
        }


        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);


    }
}
