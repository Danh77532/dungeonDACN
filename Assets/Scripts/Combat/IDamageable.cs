using UnityEngine;

public struct DamageInfo
{
    public int Amount;
    public Vector2 Knockback;
    public GameObject Source;
}

public interface IDamageable
{
    void TakeDamage(DamageInfo info);
}