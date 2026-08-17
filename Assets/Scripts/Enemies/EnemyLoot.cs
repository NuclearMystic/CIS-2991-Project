using CIS2991Project.Core;
using CIS2991Project.Items;
using UnityEngine;

namespace CIS2991Project.Enemies
{
    // Rolls and spawns a random loot pickup from an EnemyDefinition's loot table.
    public static class EnemyLoot
    {
        public static void Spawn(EnemyDefinition definition, Vector3 position)
        {
            if (definition.lootTable == null || definition.lootTable.Length == 0)
                return;

            var drop = definition.lootTable[Random.Range(0, definition.lootTable.Length)];
            if (drop.item == null)
                return;

            var loot = new GameObject($"Loot_{drop.item.displayName}");
            loot.transform.position = position;

            var renderer = loot.AddComponent<SpriteRenderer>();
            renderer.sprite = definition.lootSprite != null ? definition.lootSprite : RuntimeSpriteUtils.CreateCircleSprite(Color.yellow);
            renderer.sortingOrder = 5;

            loot.AddComponent<CircleCollider2D>().isTrigger = true;
            loot.AddComponent<ConsumablePickup>().Configure(drop.item, drop.RollAmount(), definition.lootDropSound);
        }
    }
}
