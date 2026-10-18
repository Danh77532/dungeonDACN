using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
public class DummyEnemy : MonoBehaviour
{
    [SerializeField] SpriteRenderer sprite;

    Health _health;
    Rigidbody2D _rb;

    void Awake()
    {
        _health = GetComponent<Health>();
        _rb = GetComponent<Rigidbody2D>();
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

    void OnDamaged(DamageInfo info)
    {
        _rb.linearVelocity = info.Knockback; // Unity 2022: dùng _rb.velocity
        StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color = Color.white;
    }

    void OnDied() => Destroy(gameObject);
}