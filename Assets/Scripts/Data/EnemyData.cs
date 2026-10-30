using UnityEngine;

// Shared design-time values. Runtime state such as current health belongs on the character.
[CreateAssetMenu(fileName = "EnemyData", menuName = "Dungeon/Data/Enemy")]
public sealed class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string displayName = "Enemy";
    public Sprite icon;
    public GameObject prefab;

    [Header("Stats")]
    [Min(1)] public int maxHealth = 100;
    [Min(0)] public int attackDamage = 10;
    [Min(0f)] public float patrolSpeed = 2f;
    [Min(0f)] public float chaseSpeed = 4f;

    [Header("Detection and attack")]
    [Min(0f)] public float detectRange = 6f;
    [Min(0f)] public float loseRange = 9f;
    [Min(0f)] public float attackRange = 1.2f;
    [Min(0f)] public float attackRadius = 0.7f;
    [Min(0f)] public float windupTime = 0.35f;
    [Min(0f)] public float recoverTime = 0.5f;
    [Min(0f)] public float attackCooldown = 1f;
    [Min(0f)] public float knockbackForce = 7f;
    [Min(0f)] public float hurtTime = 0.3f;
}
