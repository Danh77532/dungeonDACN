using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] MeleeAttack melee;

    PlayerMovement _movement;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    static readonly int YVelocityHash = Animator.StringToHash("yVelocity");
    static readonly int AttackHash = Animator.StringToHash("Attack");

    void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (melee == null) melee = GetComponent<MeleeAttack>();
    }

    void OnEnable()
    {
        if (melee != null) melee.OnAttack += TriggerAttack;
    }

    void OnDisable()
    {
        if (melee != null) melee.OnAttack -= TriggerAttack;
    }

    void Update()
    {
        if (animator == null) return;

        // Vận tốc ngang (dùng cho Walk / Run)
        float speed = Mathf.Abs(_movement.Velocity.x);
        animator.SetFloat(SpeedHash, speed);

        // Trạng thái chạm đất
        animator.SetBool(IsGroundedHash, _movement.IsGrounded);

        // Vận tốc dọc (dùng để phân biệt Jump và Fall)
        animator.SetFloat(YVelocityHash, _movement.Velocity.y);
    }

    void TriggerAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger(AttackHash);
        }
    }
}
