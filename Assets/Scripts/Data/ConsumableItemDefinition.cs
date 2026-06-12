using UnityEngine;

namespace CIS2991Project.Data
{
    [CreateAssetMenu(menuName = "CIS2991/Items/Consumable", fileName = "NewConsumableItem")]
    public class ConsumableItemDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Bandage";
        [SerializeField, TextArea] private string description = "Restores a small amount of health.";
        [SerializeField] private int healAmount = 10;

        public string DisplayName => displayName;
        public string Description => description;
        public int HealAmount => healAmount;
    }
}
