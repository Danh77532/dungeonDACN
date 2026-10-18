using UnityEngine;

public class ContactDamage : MonoBehaviour
{
    [SerializeField] int damage = 10;
    [SerializeField] float knockbackForce = 8f;
    [SerializeField] LayerMask targetLayers;

    void OnCollisionStay2D(Collision2D other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        var target = other.collider.GetComponentInParent<IDamageable>();
        if (target == null) return;

        float dir = Mathf.Sign(other.transform.position.x - transform.position.x);
        target.TakeDamage(new DamageInfo
        {
            Amount = damage,
            Knockback = new Vector2(dir * knockbackForce, 5f),
            Source = gameObject
        });
    }
}