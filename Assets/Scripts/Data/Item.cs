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

    [Tooltip("Armor: damage reduction.")]
    public int defense;

    [Tooltip("Consumables: health restored on use. " +
             "(hunger/thirst/radiation restores can be added here later.)")]
    public int healthRestore;

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