using UnityEngine;
using System.Collections.Generic;

// ============================================================================
//  Item.cs    THE ITEM CONTRACT  (sprint 1)
// ----------------------------------------------------------------------------
//  This is the ONE shared definition of what an "item" is. Inventory, the
//  shop, combat, and save/load all build against THIS file. Don't make your
//  own version of an item anywhere else. If this is missing a field you need,
//  say so and we change it here, once, for everyone.
//
//  An Item is a ScriptableObject: a reusable data asset that lives in the
//  Project, not in a scene. To make a new item in Unity:
//      Right-click in the Project window > Create > Afterfall > Item
//  ...then fill in the fields in the Inspector. No code needed to add items.
//
// ============================================================================

public enum ItemType
{
    Weapon,
    Armor,
    Consumable,   // food, potions, anything used up on use
    QuestItem,
    Misc
}

public enum WeaponAmmoType
{
    None,   // melee or anything that doesn't use the ammo HUD
    Pistol,
    Shotgun,
    Rifle
}

public enum WeaponCategory
{
    Ranged,   // fires a projectile - uses ammoType/ammoCapacity
    Melee     // swings in the direction the player is facing - ignores ammo
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Afterfall/Item")]
public class Item : ScriptableObject
{
    // --- Identity -----------------------------------------------------------
    [Header("Identity")]
    [Tooltip("Unique id, never shown to the player. Used by save/load. " +
             "e.g. \"weapon_pipe_rifle\". Lowercase, no spaces, no duplicates.")]
    public string id;

    [Tooltip("The name the player sees, e.g. \"Pipe Rifle\".")]
    public string displayName;

    [TextArea(2, 4)]
    [Tooltip("Flavor / description text shown when the item is selected.")]
    public string description;

    [Tooltip("The picture shown in inventory and shop slots.")]
    public Sprite icon;

    [Tooltip("What kind of item this is. Decides which combat fields below matter.")]
    public ItemType itemType;

    // --- Stacking -----------------------------------------------------------
    [Header("Stacking")]
    [Tooltip("Can multiples share one slot? (ammo/food yes, a unique sword no)")]
    public bool stackable;

    [Tooltip("If stackable, how many fit in one slot. Ignored when not stackable.")]
    public int maxStackSize = 1;

    // --- Economy ------------------------------------------------------------
    [Header("Economy")]
    [Tooltip("Base buy/sell price at shops and traders.")]
    public int value;

    // --- Combat / Equipment -------------------------------------------------
    //  Only the field that matches itemType is used:
    //    Weapon     -> damage
    //    Armor      -> defense
    //    Consumable -> healthRestore
    //  Flat fields on one class is deliberate: it's far easier for everyone to
    //  author items in the Inspector than to juggle separate weapon/armor asset
    //  types. Can split into subclasses later if we ever actually need to.
    [Header("Combat / Equipment")]
    [Tooltip("Weapons: damage per hit.")]
    public int damage;

    [Tooltip("Weapons: Ranged fires a projectile (uses the fields below). Melee swings in the direction " +
             "the player is facing and ignores ammo entirely.")]
    public WeaponCategory weaponCategory = WeaponCategory.Ranged;

    [Tooltip("Weapons: which ammo icon set the HUD shows while this weapon is equipped. Only used when " +
             "Weapon Category is Ranged - leave None for melee weapons.")]
    public WeaponAmmoType ammoType;

    [Tooltip("Weapons: magazine size — how many ammo icons the HUD draws for this weapon.")]
    public int ammoCapacity = 6;

    [Tooltip("Melee weapons: how far in front of the player the swing reaches.")]
    public float meleeRange = 1f;

    [Tooltip("Melee weapons: seconds between swings, before the Melee skill's speed bonus.")]
    public float meleeAttackCooldown = 0.5f;

    [Tooltip("Weapons: sound played when this weapon actually connects - a melee swing hits an enemy, " +
             "or a projectile impacts one. Not played on a swing/shot that misses.")]
    public AudioClip hitSound;

    [Tooltip("Armor: damage reduction.")]
    public int defense;

    [Tooltip("Consumables: health restored on use. " +
             "(hunger/thirst/radiation restores can be added here later.)")]
    public int healthRestore;

    [Tooltip("Weapons: sprite drawn over the player when this weapon is equipped, facing down.")]
    public Sprite equippedSpriteDown;

    [Tooltip("Weapons: sprite drawn over the player when this weapon is equipped, facing up.")]
    public Sprite equippedSpriteUp;

    [Tooltip("Weapons: sprite drawn over the player when this weapon is equipped, facing left.")]
    public Sprite equippedSpriteLeft;

    [Tooltip("Weapons: sprite drawn over the player when this weapon is equipped, facing right.")]
    public Sprite equippedSpriteRight;

    [Tooltip("Weapons: position offset for the equipped sprite while facing down. Different weapon art " +
             "doesn't line up the same, so this is tuned per weapon rather than shared.")]
    public Vector2 equippedOffsetDown;

    [Tooltip("Weapons: position offset for the equipped sprite while facing up.")]
    public Vector2 equippedOffsetUp;

    [Tooltip("Weapons: position offset for the equipped sprite while facing left.")]
    public Vector2 equippedOffsetLeft;

    [Tooltip("Weapons: position offset for the equipped sprite while facing right.")]
    public Vector2 equippedOffsetRight;

    [Tooltip("Weapons: extra frames played over the idle sprite while attacking (firing/swinging), facing " +
             "down. Leave empty to just hold the idle sprite while attacking.")]
    public Sprite[] attackFramesDown;

    [Tooltip("Weapons: extra frames played over the idle sprite while attacking (firing/swinging), facing up.")]
    public Sprite[] attackFramesUp;

    [Tooltip("Weapons: extra frames played over the idle sprite while attacking (firing/swinging), facing left.")]
    public Sprite[] attackFramesLeft;

    [Tooltip("Weapons: extra frames played over the idle sprite while attacking (firing/swinging), facing right.")]
    public Sprite[] attackFramesRight;

    [Tooltip("Weapons: playback speed (frames per second) for the attack frames above.")]
    public float attackFrameRate = 12f;

    // --- Stat modifiers (optional, hooks into the stat model) ---------------

    [Header("Stat Modifiers (optional)")]
    [Tooltip("Bonuses granted while equipped. Leave empty unless the item buffs a stat.")]
    public List<StatModifier> statModifiers = new List<StatModifier>();
}

// ============================================================================
//  Supporting types

public enum StatType
{
    Strength,
    Perception,
    Endurance,
    Charisma,
    Intelligence,
    Luck,
    Dexterity,
    Health
}

[System.Serializable]
public struct StatModifier
{
    public StatType stat;
    public int amount;
}

// Shared by anything that hands the player a random quantity of an item - chest contents and
// enemy loot tables (Assets/Scripts/Items/ChestInventory.cs, Assets/Scripts/Enemies/EnemyDefinition.cs).
[System.Serializable]
public struct ItemDrop
{
    public Item item;
    [Min(1)] public int minAmount;
    [Min(1)] public int maxAmount;

    public int RollAmount()
    {
        var min = Mathf.Max(1, minAmount);
        var max = Mathf.Max(min, maxAmount);
        return Random.Range(min, max + 1);
    }
}