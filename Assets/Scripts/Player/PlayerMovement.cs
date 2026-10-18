using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] PlayerInputReader input;
    [SerializeField] Transform groundCheck;
    [SerializeField] Vector2 groundCheckSize = new(0.4f, 0.06f);
    [SerializeField] LayerMask groundLayer;

    [Header("Move")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float acceleration = 90f;

    [Header("Jump")]
    [SerializeField] float jumpSpeed = 15f;
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBufferTime = 0.1f;

    Rigidbody2D _rb;
    float _coyote, _jumpBuffer;
    bool _cutJump;
    float _stunUntil;
    Vector3 _baseScale;
    public int FacingDirection { get; private set; } = 1;
    public bool IsGrounded { get; private set; }
    public Vector2 Velocity => _rb != null ? _rb.linearVelocity : Vector2.zero;

    void Awake()
{
    _rb = GetComponent<Rigidbody2D>();
    _baseScale = transform.localScale; // nhớ Scale bạn đã đặt trong Editor
}

    void OnEnable()
    {
        input.JumpPressed += OnJumpPressed;
        input.JumpReleased += OnJumpReleased;
    }
    void OnDisable()
    {
        input.JumpPressed -= OnJumpPressed;
        input.JumpReleased -= OnJumpReleased;
    }

    void OnJumpPressed()  => _jumpBuffer = jumpBufferTime;
    void OnJumpReleased() => _cutJump = true;
    public void Stun(float duration) => _stunUntil = Time.time + duration;
    void FixedUpdate()
    {
        if (Time.time < _stunUntil) return;
        float dt = Time.fixedDeltaTime;
        Vector2 v = _rb.linearVelocity; // Unity 2022: dùng _rb.velocity

        bool grounded = v.y <= 0.01f &&
            Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        IsGrounded = grounded;

        _coyote = grounded ? coyoteTime : _coyote - dt;
        _jumpBuffer -= dt;

        // Ngang
        v.x = Mathf.MoveTowards(v.x, input.Move * moveSpeed, acceleration * dt);

        // Nhảy
        if (_jumpBuffer > 0f && _coyote > 0f)
        {
            v.y = jumpSpeed;
            _jumpBuffer = 0f;
            _coyote = 0f;
        }
        if (_cutJump && v.y > 0f) v.y *= 0.5f;
        _cutJump = false;

        _rb.linearVelocity = v;

        // Quay mặt
        if (Mathf.Abs(input.Move) > 0.01f)
        {
            FacingDirection = input.Move > 0 ? 1 : -1;
            transform.localScale = new Vector3(
                 Mathf.Abs(_baseScale.x) * FacingDirection,
                _baseScale.y,
                _baseScale.z);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}