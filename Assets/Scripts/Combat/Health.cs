using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHealth = 100;
    [SerializeField] PlayerStats playerStats; public PlayerStats BaseStats => playerStats;
    [SerializeField] float invincibleTime = 0f; // 0 = không có i-frame

    float _invincibleUntil;

    public int Current { get; private set; }
    public int Max => maxHealth;
    public float InvincibleTime => invincibleTime;

    public event Action<int, int> Changed;
    public event Action<DamageInfo> Damaged;
    public event Action Died;

    void Awake() { if (playerStats != null) maxHealth = Mathf.Max(1, playerStats.maxHealth); Current = maxHealth; }

#if UNITY_EDITOR
    [ContextMenu("Test lethal damage")]
    void TestLethalDamage() { if (Application.isPlaying) TakeDamage(new DamageInfo { Amount = Current }); }
#endif
    public void TakeDamage(DamageInfo info)
    {
        if (Current <= 0) return;
        if (Time.time < _invincibleUntil) return;

        _invincibleUntil = Time.time + invincibleTime;
        Current = Mathf.Max(0, Current - info.Amount);
        Changed?.Invoke(Current, maxHealth);
        Damaged?.Invoke(info);
        if (Current == 0) Died?.Invoke();
    }
}