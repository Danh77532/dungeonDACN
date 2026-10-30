using UnityEngine;

// Base values for a new character, not mutable values for the current run.
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Dungeon/Data/Player Stats")]
public sealed class PlayerStats : ScriptableObject
{
    [Header("Survival")]
    [Min(1)] public int maxHealth = 100;
    [Min(0)] public int armor;
    [Min(0f)] public float maxStamina = 100f;
    [Min(0f)] public float maxEnergy = 100f;

    [Header("Combat")]
    [Min(0)] public int damage = 10;
    [Min(0f)] public float attackCooldown = 0.35f;
    [Range(0f, 1f)] public float criticalChance = 0.05f;

    [Header("Movement")]
    [Min(0f)] public float moveSpeed = 8f;
    [Min(0f)] public float acceleration = 90f;
    [Min(0f)] public float jumpSpeed = 15f;
}
