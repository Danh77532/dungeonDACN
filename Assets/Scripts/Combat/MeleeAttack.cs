using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [SerializeField] PlayerInputReader input;
    [SerializeField] Transform attackPoint;
    [SerializeField] float radius = 0.6f;
    [SerializeField] int damage = 10;
    [SerializeField] float knockbackForce = 6f;
    [SerializeField] float cooldown = 0.35f;
    [SerializeField] LayerMask targetLayers;

    float _nextAttackTime;
    public event System.Action OnAttack;

    void OnEnable()  => input.AttackPressed += Attack;
    void OnDisable() => input.AttackPressed -= Attack;

    void Attack()
    {
        if (Time.time < _nextAttackTime) return;
        _nextAttackTime = Time.time + cooldown;
        OnAttack?.Invoke();

        var hits = Physics2D.OverlapCircleAll(attackPoint.position, radius, targetLayers);
        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out IDamageable target)) continue;

            float dir = Mathf.Sign(hit.transform.position.x - transform.position.x);
            target.TakeDamage(new DamageInfo
            {
                Amount = damage,
                Knockback = new Vector2(dir * knockbackForce, 3f),
                Source = gameObject
            });
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, radius);
    }
}