using System.Collections;
using UnityEngine;

public enum EnemyState { Patrol, Chase, Attack, Hurt, Dead }

[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [Header("Tham chiếu")]
    [SerializeField] Transform wallCheck;
    [SerializeField] Transform ledgeCheck;
    [SerializeField] Transform attackPoint;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask playerLayer;

    [Header("Di chuyển")]
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float chaseSpeed = 4f;

    [Header("Phát hiện")]
    [SerializeField] float detectRange = 6f;   // thấy Player trong tầm này thì đuổi
    [SerializeField] float loseRange = 9f;     // Player chạy xa hơn thì bỏ cuộc
    [SerializeField] float attackRange = 1.2f; // vào tầm này thì đánh

    [Header("Tấn công")]
    [SerializeField] int attackDamage = 10;
    [SerializeField] float attackRadius = 0.7f;
    [SerializeField] float windupTime = 0.35f;
    [SerializeField] float recoverTime = 0.5f;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float knockbackForce = 7f;

    [Header("Bị đánh")]
    [SerializeField] float hurtTime = 0.3f;

    [Header("Chỉ để xem khi Play")]
    [SerializeField] EnemyState _state = EnemyState.Patrol;

    Rigidbody2D _rb;
    Health _health;
    Transform _target;
    Coroutine _attackRoutine;
    int _facing = 1;
    float _hurtTimer;
    float _nextAttackTime;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        _health.Damaged += OnDamaged;
        _health.Died += OnDied;
    }

    void OnDisable()
    {
        _health.Damaged -= OnDamaged;
        _health.Died -= OnDied;
    }

    void FixedUpdate()
    {
        switch (_state)
        {
            case EnemyState.Patrol: DoPatrol(); break;
            case EnemyState.Chase:  DoChase();  break;
            case EnemyState.Attack: Move(0f);   break;
            case EnemyState.Hurt:   DoHurt();   break;
            case EnemyState.Dead:   break;
        }
    }

    // ---------- Các trạng thái ----------

    void DoPatrol()
    {
        if (FindPlayer()) { _state = EnemyState.Chase; return; }

        if (IsBlocked()) Flip(-_facing);
        Move(patrolSpeed * _facing);
    }

    void DoChase()
    {
        if (_target == null || !IsTargetAlive() ||
            Vector2.Distance(transform.position, _target.position) > loseRange)
        {
            _target = null;
            _state = EnemyState.Patrol;
            return;
        }

        float dx = _target.position.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.1f && Mathf.Sign(dx) != _facing) Flip((int)Mathf.Sign(dx));

        if (Mathf.Abs(dx) <= attackRange)
        {
            Move(0f);
            if (Time.time >= _nextAttackTime)
                _attackRoutine = StartCoroutine(AttackRoutine());
            return;
        }

        if (IsBlocked()) Move(0f);
        else Move(chaseSpeed * _facing);
    }

    void DoHurt()
    {
        _hurtTimer -= Time.fixedDeltaTime;
        if (_hurtTimer <= 0f)
            _state = _target != null ? EnemyState.Chase : EnemyState.Patrol;
    }

    IEnumerator AttackRoutine()
    {
        _state = EnemyState.Attack;

        sprite.color = Color.yellow;               // báo hiệu cho người chơi né
        yield return new WaitForSeconds(windupTime);
        sprite.color = Color.white;

        var hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out IDamageable target)) continue;
            float dir = Mathf.Sign(hit.transform.position.x - transform.position.x);
            target.TakeDamage(new DamageInfo
            {
                Amount = attackDamage,
                Knockback = new Vector2(dir * knockbackForce, 4f),
                Source = gameObject
            });
        }

        yield return new WaitForSeconds(recoverTime);
        _nextAttackTime = Time.time + attackCooldown;
        _attackRoutine = null;
        _state = EnemyState.Chase;
    }

    // ---------- Sự kiện từ Health ----------

    void OnDamaged(DamageInfo info)
    {
        if (_state == EnemyState.Dead) return;

        if (_attackRoutine != null) { StopCoroutine(_attackRoutine); _attackRoutine = null; }
        if (info.Source != null) _target = info.Source.transform; // bị đánh thì quay lại đuổi

        _rb.linearVelocity = info.Knockback; // Unity 2022: dùng velocity
        _hurtTimer = hurtTime;
        _state = EnemyState.Hurt;
        StartCoroutine(Flash());
    }

    void OnDied()
    {
        _state = EnemyState.Dead;
        Destroy(gameObject);
    }

    IEnumerator Flash()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color = Color.white;
    }

    // ---------- Hàm phụ ----------

    bool FindPlayer()
    {
        var hit = Physics2D.OverlapCircle(transform.position, detectRange, playerLayer);
        if (hit == null) return false;
        _target = hit.transform;
        return IsTargetAlive();
    }

    bool IsTargetAlive()
    {
        return _target != null &&
               _target.TryGetComponent(out Health h) && h.Current > 0;
    }

    bool IsBlocked()
    {
        bool wall = Physics2D.OverlapCircle(wallCheck.position, 0.1f, groundLayer);
        bool ground = Physics2D.Raycast(ledgeCheck.position, Vector2.down, 1f, groundLayer);
        return wall || !ground;
    }

    void Move(float x) => _rb.linearVelocity = new Vector2(x, _rb.linearVelocity.y);

    void Flip(int dir)
    {
        _facing = dir;
        transform.localScale = new Vector3(dir, 1f, 1f); // các child tự quay theo
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseRange);
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + Vector3.down);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(wallCheck.position, 0.1f);
        }
    }
}