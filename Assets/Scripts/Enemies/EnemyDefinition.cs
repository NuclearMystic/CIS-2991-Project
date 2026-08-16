using UnityEngine;

namespace CIS2991Project.Enemies
{
    public enum EnemyAttackType
    {
        Melee,
        Ranged
    }

    // Shared stats for one kind of enemy (Zombie/Raider/Mutant/...). Create one asset per enemy
    // type (right-click in Project > Create > Afterfall > Enemy) and assign it to the Enemy
    // component on that NPC's prefab. Multiple NPCs can share the same definition.
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Afterfall/Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string displayName;

        [Header("Health & Reward")]
        [Min(1)] public int maxHealth = 10;
        [Min(0)] public int expReward = 15;

        [Header("Movement")]
        [Min(0f)] public float patrolSpeed = 1.5f;
        [Min(0f)] public float chaseSpeed = 3f;

        [Header("Detection & Range")]
        [Tooltip("Player must be within this distance for the enemy to notice them and start chasing.")]
        [Min(0f)] public float sightRange = 6f;
        [Tooltip("Enemy stops closing in and attacks once the player is this close (melee reach, or the range it holds at to shoot).")]
        [Min(0.1f)] public float attackRange = 1f;

        [Header("Attack")]
        public EnemyAttackType attackType = EnemyAttackType.Melee;
        [Min(0)] public int attackDamage = 1;
        [Tooltip("Seconds between attacks once the player is in range.")]
        [Min(0.05f)] public float attackCooldown = 1f;

        [Header("Ranged Attack — only used when Attack Type is Ranged")]
        public Sprite projectileSprite;
        [Min(0f)] public float projectileSpeed = 8f;
        [Min(0f)] public float projectileLifetime = 3f;

        [Header("Loot — one random entry from this list drops on death, in a rolled amount " +
                "(leave empty for no loot)")]
        public global::ItemDrop[] lootTable;
        public Sprite lootSprite;
        public AudioClip lootDropSound;
    }
}
