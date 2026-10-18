using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
public class PlayerReaction : MonoBehaviour
{
    [SerializeField] PlayerMovement movement;
    [SerializeField] MeleeAttack melee;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] float stunTime = 0.25f;

    Health _health;
    Rigidbody2D _rb;

    void Awake()
    {
        _health = GetComponent<Health>();
        _rb = GetComponent<Rigidbody2D>();
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
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
        Debug.Log($"Player còn {_health.Current}/{_health.Max} máu");
        movement.Stun(stunTime);
        _rb.linearVelocity = info.Knockback; // Unity 2022: dùng velocity
        StartCoroutine(Blink(_health.InvincibleTime));
    }

    IEnumerator Blink(float duration)
    {
        float end = Time.time + duration;
        while (Time.time < end)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        sprite.enabled = true;
    }

    void OnDied()
    {
        Debug.Log("Player đã chết");
        StopAllCoroutines();
        sprite.enabled = true;
        sprite.color = Color.gray;
        movement.enabled = false;
        melee.enabled = false;
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
    }
}